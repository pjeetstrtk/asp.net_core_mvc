using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillingApp.Models
{
    public class Billing
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string InvoiceNo { get; set; } = string.Empty;

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public DateTime BillDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAmount { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        public List<BillingItem> Items { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();
    }
}