using OfficeOpenXml;
using System.Data;


namespace Ebenisterie.Helpers
{
    public static class ExcelWriter
    {

        public static void WriteTable(
            ExcelWorksheet worksheet,
            DataTable table)
        {

            // Entêtes
            for (int col = 0; col < table.Columns.Count; col++)
            {
                worksheet.Cells[1, col + 1]
                    .Value = table.Columns[col].ColumnName;
            }



            // Données

            for (int row = 0; row < table.Rows.Count; row++)
            {

                for (int col = 0; col < table.Columns.Count; col++)
                {

                    worksheet.Cells[row + 2, col + 1]
                        .Value = table.Rows[row][col];

                }

            }


            worksheet.Cells.AutoFitColumns();

        }
    }
}