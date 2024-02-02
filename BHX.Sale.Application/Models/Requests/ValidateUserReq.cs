using System.ComponentModel.DataAnnotations;

namespace BHX.Sale.Application.Models.Requests
{
    public class ValidateUserReq
    {
        [Required]
        [MaxLength(50)]
        public string EmailId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Password { get; set; }
    }
}
