


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Contracts.Common.Events;
using Contracts.Common.Interfaces;
using Shared.Enums.Order;

namespace Ordering.Domain.Entities;


[Table("UploadSessions")]
public class UploadSession : AuditableEventEntity<Guid>, IEventEntity
{
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string ContentType { get; set; } = "application/octet-stream";

    public long FileSize { get; set; }

    public int TotalChunks { get; set; }

    // Lưu userId để map với hệ thống User của bạn
    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    // Đường dẫn file cuối cùng trên Blob Storage sau khi merge
    [MaxLength(500)]
    public string? FinalUrl { get; set; }

    public bool IsCompleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}