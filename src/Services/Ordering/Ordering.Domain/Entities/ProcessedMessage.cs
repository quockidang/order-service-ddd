

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Contracts.Common.Events;
using Contracts.Common.Interfaces;
using Contracts.Domains;
using Contracts.Domains.Interfaces;
using Shared.Enums.Order;

namespace Ordering.Domain.Entities;

[Table("ProcessedMessage")]
public class ProcessedMessage : EntityAuditBase<long>
{
    // public string MessageKey { get; set; } = null!;
    public string Source { get; set; } = null!;

    public string? Topic { get; set; }
    public int? PartitionId { get; set; }
    public long? OffsetValue { get; set; }

    public string Status { get; set; } = "PROCESSING";
    public string? Response { get; set; }
}