using Ebenisterie.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ebenisterie.Exporters
{
    internal class ClientsExporter
    {
        private readonly SqlServerService _sql;

        public ClientsExporter(SqlServerService sql)
        {
            _sql = sql;
        }

        public DataTable GetData()
        {
            string query = @"
                SELECT
                      [Customer Name] as Nom
                      ,[Customer Address] as adresse
                      ,[Customer City] as ville
                      ,[Customer State] as province
                      ,[Customer Zip] as codePostal
                      ,[Customer Contact] as no_contact
                      ,[Customer Email] as courriel
                      ,[Customer Fax] as fax
                      ,[Customer Mobile] as cellulaire
                      ,[Customer Phone] as telephone
                      ,[Customer Comments]
                FROM [Job Info]";

            return _sql.ExecuteQuery(query);
        }
    }
}
