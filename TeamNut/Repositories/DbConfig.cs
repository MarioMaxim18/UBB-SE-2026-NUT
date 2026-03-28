using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamNut.Repositories
{
    internal class DbConfig
    {
        public static string ConnectionString => @"Data Source=(localdb)\TeamNutInstance;Initial Catalog=NUTdb;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;";
    }
}
