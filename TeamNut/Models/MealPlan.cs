using System;

namespace TeamNut.Models
{
    public class MealPlan
    {
        public int Id { get; set; } // mealplan_id
        public int UserId { get; set; } // user_id
        public DateTime CreatedAt { get; set; } // created_at
        public string GoalType { get; set; } // goal type
    }
}