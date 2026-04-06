using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Services;

public partial class MainViewModel : ObservableObject
{
    private readonly ReminderService _reminderService = new();

    [ObservableProperty]
    private string _nextReminderText = "Loading...";

    public async Task UpdateHeaderReminder()
    {
        
        int userId = UserSession.UserId ?? 0;

        if (userId != 0)
        {
            var next = await _reminderService.GetNextReminder(userId);
            NextReminderText = next != null
                ? $"{next.Name} at {next.Time:hh\\:mm}"
                : "No upcoming meals";
        }
    }
}