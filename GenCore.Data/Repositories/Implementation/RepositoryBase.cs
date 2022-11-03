﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCore.Data.Repositories.Implementation
{
    public abstract class RepositoryBase
    {
        protected readonly string _connectionString;
        protected readonly string _database;

        public RepositoryBase(string connectionString)
        {
            _connectionString = connectionString;
            _database = connectionString.Split(";")[1].Split("=")[1].Trim();
        }
    }
}
