using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Ebenisterie.Services
{
    public class SqlServerService
    {
        private readonly string _connectionString;

        public SqlServerService()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            _connectionString =
                configuration.GetConnectionString("SqlServer")
                ?? throw new Exception("Connection SQL Server introuvable.");
        }


        /*public SqlServerService()
        {
            _connectionString =
                @"Server=.;
                  Database=report;
                  Trusted_Connection=True;
                  TrustServerCertificate=True;";
        }*/


        public bool TesterConnexion(out string message)
        {
            try
            {
                using SqlConnection connection =
                    new SqlConnection(_connectionString);


                connection.Open();


                message = "Connexion SQL Server réussie.";

                return true;
            }
            catch (Exception ex)
            {
                message =
                    "Erreur SQL Server : " + ex.Message;

                return false;
            }
        }


        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
        public DataTable ExecuteQuery(string sql)
        {
            DataTable table = new DataTable();


            using (SqlConnection connection = GetConnection())
            {

                connection.Open();


                SqlDataAdapter adapter =
                    new SqlDataAdapter(sql, connection);


                adapter.Fill(table);

            }


            return table;
        }
    }
}