using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using TeamNut.Models;
using TeamNut.Services;
using TeamNut.Services.Interfaces;

/// <summary>
/// MainViewModel.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private const int InvalidUserId = 0;
    private const string LoadingReminderText = "Loading...";
    private const string NoUpcomingMealsText = "No upcoming meals";
    private const string ReminderDisplayFormat = "{0} at {1}";
    private const string ReminderTimeFormat = @"hh\:mm";

    private readonly IReminderService? reminderService;

    [ObservableProperty]
    public partial string NextReminderText { get; set; }

    public MainViewModel(IReminderService reminderService)
    {
        this.reminderService = reminderService;
        NextReminderText = LoadingReminderText;
    }

    public MainViewModel()
    {
        NextReminderText = LoadingReminderText;
    }

    public async Task UpdateHeaderReminder()
    {
        int userId = UserSession.UserId ?? InvalidUserId;

        if (userId != InvalidUserId)
        {
            if (reminderService == null)
            {
                NextReminderText = NoUpcomingMealsText;
                return;
            }

            var next = await reminderService.GetNextReminder(userId);

            NextReminderText = next != null
                ? string.Format(
                    ReminderDisplayFormat,
                    next.Name,
                    next.Time.ToString(ReminderTimeFormat))
                : NoUpcomingMealsText;
        }
    }

    public async Task LoadHeaderData()
    {
        await UpdateHeaderReminder();
    }
}