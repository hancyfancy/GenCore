using Dapper;
using GenCore.Data.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCore.Data.Repositories.Implementation
{
    public class RolesRepository : IRolesRepository
    {
        private readonly string _connectionString;

        public RolesRepository(string connectionString)
        {
            _connectionString = connectionString;
            CreateTable();
        }

        private int CreateTable()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"IF 
	                                    (NOT EXISTS (SELECT TABLE_CATALOG FROM INFORMATION_SCHEMA.TABLES  
                                                        WHERE TABLE_SCHEMA = 'auth' 
                                                        AND  TABLE_NAME = 'roles'))
                                    BEGIN
                                        CREATE TABLE auth.roles (
											RoleId BIGINT IDENTITY (1, 1) PRIMARY KEY,
											Role NVARCHAR (100) NOT NULL CHECK (Role = 'User' OR Role = 'Specialist' OR Role = 'Admin'),
											SubRole NVARCHAR (100) NOT NULL CHECK (SubRole = 'Standard' OR SubRole = 'Bronze' OR SubRole = 'Silver' OR SubRole = 'Gold' OR SubRole = 'Platinum')
										)
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

        private int DropTable()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"DROP TABLE IF EXISTS auth.userroles
                                    DROP TABLE IF EXISTS auth.roles";

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
