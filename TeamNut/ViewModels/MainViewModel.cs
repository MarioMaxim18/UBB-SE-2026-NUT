using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Services;
using TeamNut.Services.Interfaces;

public partial class MainViewModel : ObservableObject
{
    private readonly IReminderService _reminderService;

    public MainViewModel(IReminderService reminderService)
    {
        _reminderService = reminderService;
    }

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

   
    public async Task LoadHeaderData()
    {
        await UpdateHeaderReminder();
    }

}
