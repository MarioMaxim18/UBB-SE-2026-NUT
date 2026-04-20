namespace TeamNut.Models
{
    /// <summary>Represents a food ingredient with nutritional values per 100g.</summary>
    public class Ingredient
    {
        /// <summary>Gets or sets the food identifier.</summary>
        public int FoodId { get; set; }

        /// <summary>Gets or sets the ingredient name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the calories per 100g.</summary>
        public double CaloriesPer100g { get; set; }

        /// <summary>Gets or sets the protein grams per 100g.</summary>
        public double ProteinPer100g { get; set; }

        /// <summary>Gets or sets the carbohydrate grams per 100g.</summary>
        public double CarbsPer100g { get; set; }

        /// <summary>Gets or sets the fat grams per 100g.</summary>
        public double FatPer100g { get; set; }
    }
}
