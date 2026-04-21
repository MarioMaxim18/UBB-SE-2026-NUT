namespace TeamNut.Services.Interfaces
{
    public interface IFormattingService
    {
        string FormatMetricWithGoal(double total, double goal, string unit);
        string FormatMetricWithoutGoal(double total, string unit);
        string FormatBurnedCalories(double calories);
    }
}
