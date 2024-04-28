using System.ComponentModel.DataAnnotations;

namespace ApplicationRent.Data.Identity
{
    public class Feedback
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Message { get; set; }

        public bool Status { get; set; } = false; // По умолчанию false
        public string Platform { get; set; }
    }
}
