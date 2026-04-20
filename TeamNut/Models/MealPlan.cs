using System;

namespace TeamNut.Models
{
    /// <summary>Represents a generated meal plan for a user.</summary>
    public class MealPlan
    {
        /// <summary>Gets or sets the meal plan identifier.</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the user identifier.</summary>
        public int UserId { get; set; }

        /// <summary>Gets or sets when the plan was created.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Gets or sets the user's goal type (e.g. bulk, cut).</summary>
        public string GoalType { get; set; } = string.Empty;
    }
}
