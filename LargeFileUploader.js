/**
 * Class quản lý upload file siêu lớn (50-100GB)
 * Tương thích với Backend C# Azure Storage
 */
class LargeFileUploader {
    constructor(file, options = {}) {
        this.file = file;
        this.options = {
            chunkSize: 20 * 1024 * 1024, // 20MB (Khuyến nghị cho Azure Block Blob)
            concurrency: 6,              // Số luồng song song (Tận dụng HTTP/2)
            maxRetries: 3,               // Số lần thử lại nếu lỗi mạng
            apiBaseUrl: 'http://localhost:5042/api/v1/uploads', // URL Backend C#
            onProgress: () => {},        // Callback update UI
            onComplete: () => {},
            onError: () => {},
            ...options
        };

        this.sessionId = null;
        this.aborted = false;
        this.totalChunks = Math.ceil(this.file.size / this.options.chunkSize);
        this.uploadQueue = []; // Danh sách các chunk cần upload
        this.activeConnections = 0;
        this.uploadedChunksCount = 0;
    }

    // 1. Khởi động: Gọi API Init để lấy SessionID và danh sách chunk còn thiếu
    async start() {
        try {
            // Bước 1: Init Session
            const initBody = {
                fileName: this.file.name,
                fileSize: this.file.size,
                totalChunks: this.totalChunks,
                userName: "user_demo_01" // Thực tế lấy từ Auth Context
            };

            const response = await fetch(`${this.options.apiBaseUrl}/init`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(initBody)
            });

            if (!response.ok) throw new Error("Init failed");

            const data = await response.json();
            this.sessionId = data.sessionId;
            
            // LOGIC RESUME QUAN TRỌNG:
            // Backend trả về list chunk CHƯA CÓ (missingChunks).
            // Ta chỉ đẩy những index này vào hàng đợi.
            this.uploadQueue = data.missingChunks;
            this.uploadedChunksCount = this.totalChunks - this.uploadQueue.length;

            console.log(`Session: ${this.sessionId}. Resuming... Need to upload ${this.uploadQueue.length} chunks.`);

            // Bước 2: Bắt đầu chạy Worker Pool
            this.processQueue();

        } catch (error) {
            this.options.onError(error);
        }
    }

    // 2. Worker Pool: Quản lý số lượng request song song
    processQueue() {
        if (this.aborted) return;

        // Nếu hàng đợi trống và không còn request nào đang chạy -> Xong
        if (this.uploadQueue.length === 0 && this.activeConnections === 0) {
            this.finalize();
            return;
        }

        // Spawn thêm worker nếu chưa đạt giới hạn concurrency
        while (this.activeConnections < this.options.concurrency && this.uploadQueue.length > 0) {
            const chunkIndex = this.uploadQueue.shift(); // Lấy chunk tiếp theo
            this.uploadChunk(chunkIndex);
        }
    }

    // 3. Upload từng chunk (Kèm logic Retry)
    async uploadChunk(chunkIndex, retryCount = 0) {
        this.activeConnections++;

        try {
            // Cắt file (Slice) - Chỉ tốn RAM cho 10MB này
            const start = chunkIndex * this.options.chunkSize;
            const end = Math.min(start + this.options.chunkSize, this.file.size);
            const chunkBlob = this.file.slice(start, end);

            const formData = new FormData();
            formData.append('index', chunkIndex);
            formData.append('file', chunkBlob, "blob");

            const res = await fetch(`${this.options.apiBaseUrl}/${this.sessionId}/chunk`, {
                method: 'POST',
                body: formData
            });

            if (!res.ok) throw new Error(`Upload chunk ${chunkIndex} failed`);

            // Thành công
            this.activeConnections--;
            this.uploadedChunksCount++;
            
            // Update UI
            const percent = Math.floor((this.uploadedChunksCount / this.totalChunks) * 100);
            this.options.onProgress(percent, this.uploadedChunksCount, this.totalChunks);

            // Gọi tiếp đệ quy để lấy việc mới
            this.processQueue();

        } catch (error) {
            console.warn(`Chunk ${chunkIndex} error:`, error);
            
            if (retryCount < this.options.maxRetries) {
                console.log(`Retrying chunk ${chunkIndex} (${retryCount + 1})...`);
                // Delay nhẹ trước khi retry (exponential backoff đơn giản)
                setTimeout(() => {
                    this.activeConnections--; // Giảm count để worker loop gọi lại
                    this.uploadChunk(chunkIndex, retryCount + 1); // Gọi lại chính nó
                    // Lưu ý: ở đây ta không gọi processQueue ngay mà để retry chạy
                }, 1000 * (retryCount + 1));
            } else {
                this.aborted = true;
                this.options.onError(new Error(`Failed chunk ${chunkIndex} after retries.`));
            }
        }
    }

    // 4. Kết thúc: Gọi API Finalize để Backend ghép file
    async finalize() {
        try {
            console.log("All chunks sent. Finalizing...");
            const res = await fetch(`${this.options.apiBaseUrl}/${this.sessionId}/finalize`, {
                method: 'POST'
            });

            if (!res.ok) throw new Error("Finalize failed");
            
            const result = await res.json();
            this.options.onComplete(result);
            
        } catch (error) {
            this.options.onError(error);
        }
    }

    pause() {
        this.aborted = true;
        // Lưu ý: Các request đang bay (in-flight) vẫn sẽ chạy nốt, 
        // nhưng queue sẽ dừng cấp việc mới.
    }
}