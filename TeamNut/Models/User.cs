using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TeamNut.Models
{
    /// <summary>Represents an application user account.</summary>
    public partial class User : ObservableValidator
    {
        /// <summary>Gets or sets the user identifier.</summary>
        [ObservableProperty]
        [Key]
        public partial int Id { get; set; }

        /// <summary>Gets or sets the username.</summary>
        [ObservableProperty]
        [Required(ErrorMessage = "Username is mandatory")]
        [StringLength(30, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username must be alphanumeric")]
        public partial string Username { get; set; } = string.Empty;

        /// <summary>Gets or sets the password.</summary>
        [ObservableProperty]
        [Required(ErrorMessage = "Password is mandatory")]
        [StringLength(30, MinimumLength = 8)]
        public partial string Password { get; set; } = string.Empty;

        /// <summary>Gets or sets the user role (User or Nutritionist).</summary>
        [ObservableProperty]
        [Required(ErrorMessage = "Role is mandatory")]
        [RegularExpression(@"^(User|Nutritionist)$", ErrorMessage = "Role must be 'User' or 'Nutritionist'")]
        public partial string Role { get; set; } = "User";

        /// <summary>Validates all properties and returns any error messages.</summary>
        /// <returns>A list of validation error messages.</returns>
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
