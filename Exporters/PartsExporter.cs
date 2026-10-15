using Ebenisterie.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ebenisterie.Exporters
{
    internal class PartsExporter
    {
        private readonly SqlServerService _sql;

        public PartsExporter(SqlServerService sql)
        {
            _sql = sql;
        }

        public DataTable GetData()
        {
            string query = @"
                SELECT
[Cabinet ID]
      ,[Part ID]
      ,[Width]
      ,[Width String]
      ,[Length]
      ,[Length String]
      ,[Material ID]
      ,[MatSchID]
      ,[Door ID]
      ,[Drawer ID]
      ,[RollOut ID]
      ,[Top ID]
      ,[Operation]
      ,[Shaped]
      ,[Image]
      ,[LamEdgeBand]
      ,[NumDado]
      ,[NumDrill]
      ,[NumRoute]
      ,[LaborSetup]
      ,[LaborEach]
      ,[DXFFile]
      ,[SheetNum]
      ,[EdgeBand]
      ,[CutOrder]
      ,[BarCode]
      ,[Description]
      ,[NCID]
      ,[FaceFinishID]
      ,[BackFinishID]
      ,[EdgeFinishID]
      ,[Banding]
      ,[BandingFlags]
      ,[IntBandMaterial]
      ,[IntBandColor]
      ,[ExtBandMaterial]
      ,[ExtBandColor]
      ,[DoorBandMaterial]
      ,[DoorBandColor]
      ,[TextureFace]
      ,[TextureBack]
      ,[TextureEdge]
      ,[BuyOut]
      ,[Area]
      ,[Parameters]
      ,[Cost]
      ,[ArcCount]
      ,[PartType]
      ,[ID]
  FROM [Parts] order by [Part ID]
";

            return _sql.ExecuteQuery(query);
        }
    }
}
