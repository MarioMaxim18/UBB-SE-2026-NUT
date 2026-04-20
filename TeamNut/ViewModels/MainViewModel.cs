using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using TeamNut.Models;
using TeamNut.Services;

namespace TeamNut.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ReminderService reminderService = new ReminderService();

        [ObservableProperty]
        public partial string NextReminderText { get; set; } = "Loading...";

        public async Task UpdateHeaderReminder()
        {
            int userId = UserSession.UserId ?? 0;

            if (userId != 0)
            {
                var next = await reminderService.GetNextReminder(userId);
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
}
