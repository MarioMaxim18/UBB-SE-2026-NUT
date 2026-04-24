using System.Collections.Generic;
using FluentAssertions;
using TeamNut.Models;
using TeamNut.Services;
using Xunit;

namespace TeamNut.Tests.Services
{
    public class FilteringServiceTests
    {
        private readonly FilteringService service;

        public FilteringServiceTests()
        {
            service = new FilteringService();
        }

        [Fact]
        public void FilterMeals_NullList_ReturnsEmpty()
        {
            var result = service.FilterMeals(null!, "ciorba");

            result.Should().BeEmpty();
        }

        [Fact]
        public void FilterMeals_EmptySearch_ReturnsAll()
        {
            var meals = new List<Meal>
            {
                new Meal { Name = "Sarmale" },
                new Meal { Name = "Ciorba de burta" },
                new Meal { Name = "Mici" }
            };

            var result = service.FilterMeals(meals, string.Empty);

            result.Should().HaveCount(3);
        }

        [Fact]
        public void FilterMeals_NullSearch_ReturnsAll()
        {
            var meals = new List<Meal>
            {
                new Meal { Name = "Sarmale" },
                new Meal { Name = "Tochitura" }
            };

            var result = service.FilterMeals(meals, null!);

            result.Should().HaveCount(2);
        }

        [Fact]
        public void FilterMeals_MatchingSearch_FiltersCorrectly()
        {
            var meals = new List<Meal>
            {
                new Meal { Name = "Ciorba de legume" },
                new Meal { Name = "Ciorba de perisoare" },
                new Meal { Name = "Sarmale" }
            };

            var result = service.FilterMeals(meals, "Ciorba");

            result.Should().HaveCount(2);
            result.Should().Contain(m => m.Name == "Ciorba de legume");
            result.Should().Contain(m => m.Name == "Ciorba de perisoare");
        }

        [Fact]
        public void FilterMeals_NoMatches_ReturnsEmpty()
        {
            var meals = new List<Meal>
            {
                new Meal { Name = "Sarmale" },
                new Meal { Name = "Mici" }
            };

            var result = service.FilterMeals(meals, "Pizza");

            result.Should().BeEmpty();
        }

        [Fact]
        public void FilterMeals_CaseInsensitive()
        {
            var meals = new List<Meal>
            {
                new Meal { Name = "Sarmale cu mamaliga" },
                new Meal { Name = "Paste carbonara" }
            };

            var result = service.FilterMeals(meals, "sarmale");

            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Sarmale cu mamaliga");
        }

        [Fact]
        public void FilterMeals_TrimsWhitespace()
        {
            var meals = new List<Meal>
            {
                new Meal { Name = "Sarmale" }
            };

            var result = service.FilterMeals(meals, "  Sarmale  ");

            result.Should().HaveCount(1);
        }

        [Fact]
        public void FilterMeals_WhitespaceOnly_ReturnsAll()
        {
            var meals = new List<Meal>
            {
                new Meal { Name = "Sarmale" },
                new Meal { Name = "Mici" }
            };

            var result = service.FilterMeals(meals, "   ");

            result.Should().HaveCount(2);
        }

        [Fact]
        public void FilterMeals_NullMealName_DoesNotCrash()
        {
            var meals = new List<Meal>
            {
                new Meal { Name = null!, Calories = 200 },
                new Meal { Name = "Sarmale", Calories = 350 }
            };

            var result = service.FilterMeals(meals, "Sarmale");

            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Sarmale");
        }

        [Fact]
        public void FilterMeals_AllNullNames_ReturnsEmpty()
        {
            var meals = new List<Meal>
            {
                new Meal { Name = null!, Calories = 100 },
                new Meal { Name = null!, Calories = 200 }
            };

            var result = service.FilterMeals(meals, "ciorba");

            result.Should().BeEmpty();
        }

        [Fact]
        public void FilterIngredients_NullList_ReturnsEmpty()
        {
            service.FilterIngredients(null!, "ceapa").Should().BeEmpty();
        }

        [Fact]
        public void FilterIngredients_EmptySearch_ReturnsAll()
        {
            var ingredients = new List<Ingredient>
            {
                new Ingredient { Name = "Ceapa" },
                new Ingredient { Name = "Usturoi" },
                new Ingredient { Name = "Rosii" }
            };

            var result = service.FilterIngredients(ingredients, string.Empty);

            result.Should().HaveCount(3);
        }

        [Fact]
        public void FilterIngredients_MatchingSearch_Filters()
        {
            var ingredients = new List<Ingredient>
            {
                new Ingredient { Name = "Piept de pui" },
                new Ingredient { Name = "Pulpa de pui" },
                new Ingredient { Name = "Vita" }
            };

            var result = service.FilterIngredients(ingredients, "pui");

            result.Should().HaveCount(2);
        }

        [Fact]
        public void FilterIngredients_CaseInsensitive()
        {
            var ingredients = new List<Ingredient>
            {
                new Ingredient { Name = "ROSII" },
                new Ingredient { Name = "Cartofi" }
            };

            var result = service.FilterIngredients(ingredients, "rosii");

            result.Should().HaveCount(1);
        }
    }
}
