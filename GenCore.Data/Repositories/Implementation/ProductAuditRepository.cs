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
