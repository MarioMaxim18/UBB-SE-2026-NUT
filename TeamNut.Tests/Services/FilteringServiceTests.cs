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
        public void FilterMeals_WithNullMeals_ReturnsEmptyList()
        {
            var result = service.FilterMeals(null, "test");

            result.Should().BeEmpty();
        }

        [Fact]
        public void FilterMeals_WithEmptySearchText_ReturnsAllMeals()
        {
            var meals = new List<Meal>
            {
                new Meal { Name = "Chicken Salad" },
                new Meal { Name = "Beef Stew" },
                new Meal { Name = "Fish Tacos" }
            };

            var result = service.FilterMeals(meals, "");

            result.Should().HaveCount(3);
        }

        [Fact]
        public void FilterMeals_WithNullSearchText_ReturnsAllMeals()
        {
            var meals = new List<Meal>
            {
                new Meal { Name = "Chicken Salad" },
                new Meal { Name = "Beef Stew" }
            };

            var result = service.FilterMeals(meals, null);

            result.Should().HaveCount(2);
        }

        [Fact]
        public void FilterMeals_WithWhitespaceSearchText_ReturnsAllMeals()
        {
            var meals = new List<Meal>
            {
                new Meal { Name = "Chicken Salad" },
                new Meal { Name = "Beef Stew" }
            };

            var result = service.FilterMeals(meals, "   ");

            result.Should().HaveCount(2);
        }

        [Fact]
        public void FilterMeals_WithMatchingSearch_ReturnsFilteredMeals()
        {
            var meals = new List<Meal>
            {
                new Meal { Name = "Chicken Salad" },
                new Meal { Name = "Chicken Soup" },
                new Meal { Name = "Beef Stew" }
            };

            var result = service.FilterMeals(meals, "Chicken");

            result.Should().HaveCount(2);
            result.Should().Contain(m => m.Name == "Chicken Salad");
            result.Should().Contain(m => m.Name == "Chicken Soup");
        }

        [Fact]
        public void FilterMeals_WithNoMatches_ReturnsEmptyList()
        {
            var meals = new List<Meal>
            {
                new Meal { Name = "Chicken Salad" },
                new Meal { Name = "Beef Stew" }
            };

            var result = service.FilterMeals(meals, "Pizza");

            result.Should().BeEmpty();
        }

        [Fact]
        public void FilterMeals_IsCaseInsensitive_ReturnsMatches()
        {
            var meals = new List<Meal>
            {
                new Meal { Name = "Chicken Salad" },
                new Meal { Name = "BEEF STEW" }
            };

            var result = service.FilterMeals(meals, "chicken");

            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Chicken Salad");
        }

        [Fact]
        public void FilterMeals_WithSearchTextTrimming_IgnoresWhitespace()
        {
            var meals = new List<Meal>
            {
                new Meal { Name = "Chicken Salad" }
            };

            var result = service.FilterMeals(meals, "  Chicken  ");

            result.Should().HaveCount(1);
        }

        [Fact]
        public void FilterMeals_WithNullMealName_SkipsThatMeal()
        {
            var meals = new List<Meal>
            {
                new Meal { Name = "Chicken Salad" },
                new Meal { Name = null },
                new Meal { Name = "Beef Stew" }
            };

            var result = service.FilterMeals(meals, "Salad");

            result.Should().HaveCount(1);
        }

        [Fact]
        public void FilterIngredients_WithNullIngredients_ReturnsEmptyList()
        {
            var result = service.FilterIngredients(null, "test");

            result.Should().BeEmpty();
        }

        [Fact]
        public void FilterIngredients_WithEmptySearchText_ReturnsAllIngredients()
        {
            var ingredients = new List<Ingredient>
            {
                new Ingredient { Name = "Chicken" },
                new Ingredient { Name = "Beef" },
                new Ingredient { Name = "Fish" }
            };

            var result = service.FilterIngredients(ingredients, "");

            result.Should().HaveCount(3);
        }

        [Fact]
        public void FilterIngredients_WithMatchingSearch_ReturnsFilteredIngredients()
        {
            var ingredients = new List<Ingredient>
            {
                new Ingredient { Name = "Chicken Breast" },
                new Ingredient { Name = "Chicken Thigh" },
                new Ingredient { Name = "Beef" }
            };

            var result = service.FilterIngredients(ingredients, "Chicken");

            result.Should().HaveCount(2);
        }

        [Fact]
        public void FilterIngredients_IsCaseInsensitive_ReturnsMatches()
        {
            var ingredients = new List<Ingredient>
            {
                new Ingredient { Name = "TOMATO" },
                new Ingredient { Name = "Potato" }
            };

            var result = service.FilterIngredients(ingredients, "tomato");

            result.Should().HaveCount(1);
        }
    }
}