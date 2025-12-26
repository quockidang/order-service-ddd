

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Contracts.Common.Events;
using Contracts.Common.Interfaces;
using Shared.Enums.Order;

namespace Ordering.Domain.Entities;

[Table("Order")]
public class Order : AuditableEventEntity<Guid>, IEventEntity
{
    [Required]
    [Column(TypeName = "varchar(150)")]
    public required string UserName { get; set; }

    public Guid DocumentNo { get; set; } = Guid.NewGuid();

    [Column(TypeName = "decimal(10,2)")] 
    public required decimal TotalPrice { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    public required string FirstName { get; set; }
    [Required]
    [Column(TypeName = "varchar(250)")]
    public required string LastName { get; set; }

    [Required]
    [EmailAddress]
    [Column(TypeName = "varchar(250)")]
    public required string EmailAddress { get; set; }

    [Column(TypeName = "varchar(255)")] 
    public required string ShippingAddress { get; set; }
    
    [Column(TypeName = "varchar(255)")] 
    public required string InvoiceAddress { get; set; }

    public EOrderStatus Status { get; set; }

    [NotMapped]
    public string FullName => FirstName + " " + LastName;
    
}