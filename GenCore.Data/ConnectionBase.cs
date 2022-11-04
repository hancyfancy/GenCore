using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCore.Data
{
    public class ConnectionBase
    {
        protected readonly string _connectionString;
        protected readonly string _database;

        public ConnectionBase(string connectionString)
        {
            _connectionString = connectionString;
            _database = connectionString.Split(";")[1].Split("=")[1].Trim();
        }
    }
}
