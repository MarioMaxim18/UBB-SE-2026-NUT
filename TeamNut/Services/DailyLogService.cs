using System;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;

namespace TeamNut.Services
{
    public class DailyLogService
    {
        private readonly DailyLogRepository _repository;
        private readonly UserRepository _userRepository;

        public DailyLogService()
        {
            _repository = new DailyLogRepository();
            _userRepository = new UserRepository();
        }

        private int GetUserId()
        {
            return UserSession.UserId
                ?? throw new InvalidOperationException("User is not logged in.");
        }

        public async Task<bool> HasAnyLogsAsync()
        {
            return await _repository.HasAnyLogs(GetUserId());
        }

        public async Task<DailyLog> GetTodayTotalsAsync()
        {
            var userId = GetUserId();
            var start = DateTime.Today;
            var end = start.AddDays(1);
            return await _repository.GetNutritionTotalsForRange(userId, start, end);
        }

        public async Task<DailyLog> GetCurrentWeekTotalsAsync()
        {
            var userId = GetUserId();
            var today = DateTime.Today;

            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek = today.AddDays(-diff);
            var endOfWeek = startOfWeek.AddDays(7);

            return await _repository.GetNutritionTotalsForRange(userId, startOfWeek, endOfWeek);
        }

        public async Task<UserData> GetCurrentUserNutritionTargetsAsync()
        {
            return await _userRepository.GetUserDataByUserId(GetUserId());
        }
    }
}
