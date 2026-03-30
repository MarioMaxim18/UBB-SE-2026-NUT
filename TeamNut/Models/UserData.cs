using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TeamNut.Models
{
    public partial class UserData : ObservableValidator
    {
        [ObservableProperty]
        public partial int Id { get; set; }

        [ObservableProperty]
        public partial int UserId { get; set; }

        [ObservableProperty]
        [Range(1, 500, ErrorMessage = "Weight must be a positive whole number, between 1 and 500")]
        public partial int Weight { get; set; }

        [ObservableProperty]
        [Range(1, 300, ErrorMessage = "Height must be a positive whole number, between 1 and 300")]
        public partial int Height { get; set; }

        [ObservableProperty]
        public partial int Age { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = "Please select a gender")]
        [RegularExpression(@"^(male|female)$", ErrorMessage = "Gender must be 'male' or 'female'")]
        public partial string Gender { get; set; } = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = "Please select a goal")]
        [RegularExpression(@"^(bulk|cut|maintenance|well-being)$", ErrorMessage = "Select a valid goal")]
        public partial string Goal { get; set; } = string.Empty;

        [ObservableProperty]
        public partial int Bmi { get; set; }

        [ObservableProperty]
        public partial int CalorieNeeds { get; set; }

        [ObservableProperty]
        public partial int ProteinNeeds { get; set; }

        [ObservableProperty]
        public partial int CarbNeeds { get; set; }

        [ObservableProperty]
        public partial int FatNeeds { get; set; }

        public List<string> GetValidationErrors()
        {
            ValidateAllProperties();
            return GetErrors().Select(e => e.ErrorMessage!).Where(m => m != null).ToList();
        }

        public int CalculateAge(DateTimeOffset? birthDate)
        {
            if (birthDate == null) return 0;

            var today = DateTime.Today;
            var birth = birthDate.Value.DateTime;
            var age = today.Year - birth.Year;
            if (birth.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}