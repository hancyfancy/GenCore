using Dapper;
using GenConversion.Service.Utilities.Implementation;
using GenConversion.Service.Utilities.Interface;
using GenCore.Data.Extensions;
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
    public class ProductRepository : RepositoryBase, IProductRepository
    {
        private readonly ISqlConverter _sqlConverter;

        public ProductRepository(string connectionString) : base(connectionString)
        {
            _sqlConverter = new JsonToSqlUpdateParameterConverter();
            CreateTable();
            CreateDeleteTrigger();
            CreateInsertTrigger();
            CreateUpdateTrigger();
            CreateTestDataStoredProcedure();
            LoadTestData(1300);
        }

        private int LoadTestData(int dataSize)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"USE {_database}

                                    DECLARE @i INT = 0
                                    DECLARE @TypeEnum SMALLINT

                                    WHILE @i < {dataSize}   
                                    BEGIN
                                        SET @i = @i + 1
                                        SET @TypeEnum = CONVERT(SMALLINT, 1 + (6-1)*RAND(CHECKSUM(NEWID())))
	                                    EXEC production.products_sp @TypeEnum
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

        private int CreateTestDataStoredProcedure()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"USE {_database}

                                    IF 
	                                    (EXISTS (SELECT TABLE_CATALOG FROM INFORMATION_SCHEMA.TABLES  
                                                                                            WHERE TABLE_SCHEMA = 'production' 
                                                                                            AND  TABLE_NAME = 'products'))
                                    AND
	                                    (NOT EXISTS (
                                            SELECT type_desc
                                            FROM sys.procedures WITH(NOLOCK)
                                            WHERE object_id = OBJECT_ID(N'production.products_sp')
                                          ))
                                    BEGIN
	                                    CREATE PROCEDURE production.products_sp
		                                    @TypeEnum SMALLINT = NULL
	                                    AS 
	                                    BEGIN
		                                    INSERT INTO production.products
		                                    (                    
			                                    Name,
			                                    Price,
			                                    Type,
			                                    Active
		                                    ) 
		                                    VALUES 
		                                    ( 
			                                    CONVERT(NVARCHAR(100), NEWID()),
			                                    CONVERT(DECIMAL(18, 2), 5 + (2000-5)*RAND(CHECKSUM(NEWID()))),
			                                    CASE @TypeEnum WHEN 1 THEN 'Books' WHEN 2 THEN 'Electronics' WHEN 3 THEN 'Food' WHEN 4 THEN 'Furniture' WHEN 5 THEN 'Toys' ELSE 'NA' END,
			                                    ABS(CHECKSUM(NEWID())) % 2
		                                    )
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

        private int DropStoredProcedures()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"USE {_database}

                                    DROP PROCEDURE IF EXISTS production.products_sp";

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
                                                        WHERE TABLE_SCHEMA = 'production' 
                                                        AND  TABLE_NAME = 'products'))
									--AND
									--	(EXISTS (SELECT TABLE_CATALOG FROM INFORMATION_SCHEMA.TABLES  
									--                    WHERE TABLE_SCHEMA = 'audit' 
									--                    AND  TABLE_NAME = 'products'))
									AND
										(NOT EXISTS (SELECT type_desc FROM sys.triggers WHERE object_id = OBJECT_ID(N'production.products_tr_delete')))
                                    BEGIN
                                        CREATE TRIGGER production.products_tr_delete
										ON {_database}.production.products
										AFTER DELETE
										AS
										BEGIN
											SET NOCOUNT ON

											DECLARE @Id BIGINT
											SELECT @Id = ProductId FROM deleted

											DECLARE @Name NVARCHAR(100)
											SELECT @Name = Name FROM deleted

											DECLARE @Price DECIMAL(18,2)
											SELECT @Price = Price FROM deleted

											DECLARE @Type NVARCHAR(50)
											SELECT @Type = Type FROM deleted

											DECLARE @Active BIT
											SELECT @Active = Active FROM deleted

											INSERT INTO {_database}.audit.products 
											(ProductId, EventType, LoginName, ObjJson, AuditDateTime)
											VALUES
											(
											@Id,
											'DELETE',
											CONVERT(NVARCHAR(250), CURRENT_USER),
											'{{ ""Name"" : ""' + @Name + '"", ""Price"" : ' + CAST(@Price AS NVARCHAR(max)) + ', ""Type"" : ""' + @Type + '"", ""Active"" : ' + CAST(@Active AS NVARCHAR(max)) + ' }}',
											GETDATE()
											) 
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

        private int CreateInsertTrigger()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"USE {_database}

                                    IF 
	                                    (EXISTS (SELECT TABLE_CATALOG FROM INFORMATION_SCHEMA.TABLES  
                                                        WHERE TABLE_SCHEMA = 'production' 
                                                        AND  TABLE_NAME = 'products'))
									--AND
									--	(EXISTS (SELECT TABLE_CATALOG FROM INFORMATION_SCHEMA.TABLES  
									--                    WHERE TABLE_SCHEMA = 'audit' 
									--                    AND  TABLE_NAME = 'products'))
									AND
										(NOT EXISTS (SELECT type_desc FROM sys.triggers WHERE object_id = OBJECT_ID(N'production.products_tr_insert')))
                                    BEGIN
                                        CREATE TRIGGER production.products_tr_insert
										ON {_database}.production.products
										AFTER INSERT
										AS
										BEGIN
											SET NOCOUNT ON

											DECLARE @Id BIGINT
											SELECT @Id = ProductId FROM inserted

											DECLARE @Name NVARCHAR(100)
											SELECT @Name = Name FROM inserted

											DECLARE @Price DECIMAL(18,2)
											SELECT @Price = Price FROM inserted

											DECLARE @Type NVARCHAR(50)
											SELECT @Type = Type FROM inserted

											DECLARE @Active BIT
											SELECT @Active = Active FROM inserted

											INSERT INTO {_database}.audit.products 
											(ProductId, EventType, LoginName, ObjJson, AuditDateTime)
											VALUES
											(
											@Id,
											'INSERT',
											CONVERT(NVARCHAR(250), CURRENT_USER),
											'{{ ""Name"" : ""' + @Name + '"", ""Price"" : ' + CAST(@Price AS NVARCHAR(max)) + ', ""Type"" : ""' + @Type + '"", ""Active"" : ' + CAST(@Active AS NVARCHAR(max)) + ' }}',
											GETDATE()
											) 
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

        private int CreateUpdateTrigger()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"USE {_database}

                                    IF 
	                                    (EXISTS (SELECT TABLE_CATALOG FROM INFORMATION_SCHEMA.TABLES  
                                                        WHERE TABLE_SCHEMA = 'production' 
                                                        AND  TABLE_NAME = 'products'))
									--AND
									--	(EXISTS (SELECT TABLE_CATALOG FROM INFORMATION_SCHEMA.TABLES  
									--                    WHERE TABLE_SCHEMA = 'audit' 
									--                    AND  TABLE_NAME = 'products'))
									AND
										(NOT EXISTS (SELECT type_desc FROM sys.triggers WHERE object_id = OBJECT_ID(N'production.products_tr_update')))
                                    BEGIN
                                        CREATE TRIGGER production.products_tr_update
										ON {_database}.production.products
										AFTER UPDATE
										AS
										BEGIN
											SET NOCOUNT ON

											DECLARE @DeletedId BIGINT
											SELECT @DeletedId = ProductId FROM deleted

											DECLARE @DeletedName NVARCHAR(100)
											SELECT @DeletedName = Name FROM deleted

											DECLARE @DeletedPrice DECIMAL(18,2)
											SELECT @DeletedPrice = Price FROM deleted

											DECLARE @DeletedType NVARCHAR(50)
											SELECT @DeletedType = Type FROM deleted

											DECLARE @DeletedActive BIT
											SELECT @DeletedActive = Active FROM deleted

											DECLARE @InsertedId BIGINT
											SELECT @InsertedId = ProductId FROM inserted

											DECLARE @InsertedName NVARCHAR(100)
											SELECT @InsertedName = Name FROM inserted

											DECLARE @InsertedPrice DECIMAL(18,2)
											SELECT @InsertedPrice = Price FROM inserted

											DECLARE @InsertedType NVARCHAR(50)
											SELECT @InsertedType = Type FROM inserted

											DECLARE @InsertedActive BIT
											SELECT @InsertedActive = Active FROM inserted

											INSERT INTO {_database}.audit.products 
											(ProductId, EventType, LoginName, ObjJson, AuditDateTime)
											VALUES
											(
											@InsertedId,
											'UPDATE',
											CONVERT(NVARCHAR(250), CURRENT_USER),
											'[{{ ""Name"" : ""' + @DeletedName + '"", ""Price"" : ' + CAST(@DeletedPrice AS NVARCHAR(max)) + ', ""Type"" : ""' + @DeletedType + '"", ""Active"" : ' + CAST(@DeletedActive AS NVARCHAR(max)) + ' }},{{ ""Name"" : ""' + @InsertedName + '"", ""Price"" : ' + CAST(@InsertedPrice AS NVARCHAR(max)) + ', ""Type"" : ""' + @InsertedType + '"", ""Active"" : ' + CAST(@InsertedActive AS NVARCHAR(max)) + ' }}]',
											GETDATE()
											) 
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

                                    DROP TRIGGER IF EXISTS production.products_tr_delete
                                    DROP TRIGGER IF EXISTS production.products_tr_insert
                                    DROP TRIGGER IF EXISTS production.products_tr_update";

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
                                                        WHERE TABLE_SCHEMA = 'production' 
                                                        AND  TABLE_NAME = 'products')) 
                                    BEGIN
                                        CREATE TABLE production.products (
											ProductId BIGINT IDENTITY (1, 1) PRIMARY KEY,
											Name NVARCHAR (100) NOT NULL CHECK (LEN(Name) > 0),
											Price DECIMAL (18,2) NOT NULL CHECK (Price > 0),
											Type NVARCHAR (50) NOT NULL CHECK (Type = 'Toys' OR Type = 'Food' OR Type = 'Electronics' OR Type = 'Furniture' OR Type = 'Books'),
											Active BIT NOT NULL
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

                                    DROP TABLE IF EXISTS production.products";

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

        public IEnumerable<Product> Get()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"USE {_database}

                                    SELECT 
                                        p.ProductId,
	                                    p.Name, 
	                                    p.Price, 
	                                    p.Type, 
	                                    p.Active
                                    FROM 
	                                    production.products p";
                    var result = connection.Query<Product>(sql);

                    connection.Close();

                    return result;
                }
            }
            catch (Exception e)
            {
                return new List<Product>();
            }
        }

        public int Insert(Product product)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"USE {_database}

                                    INSERT INTO production.products
                                    (                    
	                                    Name,
	                                    Price,
	                                    Type,
	                                    Active
                                    )
                                    VALUES 
                                    ( 
	                                    @Name,
	                                    @Price,
	                                    @Type,
	                                    @Active
                                    )";
                    var result = connection.Execute(sql, new
                    {
                        Name = product.Name,
                        Price = product.Price,
                        Type = product.Type.ToString(),
                        Active = product.Active
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

        public int Update(long id, string product)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string updateSql = _sqlConverter.ToSql(product);

                    if (updateSql.IsEmpty())
                    {
                        return 0;
                    }

                    connection.Open();

                    string sql = $@"USE {_database}

                                    UPDATE
	                                    production.products
                                    SET
	                                    {updateSql}
                                    WHERE
                                        ProductId = @Id";
                    var result = connection.Execute(sql, new
                    {
                        Id = id
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

        public int Delete(long id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"USE {_database}

                                    DELETE FROM
	                                    production.products
                                    WHERE
	                                    ProductId = @Id";
                    var result = connection.Execute(sql, new
                    {
                        Id = id
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
