using Ebenisterie.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ebenisterie.Exporters
{
    internal class ContactExporter
    {
        private readonly SqlServerService _sql;

        public ContactExporter(SqlServerService sql)
        {
            _sql = sql;
        }

        public DataTable GetData()
        {
            string query = @"
                SELECT
                   [ShipToName] as nom
                  ,[ShipToAddress] as adresse
                  ,[ShipToCity] as ville
                  ,[ShipToState] as province
                  ,[ShipToZip] as codePostal
                  ,[ShipToContact] 
                  ,[ShipToEmail] as courriel
                  ,[ShipToFax] as fax
                  ,[ShipToMobile] as cellulaire
                  ,[ShipToPhone] as telephone
                  ,[ShipToComments] as commentaire
                FROM [Job Info]";

            return _sql.ExecuteQuery(query);
        }
    }
}
