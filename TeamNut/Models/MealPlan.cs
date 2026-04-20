using System;

namespace TeamNut.Models
{
    public class MealPlan
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string GoalType { get; set; } = string.Empty;
    }
}
