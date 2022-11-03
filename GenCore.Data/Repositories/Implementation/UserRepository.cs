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
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
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
