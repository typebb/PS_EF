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
                    IDataReader dataReader = command.ExecuteReader();
                    dataReader.Read();
                    Klant klant = new Klant((long)dataReader["CUSTOMER_ID"], (string)dataReader["NAME"], (string)dataReader["ADDRESS"]);
                    dataReader.Close();
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

            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    //command.Parameters.Add(new SqlParameter("@id", SqlDbType.BigInt));
                    command.Parameters.Add(new SqlParameter("@naam", SqlDbType.NVarChar));
                    command.Parameters.Add(new SqlParameter("@adres", SqlDbType.NVarChar));
                    command.CommandText = query;
                    //command.Parameters["@id"].Value = anItem.KlantId;
                    command.Parameters["@naam"].Value = anItem.Naam;
                    command.Parameters["@adres"].Value = anItem.Adres;
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
