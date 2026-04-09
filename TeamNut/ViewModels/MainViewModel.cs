using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Services;
public partial class MainViewModel : ObservableObject
{
    private readonly ReminderService _reminderService = new();
    private const int InvalidUserId = 0;
    private const string LoadingReminderText = "Loading...";
    private const string NoUpcomingMealsText = "No upcoming meals";
    private const string ReminderDisplayFormat = "{0} at {1}";
    private const string ReminderTimeFormat = @"hh\:mm";
    [ObservableProperty]
    private string _nextReminderText = LoadingReminderText;
    public async Task UpdateHeaderReminder()
    {
        int userId = UserSession.UserId ?? InvalidUserId;

        if (userId != InvalidUserId)
        {
            var next = await _reminderService.GetNextReminder(userId);

            NextReminderText = next != null
                ? string.Format(
                    ReminderDisplayFormat,
                    next.Name,
                    next.Time.ToString(ReminderTimeFormat)
                )
                : NoUpcomingMealsText;
        }
    }
    public async Task LoadHeaderData()
    {
        await UpdateHeaderReminder();
    }
}
