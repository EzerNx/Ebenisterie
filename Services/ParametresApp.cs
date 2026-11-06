using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;

namespace Ebenisterie.Services
{
    /// <summary>
    /// Paramètres propres à chaque PC.
    /// Chaque poste a son propre SQL Server local : le nom du serveur est
    /// détecté automatiquement, puis enregistré dans
    /// %LocalAppData%\CollaboxImport\parametres.json
    /// </summary>
    public static class ParametresApp
    {
        public const string NomApplication = "Collabox Import";

        /// <summary>Base du projet, restaurée depuis le .bak.</summary>
        public const string BaseProjet = "Report";

        private static readonly string DossierParametres = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CollaboxImport");

        private static string FichierParametres =>
            Path.Combine(DossierParametres, "parametres.json");

        /// <summary>Serveur enregistré sur ce PC, ou null.</summary>
        public static string? LireServeurEnregistre()
        {
            try
            {
                if (!File.Exists(FichierParametres))
                    return null;

                using var doc = JsonDocument.Parse(File.ReadAllText(FichierParametres));

                return doc.RootElement.TryGetProperty("Serveur", out JsonElement valeur)
                    ? valeur.GetString()
                    : null;
            }
            catch
            {
                return null; // fichier abîmé : on refera la détection
            }
        }

        public static void EnregistrerServeur(string serveur)
        {
            Directory.CreateDirectory(DossierParametres);

            string json = JsonSerializer.Serialize(
                new { Serveur = serveur },
                new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(FichierParametres, json);
        }

        /// <summary>
        /// Serveur indiqué dans appsettings.json (valeur par défaut livrée avec l'application).
        /// </summary>
        public static string? LireServeurParDefaut()
        {
            try
            {
                string fichier = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (!File.Exists(fichier))
                    return null;

                using var doc = JsonDocument.Parse(File.ReadAllText(fichier));

                string? chaine = doc.RootElement
                    .GetProperty("ConnectionStrings")
                    .GetProperty("SqlServer")
                    .GetString();

                return string.IsNullOrWhiteSpace(chaine)
                    ? null
                    : new SqlConnectionStringBuilder(chaine).DataSource;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Instances SQL Server installées sur ce PC, lues dans le registre Windows.
        /// MSSQLSERVER (instance par défaut) donne "localhost",
        /// les autres donnent "localhost\NOM" (ex. : localhost\SQLEXPRESS).
        /// </summary>
        public static List<string> InstancesLocales()
        {
            var liste = new List<string>();

            foreach (RegistryView vue in new[] { RegistryView.Registry64, RegistryView.Registry32 })
            {
                try
                {
                    using RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, vue);
                    using RegistryKey? cle = hklm.OpenSubKey(
                        @"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL");

                    if (cle == null)
                        continue;

                    foreach (string nom in cle.GetValueNames())
                    {
                        string serveur = nom.Equals("MSSQLSERVER", StringComparison.OrdinalIgnoreCase)
                            ? "localhost"
                            : @"localhost\" + nom;

                        if (!liste.Contains(serveur, StringComparer.OrdinalIgnoreCase))
                            liste.Add(serveur);
                    }
                }
                catch
                {
                    // registre inaccessible : on continue avec les autres sources
                }
            }

            return liste;
        }

        /// <summary>Chaîne de connexion Windows (compte de l'utilisateur) vers une base.</summary>
        public static string ChaineConnexion(string serveur, string nomBase = BaseProjet)
        {
            return new SqlConnectionStringBuilder
            {
                DataSource = serveur,
                InitialCatalog = nomBase,
                IntegratedSecurity = true,
                TrustServerCertificate = true
            }.ConnectionString;
        }

        /// <summary>Vérifie qu'on peut se connecter au serveur (via la base master).</summary>
        public static bool TesterServeur(string serveur, out string message)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(ChaineConnexion(serveur, "master"))
                {
                    ConnectTimeout = 5
                };

                using var connexion = new SqlConnection(builder.ConnectionString);
                connexion.Open();

                message = "Connexion réussie à " + serveur + ".";
                return true;
            }
            catch (Exception ex)
            {
                message = "Échec sur " + serveur + " : " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Cherche un serveur SQL qui répond, dans cet ordre :
        /// 1) celui enregistré sur ce PC, 2) celui de appsettings.json,
        /// 3) les instances installées sur ce PC.
        /// Les messages d'échec sont ajoutés à <paramref name="messages"/>.
        /// </summary>
        public static string? TrouverServeur(List<string> messages)
        {
            var candidats = new List<string>();

            void Ajouter(string? serveur)
            {
                if (!string.IsNullOrWhiteSpace(serveur)
                    && !candidats.Contains(serveur, StringComparer.OrdinalIgnoreCase))
                {
                    candidats.Add(serveur);
                }
            }

            Ajouter(LireServeurEnregistre());
            Ajouter(LireServeurParDefaut());
            foreach (string serveur in InstancesLocales())
                Ajouter(serveur);

            foreach (string serveur in candidats)
            {
                if (TesterServeur(serveur, out string message))
                    return serveur;

                messages.Add(message);
            }

            return null;
        }
    }
}
