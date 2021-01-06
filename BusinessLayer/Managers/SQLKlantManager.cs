﻿using BusinessLayer.Exceptions;
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
                    for (int i = 0; i < klantL.Count; i++)
                        foreach (Bestelling b in FindBestellingen(klantL[i], connection))
                            if (!klantL[i].HeeftBestelling(b)) klantL[i].VoegToeBestelling(b);
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
                    for (int i = 0; i < klantL.Count; i++)
                        foreach (Bestelling b in FindBestellingen(klantL[i], connection))
                            if (!klantL[i].HeeftBestelling(b)) klantL[i].VoegToeBestelling(b);
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
        public List<Bestelling> FindBestellingen(Klant klant, SqlConnection sqlConnection = null)
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
                        bestellingen.Add(new Bestelling((long)dataReader["ORDER_ID"], klant, (DateTime)dataReader["TIME"]));
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
        public Dictionary<Product, int> FindProducten(long id, SqlConnection sqlConnection = null)
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
