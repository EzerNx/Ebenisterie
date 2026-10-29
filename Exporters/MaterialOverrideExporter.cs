using Ebenisterie.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ebenisterie.Exporters
{
    internal class MaterialOverrideExporter
    {
        private readonly SqlServerService _sql;

        public MaterialOverrideExporter(SqlServerService sql)
        {
            _sql = sql;
        }

        public DataTable GetData()
        {
            string query = @"
                SELECT
                    [MatID],
                    [Waste]
                FROM [MaterialOverride]";

            return _sql.ExecuteQuery(query);
        }
    }
}
