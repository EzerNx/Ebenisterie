using Ebenisterie.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ebenisterie.Exporters
{
    internal class ProduitExporter
    {

        private readonly SqlServerService _sql;

        public ProduitExporter(SqlServerService sql)
        {
            _sql = sql;
        }

        public DataTable GetData()
        {
            string query = @"
                SELECT
[Room ID]
      ,[Wall ID]
      ,[Cabinet ID]
      ,[Width]
      ,[Width String]
      ,[Height]
      ,[Height String]
      ,[Depth]
      ,[Depth String]
      ,[Cabinet Name] as nom
      ,[Image]
      ,[Cabinet Type]
      ,[Cabinet Style]
      ,[Left Scribe]
      ,[Left Scribe String]
      ,[Right Scribe]
      ,[Right Scribe String]
      ,[Left End]
      ,[Right End]
      ,[Back]
      ,[Toe Height]
      ,[Toe Height String]
      ,[Toe Recess]
      ,[Toe Recess String]
      ,[Soffit Height]
      ,[Soffit Height String]
      ,[Elevation]
      ,[Elevation String]
      ,[Finished Area]
      ,[Assembly Labor]
      ,[Additional Labor]
      ,[Cabinet Face]
      ,[Retail]
      ,[Stock]
      ,[Openings]
      ,[CustomPrice]
      ,[Quantity]
      ,[Comment]
      ,[Hinge]
      ,[Description]
      ,[Assembly]
      ,[Catalog]
      ,[CvcAsmID]
      ,[LeftEndType]
      ,[RightEndType]
  FROM [Cabinets]
";

            return _sql.ExecuteQuery(query);
        }
    }
}
