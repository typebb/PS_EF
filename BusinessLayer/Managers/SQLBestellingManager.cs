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
            string query = "SELECT * FROM [Bestellingssysteem].[dbo].[ORDER]";
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                connection.Open();
                try
                {
                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        Bestelling bestelling = new Bestelling((long)dataReader["ORDER_ID"], (DateTime)dataReader["TIME"]);
                        
                        besL.Add(bestelling);
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
            return besL.AsReadOnly();
        }

        public IReadOnlyList<Bestelling> HaalOp(Func<Bestelling, bool> predicate)
        {
            SqlConnection connection = GetConnection();
            List<Bestelling> besL = new List<Bestelling>();
            string query = "SELECT * FROM [Bestellingssysteem].[dbo].[ORDER]";
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                connection.Open();
                try
                {
                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        Bestelling bestelling = new Bestelling((long)dataReader["ORDER_ID"], (DateTime)dataReader["TIME"]);
                        
                        besL.Add(bestelling);
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
            return besL.Where<Bestelling>(predicate).ToList();
        }

        public Bestelling HaalOp(long id)
        {
            SqlConnection connection = GetConnection();
            string query = "SELECT * FROM [Bestellingssysteem].[dbo].[ORDER] WHERE ORDER_ID=@id";
            using (SqlCommand command = connection.CreateCommand())
            {
                command.Parameters.Add(new SqlParameter("@id", SqlDbType.BigInt));
                command.CommandText = query;
                command.Parameters["@id"].Value = id;
                connection.Open();
                try
                {
                    IDataReader dataReader = command.ExecuteReader();
                    dataReader.Read();
                    Bestelling bestelling = new Bestelling((long)dataReader["ORDER_ID"], (DateTime)dataReader["TIME"]);
                    
                    dataReader.Close();
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

        public void Verwijder(Bestelling anItem)
        {
            SqlConnection connection = GetConnection();
            string query = "DELETE FROM [Bestellingssysteem].[dbo].[ORDER] WHERE ORDER_ID=@id";

            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    command.Parameters.Add(new SqlParameter("@id", SqlDbType.BigInt));
                    command.CommandText = query;
                    command.Parameters["@id"].Value = anItem.BestellingId;
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
            string query = "SELECT * FROM [Bestellingssysteem].[dbo].[CUSTOMER] WHERE NAME=@naam AND ADDRESS=@adres";
            string queryS = "INSERT INTO [Bestellingssysteem].[dbo].[ORDER](TIME,CUSTOMER_ID,PAID,PRICE) VALUES(@tijd,@klantId,@betaald,@prijs)";

            using (SqlCommand command = connection.CreateCommand())
            using (SqlCommand command2 = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    if (anItem.Klant != null)

                    command.Parameters.Add(new SqlParameter("@naam", SqlDbType.NVarChar));
                    command.Parameters.Add(new SqlParameter("@adres", SqlDbType.NVarChar));
                    command.CommandText = query;
                    command.Parameters["@naam"].Value = anItem.Klant.Naam;
                    command.Parameters["@adres"].Value = anItem.Klant.Adres;
                    IDataReader dataReader = command.ExecuteReader();
                    dataReader.Read();
                    long kID = (long)dataReader["CUSTOMER_ID"];
                    dataReader.Close();

                    //command2.Parameters.Add(new SqlParameter("@id", SqlDbType.BigInt));
                    command2.Parameters.Add(new SqlParameter("@tijd", SqlDbType.DateTime));
                    command2.Parameters.Add(new SqlParameter("@klantId", SqlDbType.BigInt));
                    command2.Parameters.Add(new SqlParameter("@betaald", SqlDbType.Int));
                    command2.Parameters.Add(new SqlParameter("@prijs", SqlDbType.Decimal));
                    command2.CommandText = queryS;
                    //command2.Parameters["@id"].Value = anItem.BestellingId;
                    command2.Parameters["@tijd"].Value = anItem.Tijdstip;
                    command2.Parameters["@klantId"].Value = kID;
                    command2.Parameters["@betaald"].Value = anItem.PrijsBetaald;
                    command2.Parameters["@prijs"].Value = (decimal)anItem.Kostprijs();
                    command2.ExecuteNonQuery();
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
