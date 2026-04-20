using System;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TeamNut.Models
{
    /// <summary>Represents a scheduled health reminder for a user.</summary>
    public partial class Reminder : ObservableValidator
    {
        /// <summary>Gets or sets the reminder identifier.</summary>
        [ObservableProperty]
        [Key]
        public partial int Id { get; set; }

        /// <summary>Gets or sets the user identifier.</summary>
        [ObservableProperty]
        [Required]
        public partial int UserId { get; set; }

        /// <summary>Gets or sets the reminder name.</summary>
        [ObservableProperty]
        [Required]
        public partial string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets a value indicating whether a sound plays with this reminder.</summary>
        [ObservableProperty]
        public partial bool HasSound { get; set; }

        /// <summary>Gets or sets the time of day for the reminder.</summary>
        [ObservableProperty]
        [Required]
        public partial TimeSpan Time { get; set; }

        /// <summary>Gets or sets the date string for the reminder.</summary>
        public string ReminderDate { get; set; } = string.Empty;

        /// <summary>Gets or sets how often the reminder repeats.</summary>
        [ObservableProperty]
        public partial string Frequency { get; set; } = "Once";

        /// <summary>Gets a combined date and time display string.</summary>
        public string FullDateTimeDisplay => $"{ReminderDate ?? "No date"} at {Time}";
    }
}
