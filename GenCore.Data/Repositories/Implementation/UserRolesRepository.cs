using Dapper;
using GenCore.Data.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCore.Data.Repositories.Implementation
{
    public class UserRolesRepository : IUserRolesRepository
    {
        private readonly string _connectionString;

        public UserRolesRepository(string connectionString)
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
                                                        AND  TABLE_NAME = 'userroles')) 
                                    AND
	                                    (EXISTS (SELECT TABLE_CATALOG FROM INFORMATION_SCHEMA.TABLES  
                                                        WHERE TABLE_SCHEMA = 'auth' 
                                                        AND  TABLE_NAME = 'users'))
									AND
	                                    (EXISTS (SELECT TABLE_CATALOG FROM INFORMATION_SCHEMA.TABLES  
                                                        WHERE TABLE_SCHEMA = 'auth' 
                                                        AND  TABLE_NAME = 'roles'))
                                    BEGIN
                                        CREATE TABLE auth.userroles (
											UserRoleId BIGINT IDENTITY (1, 1) PRIMARY KEY,
											UserId BIGINT NOT NULL FOREIGN KEY REFERENCES auth.users(UserId) ON DELETE NO ACTION,
											RoleId BIGINT NOT NULL FOREIGN KEY REFERENCES auth.roles(RoleId) ON DELETE NO ACTION,
											UNIQUE (UserId, RoleId),
											UNIQUE (UserId)
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

                    string sql = $@"DROP TABLE IF EXISTS auth.userroles";

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

        public int Insert(long userId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"BEGIN
                                   IF NOT EXISTS (SELECT UserRoleId FROM auth.userroles WHERE UserId = @UserId)
                                   BEGIN
                                        INSERT INTO auth.userroles
		                                (                    
			                                UserId,
			                                RoleId
		                                )
		                                VALUES 
		                                ( 
			                                @UserId,
			                                (SELECT RoleId FROM auth.roles WHERE Role = 'User' AND SubRole = 'Standard')
		                                )
                                   END
                                END";
                    var result = connection.Execute(sql, new
                    {
                        UserId = userId
                    });

                    connection.Close();

                    return result;
                }
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public int Update(long userId, string role, string subRole)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"UPDATE
	                                auth.userroles
                                SET
	                                RoleId = (SELECT RoleId FROM auth.roles WHERE Role = @Role AND SubRole = @SubRole)
                                WHERE
                                    UserId = @UserId";
                    var result = connection.Execute(sql, new
                    {
                        UserId = userId,
                        Role = role,
                        SubRole = subRole
                    });

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
