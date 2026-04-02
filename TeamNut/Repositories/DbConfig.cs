using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamNut.Repositories
{
    internal class DbConfig
    {
        public static string ConnectionString => @"Server=(localdb)\TeamNutInstance;Database=NUTdb;Trusted_Connection=True;TrustServerCertificate=True;";
    }
}
