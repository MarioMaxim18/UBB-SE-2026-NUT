using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamNut.Models
{
    internal static class UserSession
    {
        public static string? Username { get; private set; }
        public static string? Role { get; private set; }
        public static int? UserId { get; private set; }
        public static void Login(int userId, string username, string role)
        {
            UserId = userId;
            Username = username;
            Role = role;
        }
       
        public static void Logout()
        {
            UserId = null;
            Username = null;
            Role = null;
        }
    }
}
