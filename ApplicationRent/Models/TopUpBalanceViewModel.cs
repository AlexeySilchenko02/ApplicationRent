using System.ComponentModel.DataAnnotations;

namespace ApplicationRent.Models
{
    public class TopUpBalanceViewModel
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Сумма пополнения должна быть больше нуля.")]
        public decimal Amount { get; set; }
    }
}
