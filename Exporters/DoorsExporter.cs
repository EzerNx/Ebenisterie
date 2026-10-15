using Ebenisterie.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ebenisterie.Exporters
{   internal class DoorsExporter
    {

        private readonly SqlServerService _sql;

        public DoorsExporter(SqlServerService sql)
        {
            _sql = sql;
        }

        public DataTable GetData()
        {
            string query = @"
                SELECT
                  [Door ID]
      ,[Cabinet ID]
      ,[Door Num]
      ,[Door Name]
      ,[Type]
      ,[Hinge]
      ,[Width]
      ,[Width String]
      ,[Height]
      ,[Height String]
      ,[Finger Pull ID]
      ,[Finger Pull]
      ,[Edge Profile ID]
      ,[Edge Profile]
      ,[Raised Panel Detail ID]
      ,[Raised Panel Detail]
      ,[Route Pattern ID]
      ,[Route Pattern]
      ,[Material Schedule ID]
      ,[Material Schedule]
      ,[Image]
      ,[32mm Reference]
      ,[Hinge Center Lines]
      ,[Mounting Holes]
      ,[Labor]
      ,[Cost]
      ,[DrawerBox ID]
      ,[Pulls]
      ,[Hinges]
      ,[Class]
      ,[Notes]
      ,[HGrain]
      ,[EndPanel]
      ,[DoorCat]
      ,[TrackingNo]
      ,[Label]
      ,[HingeMatID]
  FROM [Doors]";

            return _sql.ExecuteQuery(query);
        }
    }
}
