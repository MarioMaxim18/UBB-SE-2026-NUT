using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Repositories.Interfaces
{
    public interface IReminderRepository
    {
        Task Add(Reminder entity);
        Task Delete(int id);
        Task<IEnumerable<Reminder>> GetAll();
        Task<IEnumerable<Reminder>> GetAllByUserId(int userId);
        Task<Reminder> GetById(int id);
        Task<Reminder> GetNextReminder(int userId);
        Task Update(Reminder entity);
    }
}