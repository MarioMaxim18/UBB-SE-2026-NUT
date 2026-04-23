using FluentAssertions;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using TeamNut;
using TeamNut.Converters;
using TeamNut.Models;
using Xunit;

namespace TeamNut.Tests.Converters
{
    public class ValueConvertersTests
    {
        public ValueConvertersTests()
        {
            UserSession.Logout();
        }

        [Fact]
        public void BooleanToVisibilityConverter_WhenValueIsTrue_ReturnsVisible()
        {
            var converter = new BooleanToVisibilityConverter();

            var result = converter.Convert(true, typeof(Visibility), null, string.Empty);

            result.Should().Be(Visibility.Visible);
        }

        [Fact]
        public void BooleanToVisibilityConverter_WhenConvertingBackVisible_ReturnsTrue()
        {
            var converter = new BooleanToVisibilityConverter();

            var result = converter.ConvertBack(Visibility.Visible, typeof(bool), null, string.Empty);

            result.Should().Be(true);
        }

        [Fact]
        public void BoolToVisibilityConverter_WhenRoleIsNutritionistAndParameterMatches_ReturnsVisible()
        {
            var converter = new BoolToVisibilityConverter();

            var result = converter.Convert("Nutritionist", typeof(Visibility), "Nutritionist", string.Empty);

            result.Should().Be(Visibility.Visible);
        }

        [Fact]
        public void BoolToVisibilityConverter_WhenInverseBooleanAndValueIsTrue_ReturnsCollapsed()
        {
            var converter = new BoolToVisibilityConverter();

            var result = converter.Convert(true, typeof(Visibility), "Inverse", string.Empty);

            result.Should().Be(Visibility.Collapsed);
        }

        [Fact(Skip = "Requires WinUI UI type initialization in the test host.")]
        public void BoolToFontWeightConverter_WhenValueIsTrue_ReturnsBold()
        {
            var converter = new BoolToFontWeightConverter();

            var result = converter.Convert(true, typeof(object), null, string.Empty);

            result.Should().Be(FontWeights.Bold);
        }

        [Fact]
        public void InverseBoolConverter_WhenValueIsFalse_ReturnsTrue()
        {
            var converter = new InverseBoolConverter();

            var result = converter.Convert(false, typeof(bool), null, string.Empty);

            result.Should().Be(true);
        }

        [Fact]
        public void EmptyStringToVisibilityConverter_WhenStringIsWhitespace_ReturnsVisible()
        {
            var converter = new EmptyStringToVisibilityConverter();

            var result = converter.Convert("   ", typeof(Visibility), null, string.Empty);

            result.Should().Be(Visibility.Visible);
        }

        [Fact]
        public void RoleToHorizontalAlignmentConverter_WhenRoleIsNutritionist_ReturnsLeft()
        {
            var converter = new RoleToHorizontalAlignmentConverter();

            var result = converter.Convert("Nutritionist", typeof(HorizontalAlignment), null, string.Empty);

            result.Should().Be(HorizontalAlignment.Left);
        }

        [Fact]
        public void IntZeroToVisibilityConverter_WhenValueIsZero_ReturnsVisible()
        {
            var converter = new IntZeroToVisibilityConverter();

            var result = converter.Convert(0, typeof(Visibility), null, string.Empty);

            result.Should().Be(Visibility.Visible);
        }

        [Fact(Skip = "Requires WinUI brush/color type initialization in the test host.")]
        public void RoleToBackgroundConverter_WhenRoleIsNutritionist_ReturnsExpectedBrush()
        {
            var converter = new RoleToBackgroundConverter();

            var result = converter.Convert("Nutritionist", typeof(SolidColorBrush), null, string.Empty);

            result.Should().BeOfType<SolidColorBrush>();
            ((SolidColorBrush)result).Color.Should().Be(ConverterConstants.NutritionistBackground);
        }

        [Fact(Skip = "Requires WinUI brush/color type initialization in the test host.")]
        public void UnansweredToHighlightConverter_WhenNutritionistAndHasUnanswered_ReturnsHighlightBrush()
        {
            var converter = new UnansweredToHighlightConverter();
            UserSession.Login(1, "nutritionist", "Nutritionist");

            try
            {
                var result = converter.Convert(true, typeof(SolidColorBrush), null, string.Empty);

                result.Should().BeOfType<SolidColorBrush>();
                ((SolidColorBrush)result).Color.Should().Be(ConverterConstants.UnansweredHighlight);
            }
            finally
            {
                UserSession.Logout();
            }
        }

        [Fact]
        public void IntGreaterThanZeroToVisibilityConverter_WhenValueIsPositive_ReturnsVisible()
        {
            var converter = new IntGreaterThanZeroToVisibilityConverter();

            var result = converter.Convert(5, typeof(Visibility), null, string.Empty);

            result.Should().Be(Visibility.Visible);
        }
    }
}
