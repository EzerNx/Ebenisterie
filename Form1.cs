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

        // Remplacé quand on trouve ou change le serveur SQL de ce PC
        private SqlServerService _sql;

        public Form1()
        {
            InitializeComponent();

            Text = ParametresApp.NomApplication;
            try
            {
                // Reprend le logo intégré au .exe
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
                // icône par défaut si l'extraction échoue
            }

            // Valeur provisoire, confirmée au démarrage par InitialiserConnexionAsync
            string serveur = ParametresApp.LireServeurEnregistre()
                ?? ParametresApp.LireServeurParDefaut()
                ?? "localhost";
            _sql = new SqlServerService(ParametresApp.ChaineConnexion(serveur));

            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            Shown += async (_, _) => await InitialiserConnexionAsync();
        }

        /// <summary>
        /// Au démarrage : trouve le serveur SQL de ce PC (enregistré, appsettings.json,
        /// ou instance installée) sans bloquer la fenêtre.
        /// </summary>
        private async Task InitialiserConnexionAsync()
        {
            ActiverBoutons(false);
            lblConnexion.ForeColor = SystemColors.ControlText;
            lblConnexion.Text = "Recherche du serveur SQL...";

            var messages = new List<string>();
            string? serveur = await Task.Run(() => ParametresApp.TrouverServeur(messages));

            foreach (string message in messages)
                Log(message);

            if (serveur != null)
            {
                ParametresApp.EnregistrerServeur(serveur); // démarrage rapide la prochaine fois
                UtiliserServeur(serveur);
            }
            else
            {
                lblConnexion.ForeColor = Color.Red;
                lblConnexion.Text = "Aucun serveur SQL trouvé";
                Log("Aucun serveur SQL trouvé sur ce PC. Clique sur « Paramètres » pour l'indiquer.");
            }

            ActiverBoutons(true);
        }

        private void UtiliserServeur(string serveur)
        {
            _sql = new SqlServerService(ParametresApp.ChaineConnexion(serveur));
            TesterConnexion();
        }

        private void ActiverBoutons(bool actif)
        {
            btnExporter.Enabled = actif;
            btnExporterCatalogue.Enabled = actif;
            btnParametres.Enabled = actif;
        }

        private void btnParametres_Click(object sender, EventArgs e)
        {
            using var form = new ParametresForm(_sql.Serveur);

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                ParametresApp.EnregistrerServeur(form.Serveur);
                UtiliserServeur(form.Serveur);
            }
        }

        /// <summary>
        /// Retourne le chemin complet du fichier d'export :
        /// C:\Users\[utilisateur]\Downloads\importDataColl\import_AAAA-MM-JJ.xlsx
        /// Le dossier est créé automatiquement s'il n'existe pas.
        /// </summary>
        private static string GetCheminFichierExport(string prefixe = "import")
        {
            string telechargements = GetDossierTelechargements();

            string dossier = Path.Combine(telechargements, NomDossierExport);

            Directory.CreateDirectory(dossier); // ne fait rien si le dossier existe déjà

            string nomFichier = prefixe + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".xlsx";

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
                lblConnexion.Text = "Connecté à " + _sql.Serveur;
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
                ("CxMateriel",   () => new CxMaterielExporter(_sql).GetData()),
                ("Rollout",   () => new RolloutExporter(_sql).GetData()),
                ("Produit", () => new ProduitExporter(_sql).GetData()),
                ("Drawer",  () => new DrawerExporter(_sql).GetData()),
                ("Doors",   () => new DoorsExporter(_sql).GetData()),
                ("Part",    () => new PartsExporter(_sql).GetData()),
                ("MaterialOverride",    () => new MaterialOverrideExporter(_sql).GetData()),
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

            ActiverBoutons(false);
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
                string fichier = await ExporterVersExcelAsync(exports, "import");

                MessageBox.Show("Fichier créé :\n" + fichier);
            }
            catch (Exception ex)
            {
                Log("ERREUR : " + ex.Message);
                MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ActiverBoutons(true);
            }
        }

        /// <summary>
        /// Export du catalogue Cabinet Vision (base CVData) dans un fichier Excel séparé :
        /// Downloads\importDataCollabox\catalogue_AAAA-MM-JJ.xlsx
        /// Aucune restauration : on lit CVData telle qu'elle est sur le serveur.
        /// </summary>
        private async void btnExporterCatalogue_Click(object sender, EventArgs e)
        {
            ActiverBoutons(false);
            progressBar1.Value = 0;

            try
            {
                // Même serveur que la base Report, mais base CVData
                SqlServerService cvdata = _sql.PourBase("CVData");

                var exports = new CatalogueExporter(cvdata).GetExports();

                string fichier = await ExporterVersExcelAsync(exports, "catalogue");

                MessageBox.Show("Fichier créé :\n" + fichier);
            }
            catch (Exception ex)
            {
                Log("ERREUR : " + ex.Message);
                MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ActiverBoutons(true);
            }
        }

        /// <summary>
        /// Crée un fichier Excel avec une feuille par export et retourne son chemin.
        /// Utilisé par l'export principal et par l'export du catalogue.
        /// </summary>
        private async Task<string> ExporterVersExcelAsync(
            List<(string Feuille, Func<DataTable> GetData)> exports,
            string prefixeFichier)
        {
            string fichier = GetCheminFichierExport(prefixeFichier);

            // Si un export a déjà été fait aujourd'hui, on le remplace
            // (lève une erreur claire si le fichier est ouvert dans Excel)
            if (File.Exists(fichier))
            {
                File.Delete(fichier);
                Log("Ancien fichier du jour remplacé.");
            }

            Log("Début export vers : " + fichier);

            // Nouveau classeur à chaque export : évite l'erreur
            // "feuille déjà existante" si on clique deux fois
            var excel = new ExcelService();

            try
            {
                for (int i = 0; i < exports.Count; i++)
                {
                    var (feuille, getData) = exports[i];

                    Log("Export " + feuille + "...");

                    // La requête SQL tourne en arrière-plan : la fenêtre ne gèle pas
                    DataTable data = await Task.Run(getData);

                    ExcelWriter.WriteTable(excel.CreateWorksheet(feuille), data);

                    progressBar1.Value = (i + 1) * 100 / exports.Count;
                }

                // Une seule sauvegarde, à la fin
                excel.Save(fichier);
            }
            finally
            {
                excel.Close();
            }

            Log("Export terminé.");
            return fichier;
        }
    }
}