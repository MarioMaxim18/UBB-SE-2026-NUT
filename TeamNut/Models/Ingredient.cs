// <copyright file="Ingredient.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

namespace TeamNut.Models
{
    /// <summary>Represents a food ingredient with nutritional values per 100g.</summary>
    public class Ingredient
    {
        public int FoodId { get; set; }

        public string Name { get; set; } = string.Empty;

        public double CaloriesPer100g { get; set; }

        public double ProteinPer100g { get; set; }

        public double CarbsPer100g { get; set; }

        public double FatPer100g { get; set; }
    }
}
