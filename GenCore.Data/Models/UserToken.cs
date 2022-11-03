using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCore.Data.Models
{
    public class UserToken : User
    {
        public long UserTokenId { get; set; }

        public string Token { get; set; }

        public DateTime RefreshAt { get; set; }
    }
}
