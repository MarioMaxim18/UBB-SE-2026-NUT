using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.ModelViews;
using TeamNut.Models;
using TeamNut.Services;

namespace TeamNut.Views.MealPlanView
{
    /// <summary>Page for displaying and managing the user's daily meal plan.</summary>
    public sealed partial class MealPlanPage : Page
    {
        /// <summary>Gets the view model.</summary>
        public MealPlanViewModel ViewModel { get; } = new MealPlanViewModel();
        private UserService userService;

        private const string ButtonOk = "OK";
        private const string ButtonSave = "Save";
        private const string ButtonCancel = "Cancel";
        private const string GenderMale = "male";
        private const string GenderFemale = "female";
        private const string TitleNotLoggedIn = "Not Logged In";
        private const string TitleNoDataFound = "No Data Found";
        private const string TitleInvalidInput = "Invalid Input";
        private const string TitleSettingsUpdated = "Settings Updated";
        private const string TitleError = "Error";
        private const string TitleNoMealPlan = "No Meal Plan";
        private const string TitleNoMeals = "No Meals";
        private const string TitleSuccess = "Success";
        private const string TitleSaveFailed = "Save Failed";
        private const string TitleRegenerationFailed = "Regeneration Failed";
        private const string TitleUpdatePreferences = "Update Your Preferences";
        private const string MsgLoginRequired = "You must be logged in to update your settings.";
        private const string MsgNoUserData = "No user data found. Please complete your profile first.";
        private const string MsgInvalidWeightHeight = "Weight and height must be positive numbers.";
        private const string MsgSettingsSaved = "Your preferences have been saved successfully!\n\n" + "Your new preferences will be applied to tomorrow's meal plan, which will be automatically generated when you log in.\n\n" + "Today's meal plan will remain unchanged.";
        private const string MsgSettingsStatus = "Settings saved! New preferences will apply to tomorrow's meal plan.";
        private const string MsgNoMealPlanLoaded = "No meal plan is currently loaded. Please generate a meal plan first.";
        private const string MsgNoMealsGenerated = "No meals to save. Please generate a meal plan first.";
        private const string MsgPreferenceInfo = "Changes will be reflected in your next meal plan generation.";
        private const string MealBullet = "ù";
        private const string KcalUnit = "kcal";
        private const int WeightMin = 1;
        private const int WeightMax = 500;
        private const int HeightMin = 1;
        private const int HeightMax = 300;
        private const int StackPanelSpacing = 15;
        private const double InfoTextOpacity = 0.7;
        private const int InfoFontSize = 12;
        private const int InfoTopMargin = 10;
        private const int IndexMale = 0;
        private const int IndexFemale = 1;
        private const int GoalBulk = 0;
        private const int GoalCut = 1;
        private const int GoalMaintenance = 2;
        private const int GoalWellBeing = 3;

        /// <summary>Initializes a new instance of the <see cref="MealPlanPage"/> class.</summary>
        public MealPlanPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
            userService = new UserService();

            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.StatusMessage))
                {
                    StatusMessageText.Text = ViewModel.StatusMessage;
                }
                else if (e.PropertyName == nameof(ViewModel.GoalDescription))
                {
                    GoalDescriptionText.Text = ViewModel.GoalDescription;
                }
                else if (e.PropertyName == nameof(ViewModel.TotalNutritionSummary))
                {
                    TotalNutritionText.Text = ViewModel.TotalNutritionSummary;
                }
                else if (e.PropertyName == nameof(ViewModel.GeneratedMeals))
                {
                    UpdateMealsList();
                }
                else if (e.PropertyName == nameof(ViewModel.ShowErrorDialog) && ViewModel.ShowErrorDialog)
                {
                    ShowErrorDialog();
                }
            };

            ViewModel.GeneratedMeals.CollectionChanged += (s, e) => UpdateMealsList();

            _ = ViewModel.LoadOrGenerateTodaysMealPlanAsync();
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            int? userId = UserSession.UserId;

            if (userId == null || userId <= 0)
            {
                await ShowSimpleDialog(TitleNotLoggedIn, MsgLoginRequired);
                return;
            }

            var userData = await userService.GetUserDataAsync(userId.Value);

            if (userData == null)
            {
                await ShowSimpleDialog(TitleNoDataFound, MsgNoUserData);
                return;
            }

            await ShowSettingsDialog(userData);
        }

        private async System.Threading.Tasks.Task ShowSettingsDialog(UserData userData)
        {
            var dialog = new ContentDialog
            {
                Title = TitleUpdatePreferences,
                PrimaryButtonText = ButtonSave,
                CloseButtonText = ButtonCancel,
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };

            var panel = new StackPanel { Spacing = StackPanelSpacing };

            var weightBox = new NumberBox
            {
                Header = "Weight (kg)",
                Value = userData.Weight,
                Minimum = WeightMin,
                Maximum = WeightMax,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
            };

            var heightBox = new NumberBox
            {
                Header = "Height (cm)",
                Value = userData.Height,
                Minimum = HeightMin,
                Maximum = HeightMax,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
            };

            var genderCombo = new ComboBox
            {
                Header = "Gender",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                SelectedIndex = userData.Gender.Equals(GenderMale, StringComparison.OrdinalIgnoreCase)
                    ? IndexMale
                    : IndexFemale
            };
            genderCombo.Items.Add("Male");
            genderCombo.Items.Add("Female");

            var goalCombo = new ComboBox
            {
                Header = "Goal",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            goalCombo.Items.Add("Bulk");
            goalCombo.Items.Add("Cut");
            goalCombo.Items.Add("Maintenance");
            goalCombo.Items.Add("Well-being");

            goalCombo.SelectedIndex = userData.Goal.ToLower() switch
            {
                "bulk" => GoalBulk,
                "cut" => GoalCut,
                "maintenance" => GoalMaintenance,
                "well-being" => GoalWellBeing,
                _ => GoalMaintenance
            };

            var infoText = new TextBlock
            {
                Text = MsgPreferenceInfo,
                TextWrapping = TextWrapping.Wrap,
                Opacity = InfoTextOpacity,
                FontSize = InfoFontSize,
                Margin = new Thickness(0, InfoTopMargin, 0, 0)
            };

            panel.Children.Add(weightBox);
            panel.Children.Add(heightBox);
            panel.Children.Add(genderCombo);
            panel.Children.Add(goalCombo);
            panel.Children.Add(infoText);

            dialog.Content = panel;

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            {
                return;
            }

            if (weightBox.Value < WeightMin || heightBox.Value < HeightMin)
            {
                await ShowSimpleDialog(TitleInvalidInput, MsgInvalidWeightHeight);
                return;
            }

            userData.Weight = (int)weightBox.Value;
            userData.Height = (int)heightBox.Value;
            userData.Gender = genderCombo.SelectedIndex == IndexMale ? GenderMale : GenderFemale;
            userData.Goal = goalCombo.SelectedItem?.ToString()?.ToLower() ?? "maintenance";

            userData.Bmi = userData.CalculateBmi();
            userData.CalorieNeeds = userData.CalculateCalorieNeeds();
            userData.ProteinNeeds = userData.CalculateProteinNeeds();
            userData.CarbNeeds = userData.CalculateCarbNeeds();
            userData.FatNeeds = userData.CalculateFatNeeds();

            try
            {
                await userService.UpdateUserDataAsync(userData);

                await ShowSimpleDialog(TitleSettingsUpdated, MsgSettingsSaved);
                StatusMessageText.Text = MsgSettingsStatus;
            }
            catch (Exception ex)
            {
                await ShowSimpleDialog(TitleError, $"Failed to update settings: {ex.Message}");
            }
        }

        private async void SaveToDailyLogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.CurrentMealPlanId <= 0)
                {
                    await ShowSimpleDialog(TitleNoMealPlan, MsgNoMealPlanLoaded);
                    return;
                }

                if (ViewModel.GeneratedMeals.Count == 0)
                {
                    await ShowSimpleDialog(TitleNoMeals, MsgNoMealsGenerated);
                    return;
                }

                await ViewModel.SaveToDailyLogAsync();

                var message = $"Successfully saved {ViewModel.GeneratedMeals.Count} meals:\n\n";
                foreach (var meal in ViewModel.GeneratedMeals)
                {
                    message += $"{MealBullet} {meal.Name}: {meal.Calories} {KcalUnit}\n";
                }

                await ShowSimpleDialog(TitleSuccess, message);

                StatusMessageText.Text =
                    $"All {ViewModel.GeneratedMeals.Count} meals saved to daily log!";
            }
            catch (Exception ex)
            {
                await ShowSimpleDialog(
                    TitleSaveFailed,
                    $"Failed to save to daily log:\n\n{ex.Message}");
            }
        }

        private async void AddMealToLogsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { DataContext: MealViewModel meal })
            {
                return;
            }

            try
            {
                var mealPlanService = new MealPlanService();
                await mealPlanService.SaveMealToDailyLogAsync(meal.Id, meal.Calories);
                StatusMessageText.Text = $"{meal.Name} saved to daily log.";
            }
            catch (Exception ex)
            {
                await ShowSimpleDialog(TitleSaveFailed, $"Failed to save meal:\n\n{ex.Message}");
            }
        }

        private async void RegenerateTestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusMessageText.Text = "Regenerating meal plan...";
                await ViewModel.LoadOrGenerateTodaysMealPlanAsync();
            }
            catch (Exception ex)
            {
                await ShowSimpleDialog(TitleRegenerationFailed, ex.Message);
            }
        }

        private async void ShowErrorDialog()
        {
            await ShowSimpleDialog(
                ViewModel.ErrorDialogTitle,
                ViewModel.ErrorDialogMessage);

            ViewModel.ShowErrorDialog = false;
        }

        private async System.Threading.Tasks.Task ShowSimpleDialog(string title, string content)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = ButtonOk,
                XamlRoot = XamlRoot
            };

            await dialog.ShowAsync();
        }

        private void UpdateMealsList()
        {
            MealsListView.ItemsSource = ViewModel.GeneratedMeals;
        }
    }
}