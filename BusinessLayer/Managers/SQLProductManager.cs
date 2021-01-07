using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces;
using BusinessLayer.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace BusinessLayer.Managers
{
    public class SQLProductManager : IManager<Product>, IProductManager
    {
        private string connectionString;
        public SQLProductManager(string connectionS)
        {
            this.connectionString = connectionS;
        }
        SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
        public IReadOnlyList<Product> HaalOp()
        {
            SqlConnection connection = GetConnection();
            List<Product> productL = new List<Product>();
            string query = "SELECT * FROM [Bestellingssysteem].[dbo].[PRODUCT]";
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                connection.Open();
                try
                {
                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        Product product = new Product((long)dataReader["PRODUCT_ID"], (string)dataReader["NAME"], Convert.ToDouble(dataReader["PRICE"]));
                        productL.Add(product);
                    }
                    dataReader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    connection.Close();
                }
            }
            return productL.AsReadOnly();
        }

        public IReadOnlyList<Product> HaalOp(Func<Product, bool> predicate)
        {
            SqlConnection connection = GetConnection();
            List<Product> productL = new List<Product>();
            string query = "SELECT * FROM [Bestellingssysteem].[dbo].[PRODUCT]";
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                connection.Open();
                try
                {
                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        Product product = new Product((long)dataReader["PRODUCT_ID"], (string)dataReader["NAME"], Convert.ToDouble(dataReader["PRICE"]));
                        productL.Add(product);
                    }
                    dataReader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    connection.Close();
                }
            }
            return productL.Where<Product>(predicate).ToList();
        }

        public Product HaalOp(long id)
        {
            SqlConnection connection = GetConnection();
            string query = "SELECT * FROM [Bestellingssysteem].[dbo].[PRODUCT] WHERE PRODUCT_ID=@id";
            using (SqlCommand command = connection.CreateCommand())
            {
                command.Parameters.Add(new SqlParameter("@id", SqlDbType.BigInt));
                command.CommandText = query;
                command.Parameters["@id"].Value = id;
                connection.Open();
                try
                {
                    if (HeeftProduct(id, connection) == false)
                        return null;
                    IDataReader dataReader = command.ExecuteReader();
                    dataReader.Read();
                    Product product = new Product((long)dataReader["PRODUCT_ID"], (string)dataReader["NAME"], Convert.ToDouble(dataReader["PRICE"]));
                    dataReader.Close();
                    return product;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return null;
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        private bool HeeftProduct(long id, SqlConnection sqlConnection = null)
        {
            SqlConnection connection;
            if (sqlConnection is null)
                connection = GetConnection();
            else connection = sqlConnection;
            string query = "SELECT count(*) FROM [Bestellingssysteem].[dbo].[PRODUCT] WHERE PRODUCT_ID=@id";

            using (SqlCommand command = connection.CreateCommand())
            {
                if (connection.State != ConnectionState.Open) connection.Open();
                try
                {
                    command.Parameters.Add(new SqlParameter("@id", SqlDbType.BigInt));
                    command.CommandText = query;
                    command.Parameters["@id"].Value = id;
                    int n = (int)command.ExecuteScalar();
                    if (n > 0) return true; else return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    if (sqlConnection is null) connection.Close();
                }
            }
            return false;
        }
        public Product HaalOp(string naam)
        {
            SqlConnection connection = GetConnection();
            string query = "SELECT * FROM [Bestellingssysteem].[dbo].[PRODUCT] WHERE NAME=@naam";
            using (SqlCommand command = connection.CreateCommand())
            {
                command.Parameters.Add(new SqlParameter("@naam", SqlDbType.NVarChar));
                command.CommandText = query;
                command.Parameters["@naam"].Value = naam;
                connection.Open();
                try
                {
                    if (HeeftProduct(naam, connection) == false)
                        return null;
                    IDataReader dataReader = command.ExecuteReader();
                    dataReader.Read();
                    Product product = new Product((long)dataReader["PRODUCT_ID"], (string)dataReader["NAME"], Convert.ToDouble(dataReader["PRICE"]));
                    dataReader.Close();
                    return product;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return null;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private bool HeeftProduct(string naam, SqlConnection sqlConnection = null)
        {
            SqlConnection connection;
            if (sqlConnection is null)
                connection = GetConnection();
            else connection = sqlConnection;
            string query = "SELECT count(*) FROM [Bestellingssysteem].[dbo].[PRODUCT] WHERE NAME=@naam";

            using (SqlCommand command = connection.CreateCommand())
            {
                if (connection.State != ConnectionState.Open) connection.Open();
                try
                {
                    command.Parameters.Add(new SqlParameter("@naam", SqlDbType.BigInt));
                    command.CommandText = query;
                    command.Parameters["@naam"].Value = naam;
                    int n = (int)command.ExecuteScalar();
                    if (n > 0) return true; else return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    if (sqlConnection is null) connection.Close();
                }
            }
            return false;
        }
        public void Verwijder(Product anItem)
        {
            SqlConnection connection = GetConnection();
            string query = "DELETE FROM [Bestellingssysteem].[dbo].[PRODUCT] WHERE PRODUCT_ID=@id";

            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    command.Parameters.Add(new SqlParameter("@id", SqlDbType.BigInt));
                    command.CommandText = query;
                    command.Parameters["@id"].Value = anItem.ProductId;
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public void VoegToe(Product anItem)
        {
            SqlConnection connection = GetConnection();
            string query = "INSERT INTO [Bestellingssysteem].[dbo].[PRODUCT](NAME,PRICE) VALUES(@naam,@prijs)";
            string queryU = "UPDATE [Bestellingssysteem].[dbo].[PRODUCT] SET NAME = @naam, PRICE = @prijs WHERE PRODUCT_ID = @id";

            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    command.Parameters.Add(new SqlParameter("@naam", SqlDbType.NVarChar));
                    command.Parameters.Add(new SqlParameter("@prijs", SqlDbType.Decimal));
                    
                    command.Parameters["@naam"].Value = anItem.Naam;
                    command.Parameters["@prijs"].Value = (decimal)anItem.Prijs;
                    if (HeeftProduct(anItem.ProductId, connection) == false)
                    {
                        command.CommandText = query;
                    }
                    else
                    {
                        command.CommandText = queryU;
                        command.Parameters.Add(new SqlParameter("@id", SqlDbType.BigInt));
                        command.Parameters["@id"].Value = anItem.ProductId;
                    }
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}
