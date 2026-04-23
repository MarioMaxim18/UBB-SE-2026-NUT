// <copyright file="IDailyLogRepository.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

﻿using System;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Repositories.Interfaces
{
    public interface IDailyLogRepository
    {
        Task Add(DailyLog log);
        Task<DailyLog> GetNutritionTotalsForRange(int userId, DateTime startInclusive, DateTime endExclusive);
        Task<bool> HasAnyLogs(int userId);
    }
}
