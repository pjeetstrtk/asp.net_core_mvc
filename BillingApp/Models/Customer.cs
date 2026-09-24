using System.ComponentModel.DataAnnotations;

namespace BillingApp.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress, StringLength(100)]
        public string? Email { get; set; }

        [StringLength(15)]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Billing>? Billings { get; set; }
    }
}