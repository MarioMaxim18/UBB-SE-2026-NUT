// <copyright file="IDbConfig.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamNut.Repositories.Interfaces
{
    public interface IDbConfig
    {
        string ConnectionString { get; }
    }
}
