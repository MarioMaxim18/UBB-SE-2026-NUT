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
                    var parentDirectory = Directory.GetParent(directory);
                    directory = parentDirectory?.FullName;
                }

                string databasePath = Path.Combine(
                    directory ?? EmptyPathFallback,
                    DatabaseFileName);

                return string.Format(ConnectionStringFormat, databasePath);
            }
        }
    }
}