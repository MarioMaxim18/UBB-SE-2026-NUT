namespace TeamNut.Models
{
    public class Ingredient
    {
        public int FoodId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double CaloriesPer100Grams { get; set; }
        public double ProteinPer100Grams { get; set; }
        public double CarbohydratesPer100Grams { get; set; }
        public double FatPer100Grams { get; set; }
    }
}
