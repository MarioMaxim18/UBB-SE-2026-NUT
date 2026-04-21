using System;

namespace TeamNut.Models
{
    /// <summary>Represents a generated meal plan for a user.</summary>
    public class MealPlan
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public string GoalType { get; set; } = string.Empty;
    }
}
