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
    public class AuthSchema : ConnectionBase, ISchema
    {
        public AuthSchema(string connectionString) : base(connectionString)
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
                                                    WHERE object_id = OBJECT_ID(N'auth' )
                                    BEGIN
	                                    CREATE SCHEMA auth
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

                                    DROP SCHEMA IF EXISTS auth";

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
