using Ebenisterie.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ebenisterie.Exporters
{
    internal class DrawerExporter
    {
        private readonly SqlServerService _sql;

        public DrawerExporter(SqlServerService sql)
        {
            _sql = sql;
        }

        public DataTable GetData()
        {
            string query = @"
                SELECT
                   [Drawer ID]
      ,[Cabinet ID]
      ,[Drawer Num]
      ,[Drawer Name]
      ,[Type]
      ,[Width]
      ,[Width String]
      ,[Height]
      ,[Height String]
      ,[Depth]
      ,[Depth String]
      ,[Image]
      ,[32mm Reference]
      ,[Guide Location]
      ,[Case Hole]
      ,[Bottom Lip]
      ,[Labor]
      ,[Cost]
      ,[MaterialScheduleID]
      ,[MaterialSchedule]
      ,[GuideMaterialID]
      ,[GuideMaterial]
  FROM [Drawers]";

            return _sql.ExecuteQuery(query);
        }
    }
}
