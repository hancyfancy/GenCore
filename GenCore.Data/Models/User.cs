using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCore.Data.Models
{
    public class User
    {
        public long UserId { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Role { get; set; }

        public string Token { get; set; }

        public DateTime Expiry { get; set; }

        public DateTime LastActive { get; set; }
    }
}
