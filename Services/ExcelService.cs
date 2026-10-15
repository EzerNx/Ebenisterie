using OfficeOpenXml;
using System.IO;


namespace Ebenisterie.Services
{
    public class ExcelService
    {
        private ExcelPackage _package;


        public ExcelService()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Ebenisterie");

            _package = new ExcelPackage();
        }


        public ExcelWorksheet CreateWorksheet(string name)
        {
            // Vérifie si la feuille existe déjà
            var existing =
                _package.Workbook.Worksheets[name];


            if (existing != null)
            {
                _package.Workbook.Worksheets.Delete(name);
            }


            return _package.Workbook.Worksheets.Add(name);
        }



        public void Save(string path)
        {
            FileInfo file = new FileInfo(path);


            if (file.Exists)
            {
                file.Delete();
            }


            _package.SaveAs(file);
        }


        public void Close()
        {
            _package.Dispose();
        }
    }
}