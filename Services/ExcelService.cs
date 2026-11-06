using ClosedXML.Excel;

namespace Ebenisterie.Services
{
    /// <summary>
    /// Classeur Excel en mémoire (ClosedXML, licence MIT : gratuit en entreprise).
    /// </summary>
    public class ExcelService
    {
        private readonly XLWorkbook _workbook;

        public ExcelService()
        {
            _workbook = new XLWorkbook();
        }

        public IXLWorksheet CreateWorksheet(string name)
        {
            // Si la feuille existe déjà, on la remplace
            if (_workbook.Worksheets.Contains(name))
            {
                _workbook.Worksheets.Delete(name);
            }

            return _workbook.Worksheets.Add(name);
        }

        public void Save(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            _workbook.SaveAs(path);
        }

        public void Close()
        {
            _workbook.Dispose();
        }
    }
}
