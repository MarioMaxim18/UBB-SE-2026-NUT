using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TeamNut.Models
{
    public partial class Reminder : ObservableValidator
    {
        [ObservableProperty]
        [Key]
        public partial int Id { get; set; }

        [ObservableProperty]
        [Required]
        public partial int UserId { get; set; }

        [ObservableProperty]
        [Required]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 50 characters.")]
        public partial string Name { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool HasSound { get; set; } = false;

        [ObservableProperty]
        [Required]
        public partial TimeSpan Time { get; set; }

        public string ReminderDate { get; set; }

        [ObservableProperty]
        [Required]
        public partial string Frequency { get; set; } = "Once";

        public string FullDateTimeDisplay => $"{ReminderDate} at {Time}"; 

        public List<string> GetValidationErrors()
        {
            ValidateAllProperties();

            return GetErrors()
                .Select(error => error.ErrorMessage!)
                .Where(message => message != null)
                .ToList();
        }
    }
}
