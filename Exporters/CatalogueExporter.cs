using Ebenisterie.Services;
using System.Data;

namespace Ebenisterie.Exporters
{
    /// <summary>
    /// Export du catalogue Cabinet Vision (base CVData) : les 6 tables utilisées
    /// par le Sommaire des Matériaux. Le catalogue est le même pour tous les projets.
    /// </summary>
    internal class CatalogueExporter
    {
        private readonly SqlServerService _cvdata;

        public CatalogueExporter(SqlServerService cvdata)
        {
            _cvdata = cvdata;
        }

        /// <summary>
        /// Liste des feuilles Excel : nom de la feuille + requête SQL sur CVData.
        /// </summary>
        public List<(string Feuille, Func<DataTable> GetData)> GetExports()
        {
            return new List<(string, Func<DataTable>)>
            {
                // Nom du matériau, type, unité, perte par défaut
                ("Material", () => _cvdata.ExecuteQuery(@"
                    SELECT [ID], [Name], [MaterialTypeID], [UnitOfIssueID], [Waste], [Deleted]
                    FROM [dbo].[Material]
                    ORDER BY [ID]")),

                // Nom du type de matériau
                ("refMaterialType", () => _cvdata.ExecuteQuery(@"
                    SELECT [ID], [Name]
                    FROM [dbo].[refMaterialType]
                    ORDER BY [ID]")),

                // Unité : Sheet, Per FT, Each, Pair...
                ("refUnitOfIssue", () => _cvdata.ExecuteQuery(@"
                    SELECT [ID], [Name]
                    FROM [dbo].[refUnitOfIssue]
                    ORDER BY [ID]")),

                // Catégories (dossiers du menu des matériaux)
                ("MaterialMenuTree", () => _cvdata.ExecuteQuery(@"
                    SELECT [ID], [ParentID], [Name]
                    FROM [dbo].[MaterialMenuTree]
                    ORDER BY [ID]")),

                // Lien matériau -> catégorie
                ("MaterialMenuTreeItem", () => _cvdata.ExecuteQuery(@"
                    SELECT [ID], [MenuTreeID], [MaterialID]
                    FROM [dbo].[MaterialMenuTreeItem]
                    ORDER BY [ID]")),

                // Taille des feuilles (en mm)
                ("MaterialExtraSizeInfo", () => _cvdata.ExecuteQuery(@"
                    SELECT [ID], [MaterialID], [Width], [Length], [Thickness]
                    FROM [dbo].[MaterialExtraSizeInfo]
                    ORDER BY [ID]")),
            };
        }
    }
}
