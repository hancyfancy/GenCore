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
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;
        private readonly ISqlConverter _sqlConverter;

        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
            _sqlConverter = new JsonToSqlUpdateParameterConverter();
        }

        public IEnumerable<Product> Get()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $@"SELECT 
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

                    string sql = $@"INSERT INTO production.products
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

                    string sql = $@"UPDATE
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

                    string sql = $@"DELETE FROM
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
