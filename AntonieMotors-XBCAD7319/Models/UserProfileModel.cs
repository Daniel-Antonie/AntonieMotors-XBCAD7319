using System.ComponentModel.DataAnnotations;

namespace AntonieMotors_XBCAD7319.Models
{
    public class UserProfileModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Display(Name = "Phone Number")]
        [Phone]
        public string PhoneNumber { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        // Add other profile fields as needed
    }
}
