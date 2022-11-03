using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCore.Data.Extensions
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string str)
        {
            try
            {
                return string.IsNullOrWhiteSpace(str);
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
