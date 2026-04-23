using FluentAssertions;
using TeamNut.Services;
using Xunit;

namespace TeamNut.Tests.Services
{
    public class FormattingServiceTests
    {
        private readonly FormattingService service;

        public FormattingServiceTests()
        {
            service = new FormattingService();
        }

        [Theory]
        [InlineData(100, 200, "kcal", "100 / 200 kcal (50%)")]
        [InlineData(150, 200, "g", "150 / 200 g (75%)")]
        [InlineData(200, 200, "g", "200 / 200 g (100%)")]
        [InlineData(50, 100, "kcal", "50 / 100 kcal (50%)")]
        public void FormatMetricWithGoal_WithValidGoal_ReturnsFormattedString(
            double total,
            double goal,
            string unit,
            string expected)
        {
            var result = service.FormatMetricWithGoal(total, goal, unit);

            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(100, 0, "kcal")]
        [InlineData(150, -50, "g")]
        public void FormatMetricWithGoal_WithZeroOrNegativeGoal_ReturnsFormatWithoutGoal(
            double total,
            double goal,
            string unit)
        {
            var result = service.FormatMetricWithGoal(total, goal, unit);

            result.Should().NotContain("/");
            result.Should().NotContain("%");
            result.Should().Contain(total.ToString("F0"));
            result.Should().Contain(unit);
        }

        [Theory]
        [InlineData(100, "kcal", "100 kcal")]
        [InlineData(150, "g", "150 g")]
        [InlineData(0, "kcal", "0 kcal")]
        [InlineData(2500, "kcal", "2500 kcal")]
        public void FormatMetricWithoutGoal_WithVariousInputs_ReturnsFormattedString(
            double total,
            string unit,
            string expected)
        {
            var result = service.FormatMetricWithoutGoal(total, unit);

            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(500, "500 kcal")]
        [InlineData(250, "250 kcal")]
        [InlineData(0, "0 kcal")]
        [InlineData(1000, "1000 kcal")]
        public void FormatBurnedCalories_WithVariousValues_ReturnsFormattedString(
            double calories,
            string expected)
        {
            var result = service.FormatBurnedCalories(calories);

            result.Should().Be(expected);
        }

        [Fact]
        public void FormatMetricWithGoal_WithZeroTotal_ReturnsZeroPercent()
        {
            var result = service.FormatMetricWithGoal(0, 200, "kcal");

            result.Should().Be("0 / 200 kcal (0%)");
        }

        [Fact]
        public void FormatMetricWithGoal_WithTotalExceedingGoal_ReturnsOver100Percent()
        {
            var result = service.FormatMetricWithGoal(300, 200, "kcal");

            result.Should().Be("300 / 200 kcal (150%)");
        }

        [Fact]
        public void FormatMetricWithGoal_WithDecimalValues_RoundsToWholeNumbers()
        {
            var result = service.FormatMetricWithGoal(123.7, 200.3, "g");

            result.Should().Be("124 / 200 g (62%)");
        }
    }
}