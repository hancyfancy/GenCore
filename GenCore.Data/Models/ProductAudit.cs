using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCore.Data.Models
{
    public class ProductAudit
    {
        public long ProductAuditId { get; set; }

        public string ObjJson { get; set; }

        public DateTime AuditDateTime { get; set; }
    }
}
