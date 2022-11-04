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
    public class RolesRepository : ConnectionBase, IRolesRepository
    {
        public RolesRepository(string connectionString) : base(connectionString)
        {
            CreateTable();
            CreateDeleteTrigger();
            LoadData();
        }

        private int LoadData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"USE {_database}

                                    IF NOT EXISTS (SELECT 1 FROM auth.roles)
									BEGIN
										INSERT INTO auth.roles (Role, SubRole) VALUES ('Admin', 'Standard')
										INSERT INTO auth.roles (Role, SubRole) VALUES ('Specialist', 'Standard')
										INSERT INTO auth.roles (Role, SubRole) VALUES ('User', 'Standard')
										INSERT INTO auth.roles (Role, SubRole) VALUES ('User', 'Bronze')
										INSERT INTO auth.roles (Role, SubRole) VALUES ('User', 'Silver')
										INSERT INTO auth.roles (Role, SubRole) VALUES ('User', 'Gold')
										INSERT INTO auth.roles (Role, SubRole) VALUES ('User', 'Platinum')
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
                                                        AND  TABLE_NAME = 'roles'))
									AND
										(NOT EXISTS (SELECT type_desc FROM sys.triggers WHERE object_id = OBJECT_ID(N'auth.roles_tr_delete')))
                                    BEGIN
                                        CREATE TRIGGER auth.roles_tr_delete
										ON {_database}.auth.roles
										AFTER DELETE
										AS
										BEGIN
											SET NOCOUNT ON

											DECLARE @Id BIGINT
											SELECT @Id = RoleId FROM deleted

											DELETE FROM 
												{_database}.auth.userroles
											WHERE
												RoleId = @Id
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

                                    DROP TRIGGER IF EXISTS auth.roles_tr_delete";

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

                    string sql = $@"USE {_database}

                                    DROP TABLE IF EXISTS auth.userroles
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
