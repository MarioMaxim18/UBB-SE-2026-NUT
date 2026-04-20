using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Services
{
    public interface IDailyLogService
    {
        Task<UserData> GetCurrentUserNutritionTargetsAsync();
        Task<DailyLog> GetCurrentWeekTotalsAsync();
        Task<List<Meal>> GetMealsForAutocompleteAsync();
        Task<double> GetTodayBurnedCaloriesAsync();
        Task<DailyLog> GetTodayTotalsAsync();
        Task<bool> HasAnyLogsAsync();
        Task LogMealAsync(Meal meal);
        Task<List<Meal>> SearchMealsAsync(string? searchTerm);
    }
}