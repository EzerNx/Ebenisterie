using Ebenisterie.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ebenisterie.Exporters
{
    internal class CxMaterielExporter
    {
        private readonly SqlServerService _sql;

        public CxMaterielExporter(SqlServerService sql)
        {
            _sql = sql;
        }

        public DataTable GetData()
        {
            string query = @"
                     SELECT
                           [ID]
                           ,[MaterialTypeID]
                     FROM [report].[dbo].[CxMaterial]";

            return _sql.ExecuteQuery(query);
        }
    }
}
