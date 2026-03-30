namespace TeamNut.Models
{
    public class Meal
    {
        public string Name { get; set; }
        public int Calories { get; set; }
        public int Carbs { get; set; }
        public int Fat { get; set; }

        public int Protein { get; set; }

        public bool IsVegan { get; set; }
        public bool IsKeto { get; set; }
        public bool IsGlutenFree { get; set; }
        public bool IsLactoseFree { get; set; }
        public bool IsNutFree { get; set; }

        public bool IsFavorite { get; set; }
    }
}