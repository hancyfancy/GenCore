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
    public class ProductAuditRepository : IProductAuditRepository
    {
        private readonly string _connectionString;

        public ProductAuditRepository(string connectionString)
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
                                                        WHERE TABLE_SCHEMA = 'audit' 
                                                        AND  TABLE_NAME = 'products')) 
                                    AND
	                                    (EXISTS (SELECT TABLE_CATALOG FROM INFORMATION_SCHEMA.TABLES  
                                                        WHERE TABLE_SCHEMA = 'production' 
                                                        AND  TABLE_NAME = 'products'))
                                    BEGIN
                                        CREATE TABLE audit.products (
											ProductAuditId BIGINT IDENTITY (1, 1) PRIMARY KEY,
											ProductId BIGINT NOT NULL,
											EventType NVARCHAR (250) NOT NULL CHECK (LEN(EventType) > 0),
											LoginName NVARCHAR (250) NOT NULL CHECK (LEN(LoginName) > 0),
											ObjJson NVARCHAR (MAX) NOT NULL CHECK (LEN(ObjJson) > 0),
											AuditDateTime DATETIME NOT NULL
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

                    string sql = $@"DROP TABLE IF EXISTS audit.products";

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

        public IEnumerable<ProductAudit> GetUpdates(long productId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"SELECT 
                                    p.ProductAuditId,
	                                p.ObjJson,
	                                p.AuditDateTime
                                FROM 
	                                audit.products p
                                WHERE
	                                p.EventType = 'UPDATE'
									AND p.ProductId = @ProductId
								ORDER BY 
									p.AuditDateTime";
                    var result = connection.Query<ProductAudit>(sql, new
                    {
                        ProductId = productId
                    });

                    connection.Close();

                    return result;
                }
            }
            catch (Exception e)
            {
                return new List<ProductAudit>();
            }
        }
    }
}
