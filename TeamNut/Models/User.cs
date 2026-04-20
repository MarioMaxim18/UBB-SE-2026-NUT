using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TeamNut.Models
{
    public partial class User : ObservableValidator
    {
        [ObservableProperty]
        [Key]
        public partial int Id { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = "Username is mandatory")]
        [StringLength(30, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username must be alphanumeric")]
        public partial string Username { get; set; } = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = "Password is mandatory")]
        [StringLength(30, MinimumLength = 8)]
        public partial string Password { get; set; } = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = "Role is mandatory")]
        [RegularExpression(@"^(User|Nutritionist)$", ErrorMessage = "Role must be 'User' or 'Nutritionist'")]
        public partial string Role { get; set; } = "User";

        public List<string> ValidateAndReturnErrors()
        {
            ValidateAllProperties();

            return GetErrors()
                .Select(e => e.ErrorMessage!)
                .Where(msg => msg != null)
                .ToList();
        }
    }
}
