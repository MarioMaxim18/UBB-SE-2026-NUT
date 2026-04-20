using System;

namespace TeamNut.Models
{
    /// <summary>Represents a daily nutrition log entry.</summary>
    public class DailyLog
    {
        /// <summary>Gets or sets the log entry identifier.</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the user identifier.</summary>
        public int UserId { get; set; }

        /// <summary>Gets or sets the meal identifier.</summary>
        public int MealId { get; set; }

        /// <summary>Gets or sets when the meal was logged.</summary>
        public DateTime LoggedAt { get; set; }

        /// <summary>Gets or sets the calorie count.</summary>
        public double Calories { get; set; }

        /// <summary>Gets or sets the protein grams.</summary>
        public double Protein { get; set; }

        /// <summary>Gets or sets the carbohydrate grams.</summary>
        public double Carbs { get; set; }

        /// <summary>Gets or sets the fat grams.</summary>
        public double Fats { get; set; }
    }
}
