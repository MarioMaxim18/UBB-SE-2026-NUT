using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamNut.Repositories
{
    internal class DbConfig
    {
        public static string ConnectionString =>
@"Data Source=np:\\.\pipe\LOCALDB#B8524553\tsql\query;Initial Catalog=NUTdb;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;";
    }
}