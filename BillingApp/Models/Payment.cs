using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillingApp.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int BillingId { get; set; }
        public Billing? Billing { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(30)]
        public string Method { get; set; } = "Cash";

        [StringLength(100)]
        public string? Reference { get; set; }
    }
}