namespace TeamNut.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TeamNut.Services.Interfaces;

    public class PaginationService : IPaginationService
    {
        public List<T> GetPage<T>(IEnumerable<T> items, int currentPage, int pageSize)
        {
            if (items == null || pageSize <= 0 || currentPage < 1)
            {
                return new List<T>();
            }

            return items
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public int GetTotalPages(int totalItems, int pageSize)
        {
            if (pageSize <= 0)
            {
                return 1;
            }

            return Math.Max(1, (int)Math.Ceiling((double)totalItems / pageSize));
        }

        public bool IsValidPage(int currentPage, int totalPages)
        {
            return currentPage >= 1 && currentPage <= totalPages;
        }
    }
}