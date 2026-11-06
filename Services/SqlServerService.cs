using Microsoft.Data.SqlClient;
using System.Data;

namespace Ebenisterie.Services
{
    public class SqlServerService
    {
        private readonly string _connectionString;

        public string ConnectionString => _connectionString;

        /// <summary>Nom du serveur (ex. : localhost\SQLEXPRESS).</summary>
        public string Serveur => new SqlConnectionStringBuilder(_connectionString).DataSource;

        // Le serveur est choisi par ParametresApp (propre à chaque PC),
        // la chaîne de connexion est donc toujours passée au constructeur.


        /// <summary>
        /// Service connecté à une chaîne de connexion précise (ex. : la base CVData).
        /// </summary>
        public SqlServerService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Retourne un service sur le même serveur, mais pointant vers une autre base.
        /// Ex. : _sql.PourBase("CVData")
        /// </summary>
        public SqlServerService PourBase(string nomBase)
        {
            var builder = new SqlConnectionStringBuilder(_connectionString)
            {
                InitialCatalog = nomBase
            };

            return new SqlServerService(builder.ConnectionString);
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
                // On teste le serveur via "master" : la base Report peut ne pas
                // encore exister (nouvelle instance) avant la première restauration.
                var builder = new SqlConnectionStringBuilder(_connectionString)
                {
                    InitialCatalog = "master"
                };

                using SqlConnection connection =
                    new SqlConnection(builder.ConnectionString);


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