using Ebenisterie.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ebenisterie.Exporters
{
    internal class RolloutExporter
    {
        private readonly SqlServerService _sql;

        public RolloutExporter(SqlServerService sql)
        {
            _sql = sql;
        }

        public DataTable GetData()
        {
            string query = @"
                     SELECT
                           [Rollout ID]
                          ,[Cabinet ID]
                          ,[Rollout Num]
                          ,[Rollout Name]
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
                          ,[Labor]
                          ,[Cost]
                          ,[MaterialScheduleID]
                          ,[MaterialSchedule]
                          ,[GuideMaterialID]
                          ,[GuideMaterial]
                    FROM [Rollouts]";

            return _sql.ExecuteQuery(query);
        }
    }
}
