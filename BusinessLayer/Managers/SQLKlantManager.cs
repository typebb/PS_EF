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
    public class SQLKlantManager : IManager<Klant>
    {
        private string connectionString;
        public SQLKlantManager(string connectionS)
        {
            this.connectionString = connectionS;
        }
        SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
        public IReadOnlyList<Klant> HaalOp()
        {
            SqlConnection connection = GetConnection();
            List<Klant> klantL = new List<Klant>();
            string query = "SELECT * FROM [Bestellingssysteem].[dbo].[CUSTOMER]";
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                connection.Open();
                try
                {
                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        Klant klant = new Klant((long)dataReader["CUSTOMER_ID"], (string)dataReader["NAME"], (string)dataReader["ADDRESS"]);
                        klantL.Add(klant);
                    }
                    dataReader.Close();
                    /*
                    for (int i = 0; i < klantL.Count; i++)
                        foreach (Bestelling b in FindBestellingen(klantL[i], connection))
                            if (!klantL[i].HeeftBestelling(b)) klantL[i].VoegToeBestelling(b);
                    */
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
            return klantL.AsReadOnly();
        }

        public IReadOnlyList<Klant> HaalOp(Func<Klant, bool> predicate)
        {
            SqlConnection connection = GetConnection();
            List<Klant> klantL = new List<Klant>();
            string query = "SELECT * FROM [Bestellingssysteem].[dbo].[CUSTOMER]";
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                connection.Open();
                try
                {
                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        Klant klant = new Klant((long)dataReader["CUSTOMER_ID"], (string)dataReader["NAME"], (string)dataReader["ADDRESS"]);
                        klantL.Add(klant);
                    }
                    dataReader.Close();
                    /*
                    for (int i = 0; i < klantL.Count; i++)
                        foreach (Bestelling b in FindBestellingen(klantL[i], connection))
                            if (!klantL[i].HeeftBestelling(b)) klantL[i].VoegToeBestelling(b);
                    */
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
            return klantL.Where<Klant>(predicate).ToList();
        }

        public Klant HaalOp(long id)
        {
            SqlConnection connection = GetConnection();
            string query = "SELECT * FROM [Bestellingssysteem].[dbo].[CUSTOMER] WHERE CUSTOMER_ID=@id";
            using (SqlCommand command = connection.CreateCommand())
            {
                command.Parameters.Add(new SqlParameter("@id", SqlDbType.BigInt));
                command.CommandText = query;
                command.Parameters["@id"].Value = id;
                connection.Open();
                try
                {
                    if (HeeftKlant(id, connection) == false)
                        return null;
                    IDataReader dataReader = command.ExecuteReader();
                    dataReader.Read();
                    Klant klant = new Klant((long)dataReader["CUSTOMER_ID"], (string)dataReader["NAME"], (string)dataReader["ADDRESS"]);
                    dataReader.Close();
                    foreach (Bestelling b in FindBestellingen(klant, connection))
                        if (!klant.HeeftBestelling(b)) klant.VoegToeBestelling(b);
                    return klant;
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
        private bool HeeftKlant(long id, SqlConnection sqlConnection = null)
        {
            SqlConnection connection;
            if (sqlConnection is null)
                connection = GetConnection();
            else connection = sqlConnection;
            string query = "SELECT count(*) FROM [Bestellingssysteem].[dbo].[CUSTOMER] WHERE CUSTOMER_ID=@id";

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

        public void Verwijder(Klant anItem)
        {
            SqlConnection connection = GetConnection();
            string query = "DELETE FROM [Bestellingssysteem].[dbo].[CUSTOMER] WHERE CUSTOMER_ID=@id";

            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    command.Parameters.Add(new SqlParameter("@id", SqlDbType.BigInt));
                    command.CommandText = query;
                    command.Parameters["@id"].Value = anItem.KlantId;
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

        public void VoegToe(Klant anItem)
        {
            SqlConnection connection = GetConnection();
            string query = "INSERT INTO [Bestellingssysteem].[dbo].[CUSTOMER](NAME,ADDRESS) VALUES(@naam,@adres)";
            string queryU = "UPDATE [Bestellingssysteem].[dbo].[CUSTOMER] SET NAME = @naam, ADDRESS = @adres WHERE CUSTOMER_ID = @id";

            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    command.Parameters.Add(new SqlParameter("@naam", SqlDbType.NVarChar));
                    command.Parameters.Add(new SqlParameter("@adres", SqlDbType.NVarChar));

                    command.Parameters["@naam"].Value = anItem.Naam;
                    command.Parameters["@adres"].Value = anItem.Adres;
                    if (HeeftKlant(anItem.KlantId, connection) == false)
                    {
                        command.CommandText = query;
                        command.ExecuteNonQuery();
                        if (anItem.GetBestellingen().Count > 0)
                            VoegBestellingenToe(anItem.GetBestellingen(), anItem, connection);
                    }
                    else
                    {
                        command.CommandText = queryU;
                        command.Parameters.Add(new SqlParameter("@id", SqlDbType.BigInt));
                        command.Parameters["@id"].Value = anItem.KlantId;
                        command.ExecuteNonQuery();
                        if (anItem.GetBestellingen().Count > 0)
                            VoegBestellingenToe(anItem.GetBestellingen(), anItem, connection);
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
        
        private void VoegBestellingenToe(IEnumerable<Bestelling> lijst, Klant klant, SqlConnection sqlConnection = null)
        {
            SqlConnection connection;
            if (sqlConnection is null)
                connection = GetConnection();
            else connection = sqlConnection;
            string query = "INSERT INTO [Bestellingssysteem].[dbo].[ORDER](TIME,CUSTOMER_ID,PAID,PRICE) output INSERTED.ORDER_ID VALUES(@tijd,@klantId,@betaald,@prijs)";
            string queryU = "UPDATE [Bestellingssysteem].[dbo].[ORDER] SET TIME = @tijd, CUSTOMER_ID = @klantId, PAID = @betaald, PRICE = @prijs WHERE ORDER_ID = @id";

            using (SqlCommand command = connection.CreateCommand())
            {
                if (connection.State != ConnectionState.Open) connection.Open();
                try
                {
                    command.Parameters.Add(new SqlParameter("@tijd", SqlDbType.DateTime));
                    command.Parameters.Add(new SqlParameter("@klantId", SqlDbType.BigInt));
                    command.Parameters.Add(new SqlParameter("@betaald", SqlDbType.Int));
                    command.Parameters.Add(new SqlParameter("@prijs", SqlDbType.Decimal));
                    List<Bestelling> checkL = FindBestellingen(klant, connection);
                    foreach (Bestelling b in lijst)
                    {
                        command.Parameters["@tijd"].Value = b.Tijdstip;
                        command.Parameters["@klantId"].Value = b.Klant.KlantId;
                        command.Parameters["@betaald"].Value = b.PrijsBetaald;
                        command.Parameters["@prijs"].Value = (decimal)b.Kostprijs();
                        long orderID;
                        if (!checkL.Contains(b))
                        {
                            command.CommandText = query;
                            orderID = (long)command.ExecuteScalar();
                            if (b.GeefProducten().Count > 0)
                                VoegProductenToe(b.GeefProducten(), orderID, connection);
                        }
                        else
                        {
                            command.Parameters.Add(new SqlParameter("@id", SqlDbType.BigInt));
                            command.Parameters["@id"].Value = b.BestellingId;
                            command.CommandText = queryU;
                            command.ExecuteNonQuery();
                            if (b.GeefProducten().Count > 0)
                                VoegProductenToe(b.GeefProducten(), b.BestellingId, connection);
                        }
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
        private List<Bestelling> FindBestellingen(Klant klant, SqlConnection sqlConnection = null)
        {
            List<Bestelling> bestellingen = new List<Bestelling>();
            SqlConnection connection;
            if (sqlConnection is null)
                connection = GetConnection();
            else connection = sqlConnection;
            string query = "SELECT * FROM [Bestellingssysteem].[dbo].[ORDER] WHERE CUSTOMER_ID=@oID";

            using (SqlCommand command = connection.CreateCommand())
            {
                if (connection.State != ConnectionState.Open) connection.Open();
                try
                {
                    command.Parameters.Add(new SqlParameter("@oID", SqlDbType.BigInt));
                    command.CommandText = query;
                    command.Parameters["@oID"].Value = klant.KlantId;
                    IDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        Bestelling bestelling = new Bestelling((long)dataReader["ORDER_ID"], klant, (DateTime)dataReader["TIME"]);
                        if ((int)dataReader["PAID"] > 0) bestelling.ZetBetaald();
                        bestellingen.Add(bestelling);
                    }
                    dataReader.Close();
                    for (int i = 0; i < bestellingen.Count; i++)
                        foreach (KeyValuePair<Product, int> p in FindProducten(bestellingen[i].BestellingId, connection)) bestellingen[i].VoegProductToe(p.Key, p.Value);
                    return bestellingen;
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
    }
}
