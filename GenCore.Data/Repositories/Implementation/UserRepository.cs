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
    public class UserRepository : ConnectionBase, IUserRepository
    {
        public UserRepository(string connectionString) : base(connectionString)
        {
            CreateTable();
            CreateDeleteTrigger();
        }

        private int CreateDeleteTrigger()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"USE {_database}

                                    IF 
	                                    (EXISTS (SELECT TABLE_CATALOG FROM INFORMATION_SCHEMA.TABLES  
                                                        WHERE TABLE_SCHEMA = 'auth' 
                                                        AND  TABLE_NAME = 'users'))
									AND
										(NOT EXISTS (SELECT type_desc FROM sys.triggers WHERE object_id = OBJECT_ID(N'auth.users_tr_delete')))
                                    BEGIN
                                        CREATE TRIGGER auth.users_tr_delete
										ON {_database}.auth.users
										AFTER DELETE
										AS
										BEGIN
											SET NOCOUNT ON

											DECLARE @Id BIGINT
											SELECT @Id = UserId FROM deleted

											DELETE FROM 
												{_database}.auth.userroles
											WHERE
												UserId = @Id
										END
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

        private int DropTriggers()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"USE {_database}

                                    DROP TRIGGER IF EXISTS auth.users_tr_delete";

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

                    string sql = $@"USE {_database}

                                    DROP TABLE IF EXISTS auth.userencryption
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

                    string sql = $@"USE {_database}

                                    BEGIN
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

                    string sql = $@"USE {_database}

                                    UPDATE
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
