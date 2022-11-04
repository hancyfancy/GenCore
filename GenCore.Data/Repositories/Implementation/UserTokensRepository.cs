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
    public class UserTokensRepository : ConnectionBase, IUserTokensRepository
    {
        public UserTokensRepository(string connectionString) : base(connectionString)
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

                    string sql = $@"USE {_database}

                                    IF 
	                                    (NOT EXISTS (SELECT TABLE_CATALOG FROM INFORMATION_SCHEMA.TABLES  
                                                        WHERE TABLE_SCHEMA = 'auth' 
                                                        AND  TABLE_NAME = 'usertokens')) 
                                    AND
	                                    (EXISTS (SELECT TABLE_CATALOG FROM INFORMATION_SCHEMA.TABLES  
                                                        WHERE TABLE_SCHEMA = 'auth' 
                                                        AND  TABLE_NAME = 'users'))
                                    BEGIN
                                        CREATE TABLE auth.usertokens (
											UserTokenId BIGINT IDENTITY (1, 1) PRIMARY KEY,
											UserId BIGINT NOT NULL FOREIGN KEY REFERENCES auth.users(UserId) ON DELETE CASCADE,
											Token NVARCHAR (200) NOT NULL CHECK ((LEN(Token) = 200) OR (LEN(Token) = 6)),
											RefreshAt DATETIME NOT NULL CHECK (RefreshAt > GETDATE()),
											UNIQUE (UserId),
											UNIQUE (Token)
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

                    string sql = $@"USE {_database}

                                    DROP TABLE IF EXISTS auth.usertokens";

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

        public string InsertOrUpdate(long userId, string token)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"USE {_database}

                                    IF EXISTS (SELECT UserTokenId FROM auth.usertokens WHERE UserId = @UserId)
                                    BEGIN
	                                    IF (SELECT RefreshAt FROM auth.usertokens WHERE UserId = @UserId) > GETUTCDATE()
	                                    BEGIN
		                                    UPDATE 
			                                    auth.usertokens
		                                    SET	
			                                    Token = @Token,
			                                    RefreshAt = @RefreshAt
                                            OUTPUT inserted.Token
		                                    WHERE
			                                    UserId = @UserId
	                                    END
                                    END
                                    ELSE
                                    BEGIN
                                        INSERT INTO auth.usertokens (UserId, Token, RefreshAt)
									    OUTPUT inserted.Token 
	                                    VALUES (@UserId, @Token, @RefreshAt)
                                    END";
                    var result = connection.ExecuteScalar<string>(sql, new
                    {
                        UserId = userId,
                        Token = token,
                        RefreshAt = DateTime.UtcNow.AddDays(1)
                    });

                    connection.Close();

                    return result;
                }
            }
            catch (Exception e)
            {
                return default;
            }
        }

        public UserToken Get(long userId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"USE {_database}

                                    SELECT 
                                        t.UserTokenId,
										t.Token,
										t.RefreshAt
                                    FROM 
	                                    auth.usertokens t
									WHERE
										t.UserId = @UserId";
                    var result = connection.Query<UserToken>(sql, new
                    {
                        UserId = userId
                    });

                    connection.Close();

                    return result.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                return default;
            }
        }

        public User GetUser(string token)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"USE {_database}

                                    SELECT 
                                        u.UserId,
										u.Username,
										u.Email,
										u.Phone,
										u.LastActive,
										t.RefreshAt AS Expiry
                                    FROM 
	                                    auth.usertokens t
										INNER JOIN auth.users u on u.UserId = t.UserId
									WHERE
										t.Token = @Token";
                    var result = connection.Query<User>(sql, new
                    {
                        Token = token
                    });

                    connection.Close();

                    return result.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                return default;
            }
        }
    }
}
