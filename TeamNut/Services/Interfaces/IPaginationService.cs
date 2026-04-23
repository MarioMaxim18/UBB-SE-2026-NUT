// <copyright file="IPaginationService.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

namespace TeamNut.Services.Interfaces
{
    using System.Collections.Generic;

    public interface IPaginationService
    {
        List<T> GetPage<T>(IEnumerable<T> items, int currentPage, int pageSize);
        int GetTotalPages(int totalItems, int pageSize);
        bool IsValidPage(int currentPage, int totalPages);
    }
}
