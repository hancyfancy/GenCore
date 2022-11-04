using Dapper;
using GenCore.Data.Schemas.Interface;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCore.Data.Schemas.Implementation
{
    internal class AuditSchema : ConnectionBase, ISchema
    {
        public AuditSchema(string connectionString) : base(connectionString)
        {
            CreateSchema();
        }

        private int CreateSchema()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"USE {_database}

                                    IF NOT EXISTS ( SELECT  schema_id
                                                    FROM    sys.schemas
                                                    WHERE object_id = OBJECT_ID(N'audit' )
                                    BEGIN
	                                    CREATE SCHEMA audit
                                    END";

                    var result = connection.Execute(sql);

                    connection.Close();

                    return result;
                }
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        private int DropSchema()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"USE {_database}

                                    DROP SCHEMA IF EXISTS audit";

                    var result = connection.Execute(sql);

                    connection.Close();

                    return result;
                }
            }
            catch (Exception e)
            {
                return 0;
            }
        }
    }
}
