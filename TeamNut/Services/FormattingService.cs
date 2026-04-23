namespace TeamNut.Services
{
    using TeamNut.Services.Interfaces;

    public class FormattingService : IFormattingService
    {
        private const double PercentMultiplier = 100.0;

        private const string MetricFormatWithGoal = "{0:F0} / {1:F0} {2} ({3:F0}%)";

        private const string MetricFormatNoGoal = "{0:F0} {1}";

        private const string BurnedCaloriesFormat = "{0:F0} kcal";

        public string FormatMetricWithGoal(double total, double goal, string unit)
        {
            if (goal <= 0)
            {
                return FormatMetricWithoutGoal(total, unit);
            }

            var pct = (total / goal) * PercentMultiplier;
            return string.Format(MetricFormatWithGoal, total, goal, unit, pct);
        }

        public string FormatMetricWithoutGoal(double total, string unit)
        {
            return string.Format(MetricFormatNoGoal, total, unit);
        }

        public string FormatBurnedCalories(double calories)
        {
            return string.Format(BurnedCaloriesFormat, calories);
        }
    }
}
