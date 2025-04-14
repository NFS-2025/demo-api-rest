using System.ComponentModel.DataAnnotations;

namespace demo_api_rest.Models
{
    public class CsrfTrap
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime TrapDate { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? SessionCookie { get; set; }

        [MaxLength(1000)]
        public string? UserAgent { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        public string? AdditionalData { get; set; }
    }
}