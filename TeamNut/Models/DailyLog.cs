using System;

namespace TeamNut.Models
{
    public class DailyLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MealId { get; set; }
        public DateTime LoggedAt { get; set; }
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fats { get; set; }
    }
}
