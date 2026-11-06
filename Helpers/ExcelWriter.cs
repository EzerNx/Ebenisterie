using ClosedXML.Excel;
using System.Data;

namespace Ebenisterie.Helpers
{
    public static class ExcelWriter
    {
        // Limite d'Excel pour le texte d'une cellule
        private const int LongueurMaxTexte = 32767;

        public static void WriteTable(IXLWorksheet worksheet, DataTable table)
        {
            // Entêtes
            for (int col = 0; col < table.Columns.Count; col++)
            {
                worksheet.Cell(1, col + 1).Value = table.Columns[col].ColumnName;
            }

            // Données
            for (int row = 0; row < table.Rows.Count; row++)
            {
                for (int col = 0; col < table.Columns.Count; col++)
                {
                    worksheet.Cell(row + 2, col + 1).Value = Convertir(table.Rows[row][col]);
                }
            }

            worksheet.Columns().AdjustToContents();
        }

        /// <summary>
        /// Convertit une valeur SQL en valeur de cellule Excel :
        /// nombres en nombres, dates en dates, NULL en cellule vide, le reste en texte.
        /// </summary>
        private static XLCellValue Convertir(object? valeur)
        {
            switch (valeur)
            {
                case null:
                case DBNull:
                    return Blank.Value;

                case string texte:
                    return texte.Length > LongueurMaxTexte
                        ? texte.Substring(0, LongueurMaxTexte)
                        : texte;

                case bool booleen:
                    return booleen;

                case DateTime date:
                    return date;

                case DateTimeOffset dateOffset:
                    return dateOffset.DateTime;

                case TimeSpan duree:
                    return duree;

                case byte or sbyte or short or ushort or int or uint
                  or long or ulong or float or double or decimal:
                    return Convert.ToDouble(valeur);

                default:
                    string autre = valeur.ToString() ?? string.Empty;
                    return autre.Length > LongueurMaxTexte
                        ? autre.Substring(0, LongueurMaxTexte)
                        : autre;
            }
        }
    }
}
