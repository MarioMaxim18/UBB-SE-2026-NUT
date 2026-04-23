// <copyright file="DbConfig.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

namespace TeamNut.Repositories
{
    using System;
    using System.IO;
    using System.Linq;
    using TeamNut.Repositories.Interfaces;

    internal class DbConfig : IDbConfig
    {
        private const string ProjectFileSearchPattern = "*.csproj";

        private const string DatabaseFileName = "NutData.db";

        private const string ConnectionStringFormat = "Data Source={0}";

        private const string EmptyPathFallback = "";

        public string ConnectionString
        {
            get
            {
                string? directory = AppDomain.CurrentDomain.BaseDirectory;

                while (directory != null && !Directory.GetFiles(directory, ProjectFileSearchPattern).Any())
                {
                    directory = Directory.GetParent(directory)?.FullName;
                }

                string dbPath = Path.Combine(
                    directory ?? EmptyPathFallback,
                    DatabaseFileName);

                return string.Format(ConnectionStringFormat, dbPath);
            }
        }
    }
}
