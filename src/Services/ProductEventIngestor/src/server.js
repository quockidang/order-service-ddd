const http = require('http');

const productConsumer = require('./product-consumer');

const RUN_INTERVAL = 3000; // 5 giây

const startPipeline = async () => {
  console.log('--- [Pipeline] Đang bắt đầu quá trình fake data... ---');
  
  try {
    const result = await productConsumer.syncToElasticsearch(10);
    
    if (result.success) {
      console.log(`[${new Date().toLocaleTimeString()}] Đã index ${result.count} sản phẩm thành công (${result.took}ms)`);
    }
  } catch (error) {
    console.error(`[Critical Error] ${error.message}`);
  }
};

// Chạy vòng lặp
//setInterval(startPipeline, RUN_INTERVAL);

// Chạy ngay lần đầu
//startPipeline();


const host = '0.0.0.0';
const port = 8000;

const server = http.createServer((req, res) => {
  res.statusCode = 200;
  res.setHeader('Content-Type', 'text/plain');
  res.end('Hello from Node.js in a container!');
});

server.listen(port, host, () => {
  console.log(`Server running at http://${host}:${port}/`);
});
