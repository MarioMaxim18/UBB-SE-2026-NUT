using System;
using System.IO;
using System.Linq;

namespace TeamNut.Repositories
{
    internal class DbConfig
    {
        public static string ConnectionString
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