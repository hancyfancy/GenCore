using Dapper;
using GenCore.Data.Models;
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
    public class UserRepository : RepositoryBase, IUserRepository
    {
        public UserRepository(string connectionString) : base(connectionString)
        {
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
                                                        AND  TABLE_NAME = 'users'))
                                    BEGIN
                                        CREATE TABLE auth.users (
											UserId BIGINT IDENTITY (1, 1) PRIMARY KEY,
											Username NVARCHAR (100) NOT NULL CHECK (LEN(Username) > 4),
											Email NVARCHAR (100) NOT NULL,
											Phone NVARCHAR (20) NOT NULL,
											LastActive DATETIME NOT NULL CHECK (LastActive < GETDATE()),
											UNIQUE(Username)
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

                    string sql = $@"DROP TABLE IF EXISTS auth.userencryption
                                    DROP TABLE IF EXISTS auth.usertokens
                                    DROP TABLE IF EXISTS auth.userroles
                                    DROP TABLE IF EXISTS auth.userverification
                                    DROP TABLE IF EXISTS auth.users";

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

        public long Insert(User user)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"BEGIN
                                   IF NOT EXISTS (SELECT UserId FROM auth.users WHERE Username = @Username)
                                   BEGIN
                                        INSERT INTO auth.users
		                                (                    
			                                Username,
			                                Email,
			                                Phone,
			                                LastActive
		                                )
		                                OUTPUT inserted.UserId 
		                                VALUES 
		                                ( 
			                                @Username,
			                                @Email,
			                                @Phone,
			                                @LastActive
		                                )
                                   END
								   ELSE
								   BEGIN
										SELECT UserId FROM auth.users WHERE Username = @Username 
								   END
                                END";
                    var result = connection.ExecuteScalar<long>(sql, new
                    {
                        Username = user.Username,
                        Email = user.Email,
                        Phone = user.Phone,
                        LastActive = user.LastActive,
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

        public int UpdateLastActive(long userId)
        {
            try
            {

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"UPDATE
	                                auth.users
                                SET
	                                LastActive = @LastActive
                                WHERE
                                    UserId = @UserId";
                    var result = connection.Execute(sql, new
                    {
                        UserId = userId,
                        LastActive = DateTime.UtcNow
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
