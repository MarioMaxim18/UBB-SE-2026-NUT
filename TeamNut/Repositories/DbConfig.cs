using System;
using System.IO;
using System.Linq;
using TeamNut.Repositories.Interfaces;

namespace TeamNut.Repositories
{
    internal class DbConfig : IDbConfig
    {
        public string ConnectionString
        {
            get
            {
                
                string directory = AppDomain.CurrentDomain.BaseDirectory;

                
                while (directory != null && !Directory.GetFiles(directory, "*.csproj").Any())
                {
                    directory = Directory.GetParent(directory)?.FullName;
                }

                
                string dbPath = Path.Combine(directory ?? "", "NutData.db");
                return $"Data Source={dbPath}";
            }
        }
    }
}