using Ebenisterie.Services;

/*namespace Ebenisterie
{
    public partial class Form1 : Form
    {
        private readonly SqlServerService _sql;
        private readonly ExcelService _excel;

        public Form1()
        {
            InitializeComponent();

            _sql = new SqlServerService(
               @"Server=.;Database=report;Trusted_Connection=True;TrustServerCertificate=True");

            _excel = new ExcelService();

            TesterConnexion();
        }

        private void TesterConnexion()
        {
            lblConnexion.Text = "Connecté";
        }

        private void Log(string message)
        {
            txtLog.AppendText(message + Environment.NewLine);
        }

        private void btnParcourir_Click(object sender, EventArgs e)
        {

        }

        private void btnExporter_Click(object sender, EventArgs e)
        {

        }
    }
}*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;               // Nécessaire pour Color.Green / Color.Red
using System.IO;
using System.Runtime.InteropServices;   // Pour SHGetKnownFolderPath
using System.Threading.Tasks;
using System.Windows.Forms;
using Ebenisterie.Exporters;
using Ebenisterie.Helpers;

namespace Ebenisterie
{
    public partial class Form1 : Form
    {
        private const string NomDossierExport = "importDataCollabox";

        private readonly SqlServerService _sql;

        public Form1()
        {
            InitializeComponent();

            _sql = new SqlServerService();

            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            TesterConnexion();
        }

        /// <summary>
        /// Retourne le chemin complet du fichier d'export :
        /// C:\Users\[utilisateur]\Downloads\importDataColl\import_AAAA-MM-JJ.xlsx
        /// Le dossier est créé automatiquement s'il n'existe pas.
        /// </summary>
        private static string GetCheminFichierExport()
        {
            string telechargements = GetDossierTelechargements();

            string dossier = Path.Combine(telechargements, NomDossierExport);

            Directory.CreateDirectory(dossier); // ne fait rien si le dossier existe déjà

            string nomFichier = "import_" + DateTime.Now.ToString("yyyy-MM-dd") + ".xlsx";

            return Path.Combine(dossier, nomFichier);
        }

        // Identifiant officiel Windows du dossier "Téléchargements" (FOLDERID_Downloads)
        private static readonly Guid FolderIdDownloads =
            new Guid("374DE290-123F-4565-9164-39C4925E467B");

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHGetKnownFolderPath(
            [MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
            uint dwFlags,
            IntPtr hToken,
            out string pszPath);

        /// <summary>
        /// Demande à Windows le vrai chemin du dossier Téléchargements,
        /// quelle que soit la langue du PC et même s'il a été déplacé
        /// (autre disque, OneDrive...).
        /// </summary>
        private static string GetDossierTelechargements()
        {
            if (SHGetKnownFolderPath(FolderIdDownloads, 0, IntPtr.Zero, out string chemin) == 0
                && !string.IsNullOrEmpty(chemin))
            {
                return chemin;
            }

            // Solution de secours : emplacement par défaut
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");
        }

        private void TesterConnexion()
        {
            bool resultat = _sql.TesterConnexion(out string message);

            if (resultat)
            {
                lblConnexion.ForeColor = Color.Green;
                lblConnexion.Text = "Connecté à SQL Server";
            }
            else
            {
                lblConnexion.ForeColor = Color.Red;
                lblConnexion.Text = "Erreur connexion SQL Server";
            }

            Log(message);
        }

        private void Log(string texte)
        {
            txtLog.AppendText(
                DateTime.Now.ToString("HH:mm:ss") + "  " + texte + Environment.NewLine);

            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private async void btnExporter_Click(object sender, EventArgs e)
        {
            // --- Liste des exports : nom de feuille + méthode qui récupère les données ---
            var exports = new List<(string Feuille, Func<DataTable> GetData)>
            {
                ("Client",  () => new ClientsExporter(_sql).GetData()),
                ("Projet",  () => new ProjetExporter(_sql).GetData()),
                ("Contact", () => new ContactExporter(_sql).GetData()),
                ("Phase",   () => new PhaseExporter(_sql).GetData()),
                ("Produit", () => new ProduitExporter(_sql).GetData()),
                ("Drawer",  () => new DrawerExporter(_sql).GetData()),
                ("Doors",   () => new DoorsExporter(_sql).GetData()),
                ("Part",    () => new PartsExporter(_sql).GetData()),
            };

            // --- Étape 1 : choisir le fichier .bak à restaurer ---
            string fichierBak;
            using (var dlg = new OpenFileDialog
            {
                Title = "Choisir la sauvegarde de la base Report",
                Filter = "Sauvegarde SQL Server (*.bak)|*.bak|Tous les fichiers (*.*)|*.*"
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                fichierBak = dlg.FileName;
            }

            btnExporter.Enabled = false;
            progressBar1.Value = 0;

            try
            {
                // --- Étape 2 : supprimer la base Report et la restaurer depuis le .bak ---
                var restorer = new DatabaseRestorer(_sql.ConnectionString);
                Log("Restauration depuis : " + fichierBak);

                // Progress<T> renvoie les messages sur le thread de la fenêtre
                await restorer.RestoreAsync(fichierBak, new Progress<string>(Log));

                TesterConnexion();

                // --- Étape 3 : export Excel (fonctionnement existant) ---
                string fichier = GetCheminFichierExport();

                // Si un export a déjà été fait aujourd'hui, on le remplace
                // (lève une erreur claire si le fichier est ouvert dans Excel)
                if (File.Exists(fichier))
                {
                    File.Delete(fichier);
                    Log("Ancien fichier du jour remplacé.");
                }

                Log("Début export vers : " + fichier);

                // Nouveau classeur à chaque export : évite l'erreur
                // "feuille déjà existante" si on clique deux fois sur Exporter
                var excel = new ExcelService();

                for (int i = 0; i < exports.Count; i++)
                {
                    var (feuille, getData) = exports[i];

                    Log("Export " + feuille + "...");

                    // La requête SQL tourne en arrière-plan : la fenêtre ne gèle plus
                    DataTable data = await Task.Run(getData);

                    ExcelWriter.WriteTable(excel.CreateWorksheet(feuille), data);

                    progressBar1.Value = (i + 1) * 100 / exports.Count;
                }

                // Une seule sauvegarde, à la fin
                excel.Save(fichier);

                Log("Export terminé.");
                MessageBox.Show("Fichier créé :\n" + fichier);
            }
            catch (Exception ex)
            {
                Log("ERREUR : " + ex.Message);
                MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnExporter.Enabled = true;
            }
        }
    }
}