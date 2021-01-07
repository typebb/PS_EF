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
    public class SQLBestellingManager : IManager<Bestelling>
    {
        private string connectionString;
        public SQLBestellingManager(string connectionS)
        {
            this.connectionString = connectionS;
        }
        SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
        public IReadOnlyList<Bestelling> HaalOp()
        {
            SqlConnection connection = GetConnection();
            List<Bestelling> besL = new List<Bestelling>();
            string query = "SELECT * FROM [Bestellingssysteem].[dbo].[ORDER] o JOIN [Bestellingssysteem].[dbo].[CUSTOMER] c ON (o.CUSTOMER_ID = c.CUSTOMER_ID)";
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                connection.Open();
                try
                {
                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        Bestelling bestelling = new Bestelling((long)dataReader["ORDER_ID"], new Klant((long)dataReader["CUSTOMER_ID"], (string)dataReader["NAME"], (string)dataReader["ADDRESS"]), (DateTime)dataReader["TIME"]);

                        besL.Add(bestelling);
                    }
                    dataReader.Close();
                    for (int i = 0; i < besL.Count; i++)
                        foreach (KeyValuePair<Product, int> p in FindProducten(besL[i].BestellingId, connection)) besL[i].VoegProductToe(p.Key, p.Value);
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
            return besL.AsReadOnly();
        }

        public IReadOnlyList<Bestelling> HaalOp(Func<Bestelling, bool> predicate)
        {
            SqlConnection connection = GetConnection();
            List<Bestelling> besL = new List<Bestelling>();
            string query = "SELECT * FROM [Bestellingssysteem].[dbo].[ORDER] o JOIN [Bestellingssysteem].[dbo].[CUSTOMER] c ON (o.CUSTOMER_ID = c.CUSTOMER_ID)";
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                connection.Open();
                try
                {
                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        Bestelling bestelling = new Bestelling((long)dataReader["ORDER_ID"], new Klant((long)dataReader["CUSTOMER_ID"], (string)dataReader["NAME"], (string)dataReader["ADDRESS"]), (DateTime)dataReader["TIME"]);
                        besL.Add(bestelling);
                    }
                    dataReader.Close();
                    for (int i = 0; i < besL.Count; i++)
                        foreach (KeyValuePair<Product, int> p in FindProducten(besL[i].BestellingId, connection)) besL[i].VoegProductToe(p.Key, p.Value);
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
            return besL.Where<Bestelling>(predicate).ToList();
        }

        public Bestelling HaalOp(long id)
        {

            SqlConnection connection = GetConnection();
            string query = "SELECT * FROM [Bestellingssysteem].[dbo].[ORDER] o JOIN [Bestellingssysteem].[dbo].[CUSTOMER] c ON (o.CUSTOMER_ID = c.CUSTOMER_ID) WHERE ORDER_ID=@id";
            using (SqlCommand command = connection.CreateCommand())
            {
                command.Parameters.Add(new SqlParameter("@id", SqlDbType.BigInt));
                command.CommandText = query;
                command.Parameters["@id"].Value = id;
                connection.Open();
                try
                {
                    if (HeeftBestelling(id, connection) == false)
                        return null;
                    IDataReader dataReader = command.ExecuteReader();
                    dataReader.Read();
                    Bestelling bestelling = new Bestelling((long)dataReader["ORDER_ID"], new Klant((long)dataReader["CUSTOMER_ID"], (string)dataReader["NAME"], (string)dataReader["ADDRESS"]), (DateTime)dataReader["TIME"]);
                    dataReader.Close();
                    foreach (KeyValuePair<Product, int> p in FindProducten(id, connection)) bestelling.VoegProductToe(p.Key, p.Value);
                    return bestelling;
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

        private bool HeeftBestelling(long id, SqlConnection sqlConnection = null)
        {
            SqlConnection connection;
            if (sqlConnection is null)
                connection = GetConnection();
            else connection = sqlConnection;
            string query = "SELECT count(*) FROM [Bestellingssysteem].[dbo].[ORDER] o JOIN [Bestellingssysteem].[dbo].[CUSTOMER] c ON (o.CUSTOMER_ID = c.CUSTOMER_ID) WHERE ORDER_ID=@id";

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

        public void Verwijder(Bestelling anItem)
        {
            SqlConnection connection = GetConnection();
            string query = "DELETE FROM [Bestellingssysteem].[dbo].[ORDER] WHERE ORDER_ID=@id";
            string queryS = "DELETE FROM [Bestellingssysteem].[dbo].[ORDER_PRODUCT] WHERE ORDER_ID=@id";

            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    command.Parameters.Add(new SqlParameter("@id", SqlDbType.BigInt));
                    command.CommandText = query;
                    command.Parameters["@id"].Value = anItem.BestellingId;
                    command.ExecuteNonQuery();
                    command.CommandText = queryS;
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

        public void VoegToe(Bestelling anItem)
        {
            SqlConnection connection = GetConnection();
            string queryS = "INSERT INTO [Bestellingssysteem].[dbo].[ORDER](TIME,CUSTOMER_ID,PAID,PRICE) output INSERTED.ORDER_ID VALUES(@tijd,@klantId,@betaald,@prijs)";
            string queryU = "UPDATE [Bestellingssysteem].[dbo].[ORDER] SET TIME = @tijd, CUSTOMER_ID = @klantId, PAID = @betaald, PRICE = @prijs WHERE ORDER_ID = @id";

            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {

                    command.Parameters.Add(new SqlParameter("@tijd", SqlDbType.DateTime));
                    command.Parameters.Add(new SqlParameter("@klantId", SqlDbType.BigInt));
                    command.Parameters.Add(new SqlParameter("@betaald", SqlDbType.Int));
                    command.Parameters.Add(new SqlParameter("@prijs", SqlDbType.Decimal));

                    command.Parameters["@tijd"].Value = anItem.Tijdstip;
                    command.Parameters["@klantId"].Value = anItem.Klant.KlantId;
                    command.Parameters["@betaald"].Value = anItem.PrijsBetaald;
                    command.Parameters["@prijs"].Value = (decimal)anItem.Kostprijs();
                    long orderID;
                    if (HeeftBestelling(anItem.BestellingId, connection) == false)
                    {
                        command.CommandText = queryS;
                        orderID = (long)command.ExecuteScalar();
                        if (anItem.GeefProducten().Count > 0)
                            VoegProductenToe(anItem.GeefProducten(), orderID, connection);
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@id", SqlDbType.BigInt));
                        command.Parameters["@id"].Value = anItem.BestellingId;
                        command.CommandText = queryU;
                        command.ExecuteNonQuery();
                        if (anItem.GeefProducten().Count > 0)
                            VoegProductenToe(anItem.GeefProducten(), anItem.BestellingId, connection);
                    }

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
        private void VoegProductenToe(IEnumerable<KeyValuePair<Product, int>> lijst, long orderID, SqlConnection sqlConnection = null)
        {
            SqlConnection connection;
            if (sqlConnection is null)
                connection = GetConnection();
            else connection = sqlConnection;
            string query = "INSERT INTO [Bestellingssysteem].[dbo].[ORDER_PRODUCT](ORDER_ID, PRODUCT_ID, AMOUNT) VALUES(@oID,@pID,@amount)";
            string queryU = "UPDATE [Bestellingssysteem].[dbo].[ORDER_PRODUCT] SET ORDER_ID = @oID, PRODUCT_ID = @pID, AMOUNT = @amount WHERE ORDER_ID = @oID AND PRODUCT_ID = @pID";

            using (SqlCommand command = connection.CreateCommand())
            {
                if (connection.State != ConnectionState.Open) connection.Open();
                try
                {
                    command.Parameters.Add(new SqlParameter("@oID", SqlDbType.BigInt));
                    command.Parameters.Add(new SqlParameter("@pID", SqlDbType.BigInt));
                    command.Parameters.Add(new SqlParameter("@amount", SqlDbType.Int));

                    Dictionary<Product, int> checkL = FindProducten(orderID, connection);
                    foreach (KeyValuePair<Product, int> p in lijst)
                    {
                        if (checkL.ContainsKey(p.Key))
                            command.CommandText = queryU;
                        else command.CommandText = query;
                        command.Parameters["@oID"].Value = orderID;
                        command.Parameters["@pID"].Value = p.Key.ProductId;
                        command.Parameters["@amount"].Value = p.Value;
                        command.ExecuteNonQuery();
                    }
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
        }

        private Dictionary<Product, int> FindProducten(long id, SqlConnection sqlConnection = null)
        {
            Dictionary<Product, int> producten = new Dictionary<Product, int>();
            SqlConnection connection;
            if (sqlConnection is null)
                connection = GetConnection();
            else connection = sqlConnection;
            string query = "SELECT * FROM [Bestellingssysteem].[dbo].[ORDER_PRODUCT] op JOIN [Bestellingssysteem].[dbo].[PRODUCT] p ON (op.PRODUCT_ID = p.PRODUCT_ID) WHERE op.ORDER_ID=@oID";

            using (SqlCommand command = connection.CreateCommand())
            {
                if (connection.State != ConnectionState.Open) connection.Open();
                try
                {
                    command.Parameters.Add(new SqlParameter("@oID", SqlDbType.BigInt));
                    command.CommandText = query;
                    command.Parameters["@oID"].Value = id;
                    IDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        producten.Add(new Product((long)dataReader["PRODUCT_ID"], (string)dataReader["NAME"], Convert.ToDouble(dataReader["PRICE"])), (int)dataReader["AMOUNT"]);
                    }
                    dataReader.Close();

                    return producten;
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
            return null;
        }
    }
}
