using Ebenisterie.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ebenisterie.Exporters
{
    public class ProjetExporter
    {
        private readonly SqlServerService _sql;

        public ProjetExporter(SqlServerService sql)
        {
            _sql = sql;
        }

        public DataTable GetData()
        {
            string query = @"
                SELECT
                    ID,
                    [Job Name] AS NomProjet,
                    [Job Number] AS NumeroProjet,
                    Comment
                FROM [Job Info]";

            return _sql.ExecuteQuery(query);
        }
    }
}
