using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.ModelViews;
using TeamNut.Models;
using TeamNut.Services;
using TeamNut.ViewModels;

namespace TeamNut.Views.MealPlanView
{
    public sealed partial class MealPlanPage : Page
    {
        public MealPlanViewModel ViewModel { get; } = new MealPlanViewModel();
        private UserService userService;

        public MealPlanPage()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel;
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

            ViewModel.GeneratedMeals.CollectionChanged += (s, e) =>
            {
                UpdateMealsList();
            };

            _ = ViewModel.LoadOrGenerateTodaysMealPlanAsync();
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            int? userId = UserSession.UserId;

            if (userId == null || userId <= 0)
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Not Logged In",
                    Content = "You must be logged in to update your settings.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = await errorDialog.ShowAsync();
                return;
            }

            var userData = await userService.GetUserDataAsync(userId.Value);

            if (userData == null)
            {
                var errorDialog = new ContentDialog
                {
                    Title = "No Data Found",
                    Content = "No user data found. Please complete your profile first.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = await errorDialog.ShowAsync();
                return;
            }

            await ShowSettingsDialog(userData);
        }

        private async System.Threading.Tasks.Task ShowSettingsDialog(UserData userData)
        {
            var dialog = new ContentDialog
            {
                Title = "Update Your Preferences",
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var stackPanel = new StackPanel { Spacing = 15 };

            var weightBox = new NumberBox
            {
                Header = "Weight (kg)",
                Value = userData.Weight,
                Minimum = 1,
                Maximum = 500,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
            };
            stackPanel.Children.Add(weightBox);

            var heightBox = new NumberBox
            {
                Header = "Height (cm)",
                Value = userData.Height,
                Minimum = 1,
                Maximum = 300,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
            };
            stackPanel.Children.Add(heightBox);

            var genderCombo = new ComboBox
            {
                Header = "Gender",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                SelectedIndex = userData.Gender.Equals("male", StringComparison.OrdinalIgnoreCase) ? 0 : 1
            };
            genderCombo.Items.Add("Male");
            genderCombo.Items.Add("Female");
            stackPanel.Children.Add(genderCombo);

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
                "bulk" => 0,
                "cut" => 1,
                "maintenance" => 2,
                "well-being" => 3,
                _ => 2
            };
            stackPanel.Children.Add(goalCombo);

            var infoText = new TextBlock
            {
                Text = "Changes will be reflected in your next meal plan generation.",
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.7,
                FontSize = 12,
                Margin = new Thickness(0, 10, 0, 0)
            };
            stackPanel.Children.Add(infoText);

            dialog.Content = stackPanel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                if (weightBox.Value < 1 || heightBox.Value < 1)
                {
                    var validationDialog = new ContentDialog
                    {
                        Title = "Invalid Input",
                        Content = "Weight and height must be positive numbers.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    _ = await validationDialog.ShowAsync();
                    return;
                }

                userData.Weight = (int)weightBox.Value;
                userData.Height = (int)heightBox.Value;
                userData.Gender = genderCombo.SelectedIndex == 0 ? "male" : "female";
                var goalText = goalCombo.SelectedItem?.ToString();
                if (string.IsNullOrWhiteSpace(goalText))
                {
                    var goalDialog = new ContentDialog
                    {
                        Title = "Invalid Input",
                        Content = "Please select a goal.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    _ = await goalDialog.ShowAsync();
                    return;
                }

                userData.Goal = goalText.ToLowerInvariant();

                userData.Bmi = userData.CalculateBmi();
                userData.CalorieNeeds = userData.CalculateCalorieNeeds();
                userData.ProteinNeeds = userData.CalculateProteinNeeds();
                userData.CarbNeeds = userData.CalculateCarbNeeds();
                userData.FatNeeds = userData.CalculateFatNeeds();

                try
                {
                    await userService.UpdateUserDataAsync(userData);

                    var successDialog = new ContentDialog
                    {
                        Title = "Settings Updated",
                        Content = "Your preferences have been saved successfully!\n\nYour new preferences will be applied to tomorrow's meal plan, which will be automatically generated when you log in.\n\nToday's meal plan will remain unchanged.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    _ = await successDialog.ShowAsync();

                    StatusMessageText.Text = "Settings saved! New preferences will apply to tomorrow's meal plan.";
                }
                catch (Exception ex)
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = $"Failed to update settings: {ex.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    _ = await errorDialog.ShowAsync();
                }
            }
        }

        private async void ShowErrorDialog()
        {
            var dialog = new ContentDialog
            {
                Title = ViewModel.ErrorDialogTitle,
                Content = ViewModel.ErrorDialogMessage,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            _ = await dialog.ShowAsync();
            ViewModel.ShowErrorDialog = false; // Reset the flag
        }

        private void UpdateMealsList()
        {
            MealsListView.ItemsSource = ViewModel.GeneratedMeals;
        }

        private async void SaveToDailyLogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.CurrentMealPlanId <= 0)
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "No Meal Plan",
                        Content = "No meal plan is currently loaded. Please generate a meal plan first.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                if (ViewModel.GeneratedMeals.Count == 0)
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "No Meals",
                        Content = "No meals to save. Please generate a meal plan first.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                await ViewModel.SaveToDailyLogAsync();

                var messageText = $"Successfully saved {ViewModel.GeneratedMeals.Count} meals to daily log:\n\n";
                foreach (var meal in ViewModel.GeneratedMeals)
                {
                    messageText += $"� {meal.Name}: {meal.Calories} kcal\n";
                }

                var successDialog = new ContentDialog
                {
                    Title = "Success",
                    Content = messageText,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await successDialog.ShowAsync();

                StatusMessageText.Text = $"All {ViewModel.GeneratedMeals.Count} meals saved to daily log!";
            }
            catch (Exception ex)
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Save Failed",
                    Content = $"Failed to save to daily log:\n\n{ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private async void RegenerateTestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ViewModel.RegenerateMealPlanForTestingAsync();
                UpdateMealsList();
            }
            catch (Exception ex)
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Regeneration Failed",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private async void AddMealToLogsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.DataContext is MealViewModel meal)
                {
                    await ViewModel.SaveMealToDailyLogAsync(meal.Id);
                    StatusMessageText.Text = $"{meal.Name} added to daily log.";
                }
            }
            catch (Exception ex)
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Save Failed",
                    Content = $"Failed to add meal to daily log:\n\n{ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
    }
}
