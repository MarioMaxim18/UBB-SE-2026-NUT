using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;
using TeamNut.Repositories.Interfaces;
using TeamNut.Services.Interfaces;

namespace TeamNut.Services
{
    public class MealPlanService : IMealPlanService
    {
        private readonly IMealPlanRepository mealPlanRepository;
        private readonly IUserRepository userRepository;
        private readonly IReminderService reminderService;
        private const int MinValidId = 1;
        private const string DateFormatIso = "yyyy-MM-dd";
        private static readonly TimeSpan BreakfastTime = new TimeSpan(8, 0, 0);
        private static readonly TimeSpan LunchTime = new TimeSpan(13, 0, 0);
        private static readonly TimeSpan DinnerTime = new TimeSpan(17, 0, 0);
        private const double DefaultTolerance = 0.10;
        private const string ReminderFrequencyOnce = "Once";
        private const string DefaultBreakfastName = "Breakfast";
        private const string DefaultLunchName = "Lunch";
        private const string DefaultDinnerName = "Dinner";
        private const string GoalBulk = "bulk";
        private const string GoalCut = "cut";
        private const string GoalMaintenance = "maintenance";
        private const string GoalWellBeing = "well-being";
        private const int BulkCaloriesDelta = 300;
        private const int CutCaloriesDelta = -300;
        private const int MaintenanceCaloriesDelta = 100;
        private const string EmojiBulk = "💪";
        private const string EmojiCut = "🔥";
        private const string EmojiMaintenance = "⚖️";
        private const string EmojiWellBeing = "🧘";
        private const string EmojiDefault = "🎯";
        private const string ErrInvalidUserId = "Invalid user ID. Please ensure you are logged in.";
        private const string ErrGenerateMealPlanFailed = "Failed to generate meal plan. Please try again.";
        private const string ErrInvalidMealPlanId = "Invalid meal plan ID.";
        private const string ErrInvalidUserIdForPlan = "Invalid user ID.";
        private const string ErrUserMustBeLoggedIn = "User must be logged in to save daily logs.";

        public MealPlanService(IMealPlanRepository mmealPlanRepository, IUserRepository uuserRepository, IReminderService rreminderService)
        {
            mealPlanRepository = mmealPlanRepository;
            userRepository = uuserRepository;
            reminderService = rreminderService;
        }

        /// <summary>Generates a personalized daily meal plan for the given user.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The identifier of the newly created meal plan.</returns>
        public async Task<int> GeneratePersonalizedMealPlanAsync(int userId)
        {
            if (userId < MinValidId)
            {
                throw new InvalidOperationException(ErrInvalidUserId);
            }

            try
            {
                int mealPlanId =
                    await mealPlanRepository
                        .GeneratePersonalizedDailyMealPlan(userId);

                try
                {
                    var today = DateTime.Today.ToString(DateFormatIso);
                    var existing =
                        await reminderService.GetUserReminders(userId);

                    bool alreadyHasTodayReminder = false;
                    foreach (var r in existing)
                    {
                        if (r.ReminderDate == today)
                        {
                            alreadyHasTodayReminder = true;
                            break;
                        }
                    }

                    if (!alreadyHasTodayReminder)
                    {
                        var meals =
                            await GetMealsForMealPlanAsync(mealPlanId);

                        string breakfastName = DefaultBreakfastName;
                        string lunchName = DefaultLunchName;
                        string dinnerName = DefaultDinnerName;

                        if (meals.Count > 0 &&
                            !string.IsNullOrWhiteSpace(meals[0].Name))
                        {
                            breakfastName = meals[0].Name.Trim();
                        }

                        if (meals.Count > 1 &&
                            !string.IsNullOrWhiteSpace(meals[1].Name))
                        {
                            lunchName = meals[1].Name.Trim();
                        }

                        if (meals.Count > 2 &&
                            !string.IsNullOrWhiteSpace(meals[2].Name))
                        {
                            dinnerName = meals[2].Name.Trim();
                        }

                        await reminderService.SaveReminder(
                            new Reminder
                            {
                                UserId = userId,
                                Name = breakfastName,
                                ReminderDate = today,
                                Time = BreakfastTime,
                                HasSound = false,
                                Frequency = ReminderFrequencyOnce
                            });

                        await reminderService.SaveReminder(
                            new Reminder
                            {
                                UserId = userId,
                                Name = lunchName,
                                ReminderDate = today,
                                Time = LunchTime,
                                HasSound = false,
                                Frequency = ReminderFrequencyOnce
                            });

                        await reminderService.SaveReminder(
                            new Reminder
                            {
                                UserId = userId,
                                Name = dinnerName,
                                ReminderDate = today,
                                Time = DinnerTime,
                                HasSound = false,
                                Frequency = ReminderFrequencyOnce
                            });

                        reminderService
                            .NotifyRemindersChangedForUser(userId);
                    }
                }
                catch
                {
                }

                if (mealPlanId < MinValidId)
                {
                    throw new InvalidOperationException(
                        ErrGenerateMealPlanFailed);
                }

                return mealPlanId;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error generating meal plan: {ex.Message}", ex);
            }
        }

        /// <summary>Gets the meals contained in the given meal plan.</summary>
        /// <param name="mealPlanId">The meal plan identifier.</param>
        /// <returns>A list of meals for that plan.</returns>
        public async Task<List<Meal>> GetMealsForMealPlanAsync(int mealPlanId)
        {
            if (mealPlanId < MinValidId)
            {
                throw new ArgumentException(
                    ErrInvalidMealPlanId,
                    nameof(mealPlanId));
            }

            try
            {
                return await mealPlanRepository.GetMealsForMealPlan(mealPlanId) ?? new List<Meal>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error retrieving meals: {ex.Message}", ex);
            }
        }

        /// <summary>Gets a meal plan by its identifier.</summary>
        /// <param name="mealPlanId">The meal plan identifier.</param>
        /// <returns>The <see cref="MealPlan"/>, or <c>null</c> if invalid or not found.</returns>
        public async Task<MealPlan?> GetMealPlanByIdAsync(int mealPlanId)
        {
            if (mealPlanId < MinValidId)
            {
                return null;
            }

            try
            {
                return await mealPlanRepository.GetById(mealPlanId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving meal plan: {ex.Message}", ex);
            }
        }

        /// <summary>Gets today's meal plan for the user, generating one if it doesn't exist.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>Today's <see cref="MealPlan"/>.</returns>
        public async Task<MealPlan?> GetTodaysMealPlanAsync(int userId)
        {
            if (userId < MinValidId)
            {
                throw new ArgumentException(
                    ErrInvalidUserIdForPlan,
                    nameof(userId));
            }

            try
            {
                var latest = await mealPlanRepository.GetLatestMealPlan(userId);

                if (latest?.CreatedAt.Date == DateTime.Today)
                {
                    return latest;
                }

                int newPlanId = await GeneratePersonalizedMealPlanAsync(userId);

                return await mealPlanRepository.GetById(newPlanId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving today's meal plan: {ex.Message}", ex);
            }
        }

        /// <summary>Gets all meal plans.</summary>
        /// <returns>All meal plans in the system.</returns>
        public async Task<IEnumerable<MealPlan>> GetAllMealPlansAsync()
        {
            try
            {
                return await mealPlanRepository.GetAll();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error retrieving meal plans: {ex.Message}", ex);
            }
        }

        /// <summary>Calculates total nutrition for a list of meals.</summary>
        /// <param name="meals">The meals to sum.</param>
        /// <returns>A tuple of (calories, protein, carbs, fat).</returns>
        public (int totalCalories, int totalProtein, int totalCarbs, int totalFat)
            CalculateTotalNutrition(List<Meal> meals)
        {
            if (meals == null || meals.Count == 0)
            {
                return (0, 0, 0, 0);
            }

            int calories = 0, protein = 0, carbs = 0, fat = 0;

            foreach (var meal in meals)
            {
                calories += meal.Calories;
                protein += meal.Protein;
                carbs += meal.Carbs;
                fat += meal.Fat;
            }

            return (totalCalories: calories, totalProtein: protein, totalCarbs: carbs, totalFat: fat);
        }

        /// <summary>Validates that a meal plan's totals are within tolerance of the targets.</summary>
        /// <param name="meals">The meals to validate.</param>
        /// <param name="targetCalories">Target calorie count.</param>
        /// <param name="targetProtein">Target protein grams.</param>
        /// <param name="targetCarbs">Target carbohydrate grams.</param>
        /// <param name="targetFat">Target fat grams.</param>
        /// <param name="tolerance">Allowed fractional deviation (default 10%).</param>
        /// <returns><c>true</c> if all totals are within tolerance.</returns>
        public bool ValidateMealPlan(
            List<Meal> meals,
            int targetCalories,
            int targetProtein,
            int targetCarbs,
            int targetFat,
            double tolerance = DefaultTolerance)
        {
            var (cal, p, c, f) =
                CalculateTotalNutrition(meals);

            return
                Math.Abs(cal - targetCalories) <= targetCalories * tolerance &&
                Math.Abs(p - targetProtein) <= targetProtein * tolerance &&
                Math.Abs(c - targetCarbs) <= targetCarbs * tolerance &&
                Math.Abs(f - targetFat) <= targetFat * tolerance;
        }

        /// <summary>Returns a human-readable calorie adjustment description for the given goal.</summary>
        /// <param name="goal">The user's goal string.</param>
        /// <param name="baseTdee">The user's base total daily energy expenditure.</param>
        /// <returns>A description of the calorie adjustment.</returns>
        public string GetCalorieAdjustmentDescription(
            string goal,
            int baseTdee)
        {
            return goal?.ToLower() switch
            {
                GoalBulk =>
                    $"+{BulkCaloriesDelta} kcal " +
                    $"(Bulking phase: {baseTdee} + {BulkCaloriesDelta} = {baseTdee + BulkCaloriesDelta} kcal)",

                GoalCut =>
                    $"{CutCaloriesDelta} kcal " +
                    $"(Cutting phase: {baseTdee} {CutCaloriesDelta} = {baseTdee + CutCaloriesDelta} kcal)",

                GoalMaintenance or GoalWellBeing =>
                    $"+{MaintenanceCaloriesDelta} kcal " +
                    $"({goal}: {baseTdee} + {MaintenanceCaloriesDelta} = {baseTdee + MaintenanceCaloriesDelta} kcal)",

                _ => $"No adjustment (Base TDEE: {baseTdee} kcal)"
            };
        }

        /// <summary>Returns an emoji representing the user's goal.</summary>
        /// <param name="goal">The user's goal string.</param>
        /// <returns>An emoji string for the goal.</returns>
        public string GetGoalEmoji(string goal)
        {
            return goal?.ToLower() switch
            {
                GoalBulk => EmojiBulk,
                GoalCut => EmojiCut,
                GoalMaintenance => EmojiMaintenance,
                GoalWellBeing => EmojiWellBeing,
                _ => EmojiDefault
            };
        }

        /// <summary>Gets the goal string for the given user.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The goal string, defaulting to "maintenance".</returns>
        public async Task<string> GetUserGoalAsync(int userId)
        {
            try
            {
                var userData =
                    await userRepository.GetUserDataByUserId(userId);

                return userData?.Goal?.ToLower()
                    ?? GoalMaintenance;
            }
            catch
            {
                return GoalMaintenance;
            }
        }

        /// <summary>Saves all meals from a meal plan to the current user's daily log.</summary>
        /// <param name="mealPlanId">The meal plan identifier.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveMealsToDailyLogAsync(int mealPlanId)
        {
            if (!UserSession.UserId.HasValue)
            {
                throw new InvalidOperationException(
                    ErrUserMustBeLoggedIn);
            }

            var meals = await GetMealsForMealPlanAsync(mealPlanId);
            await mealPlanRepository.SaveMealsToDailyLog(
                UserSession.UserId.Value,
                meals);
        }

        /// <summary>Saves a single meal to the current user's daily log.</summary>
        /// <param name="mealId">The meal identifier.</param>
        /// <param name="calories">The calorie value to log.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveMealToDailyLogAsync(
            int mealId,
            int calories)
        {
            if (!UserSession.UserId.HasValue)
            {
                throw new InvalidOperationException(
                    ErrUserMustBeLoggedIn);
            }

            await mealPlanRepository.SaveMealToDailyLog(
                UserSession.UserId.Value,
                mealId,
                calories);
        }
    }
}