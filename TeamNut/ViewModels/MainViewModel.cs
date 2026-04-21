using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using TeamNut.Models;
using TeamNut.Services;
using TeamNut.Services.Interfaces;

/// <summary>View model for the main page header area.</summary>
public partial class MainViewModel : ObservableObject
{
    private const int InvalidUserId = 0;
    private const string LoadingReminderText = "Loading...";
    private const string NoUpcomingMealsText = "No upcoming meals";
    private const string ReminderDisplayFormat = "{0} at {1}";
    private const string ReminderTimeFormat = @"hh\:mm";
    private readonly IReminderService reminderService;

    public MainViewModel(IReminderService rreminderService)
    {
        reminderService = rreminderService;
    }

    /// <summary>Gets or sets the text for the next upcoming reminder.</summary>
    [ObservableProperty]
    public partial string NextReminderText { get; set; }

    /// <summary>Initializes a new instance of the <see cref="MainViewModel"/> class.</summary>
    public MainViewModel()
    {
        NextReminderText = LoadingReminderText;
    }

    /// <summary>Refreshes the next reminder text from the service.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task UpdateHeaderReminder()
    {
        int userId = UserSession.UserId ?? InvalidUserId;

        if (userId != InvalidUserId)
        {
            var next = await reminderService.GetNextReminder(userId);

            NextReminderText = next != null
                ? string.Format(
                    ReminderDisplayFormat,
                    next.Name,
                    next.Time.ToString(ReminderTimeFormat))
                : NoUpcomingMealsText;
        }
    }

    /// <summary>Loads all header data for the main page.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task LoadHeaderData()
    {
        await UpdateHeaderReminder();
    }
}
