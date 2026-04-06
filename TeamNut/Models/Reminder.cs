using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;

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
        public partial string Name { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool HasSound { get; set; } = false;

        [ObservableProperty]
        [Required]
        public partial TimeSpan Time { get; set; }

        public string ReminderDate { get; set; }

        [ObservableProperty]
        public partial string Frequency { get; set; } = string.Empty;

        public string FullDateTimeDisplay => $"{ReminderDate} at {Time}"; 
    }
}
