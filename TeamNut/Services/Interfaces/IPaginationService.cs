using System.Collections.Generic;

namespace TeamNut.Services.Interfaces
{
    public interface IPaginationService
    {
        List<T> GetPage<T>(IEnumerable<T> items, int currentPage, int pageSize);
        int GetTotalPages(int totalItems, int pageSize);
        bool IsValidPage(int currentPage, int totalPages);
    }
}
