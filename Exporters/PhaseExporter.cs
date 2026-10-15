using Ebenisterie.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ebenisterie.Exporters
{
    internal class PhaseExporter
    {
        private readonly SqlServerService _sql;

        public PhaseExporter(SqlServerService sql)
        {
            _sql = sql;
        }

        public DataTable GetData() 
        {
            string query = @"
                              SELECT  [Job ID]
      ,[RoomID]
      ,[RoomNumber]
      ,[RoomName]
      ,[RoomDescription]
      ,[Quantity]
      ,[WallCount]
      ,[ExtAsmFin]
      ,[IntAsmFin]
      ,[TopFin]
            FROM [Rooms]";

            return _sql.ExecuteQuery(query);
        }
    }
}
