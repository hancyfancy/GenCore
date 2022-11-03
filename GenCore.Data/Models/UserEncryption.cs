using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCore.Data.Models
{
    public class UserEncryption
    {
        public long UserEncryptionId { get; set; }

        public long UserId { get; set; }

        public byte[] EncryptionKey { get; set; }
    }
}
