using System.Text;
using Microsoft.Data.SqlClient;

namespace Ebenisterie.Services
{
    /// <summary>
    /// Supprime la base cible si elle existe, puis la restaure à partir d'un fichier .bak.
    /// Le nom de la base est celui de la chaîne de connexion (Database=report).
    /// </summary>
    public class DatabaseRestorer
    {
        private readonly string _masterConnectionString;

        public string DatabaseName { get; }

        public DatabaseRestorer(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);

            DatabaseName = string.IsNullOrWhiteSpace(builder.InitialCatalog)
                ? "Report"
                : builder.InitialCatalog;

            // On ne peut pas supprimer/restaurer une base en étant connecté dessus
            builder.InitialCatalog = "master";
            _masterConnectionString = builder.ConnectionString;
        }

        public async Task RestoreAsync(string bakPath, IProgress<string>? progress = null)
        {
            if (string.IsNullOrWhiteSpace(bakPath))
                throw new ArgumentException("Chemin du fichier .bak manquant.", nameof(bakPath));

            await using (var conn = new SqlConnection(_masterConnectionString))
            {
                await conn.OpenAsync();

                // 1) Lire le contenu du .bak AVANT de supprimer quoi que ce soit :
                //    si le fichier est illisible, l'ancienne base reste intacte.
                progress?.Report("Lecture du fichier de sauvegarde...");
                var files = await GetBackupFilesAsync(conn, bakPath);

                // 2) Supprimer la base si elle existe
                progress?.Report($"Vérification de la base {DatabaseName}...");
                if (await DatabaseExistsAsync(conn))
                {
                    progress?.Report($"Suppression de la base {DatabaseName}...");
                    await DropDatabaseAsync(conn);
                }

                // 3) Dossiers par défaut du serveur pour les fichiers .mdf / .ldf
                string dataPath = await GetServerPropertyAsync(conn, "InstanceDefaultDataPath")
                    ?? throw new InvalidOperationException("Dossier de données SQL Server introuvable.");
                string logPath = await GetServerPropertyAsync(conn, "InstanceDefaultLogPath") ?? dataPath;

                // 4) Restaurer
                progress?.Report($"Restauration de {DatabaseName} (peut prendre quelques minutes)...");
                string sql = BuildRestoreSql(bakPath, files, dataPath, logPath);

                await using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 0 };
                await cmd.ExecuteNonQueryAsync();
            }

            // Les connexions du pool vers l'ancienne base ne sont plus valides
            SqlConnection.ClearAllPools();
            progress?.Report($"Base {DatabaseName} restaurée.");
        }

        private async Task<bool> DatabaseExistsAsync(SqlConnection conn)
        {
            await using var cmd = new SqlCommand("SELECT DB_ID(@name)", conn);
            cmd.Parameters.AddWithValue("@name", DatabaseName);
            object? result = await cmd.ExecuteScalarAsync();
            return result != null && result != DBNull.Value;
        }

        private async Task DropDatabaseAsync(SqlConnection conn)
        {
            string db = QuoteName(DatabaseName);

            // SINGLE_USER + ROLLBACK IMMEDIATE ferme les connexions ouvertes sur la base
            string sql = $@"
ALTER DATABASE {db} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE {db};";

            await using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 0 };
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task<List<(string LogicalName, string Type)>> GetBackupFilesAsync(
            SqlConnection conn, string bakPath)
        {
            var files = new List<(string, string)>();

            await using (var cmd = new SqlCommand("RESTORE FILELISTONLY FROM DISK = @path", conn))
            {
                cmd.Parameters.AddWithValue("@path", bakPath);
                cmd.CommandTimeout = 0;

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    files.Add((reader["LogicalName"].ToString()!, reader["Type"].ToString()!));
                }
            }

            if (files.Count == 0)
                throw new InvalidOperationException("Aucun fichier trouvé dans la sauvegarde.");

            return files;
        }

        private static async Task<string?> GetServerPropertyAsync(SqlConnection conn, string property)
        {
            await using var cmd = new SqlCommand(
                $"SELECT CAST(SERVERPROPERTY('{property}') AS nvarchar(4000))", conn);
            object? result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? null : result.ToString();
        }

        private string BuildRestoreSql(string bakPath, List<(string LogicalName, string Type)> files,
                                       string dataPath, string logPath)
        {
            var sb = new StringBuilder();
            sb.Append($"RESTORE DATABASE {QuoteName(DatabaseName)} FROM DISK = {QuoteString(bakPath)} WITH ");

            int dataIndex = 0, logIndex = 0;
            foreach (var f in files)
            {
                string target;
                if (f.Type == "L")
                {
                    target = Path.Combine(logPath,
                        logIndex == 0 ? $"{DatabaseName}_log.ldf" : $"{DatabaseName}_log{logIndex}.ldf");
                    logIndex++;
                }
                else // "D" = données
                {
                    target = Path.Combine(dataPath,
                        dataIndex == 0 ? $"{DatabaseName}.mdf" : $"{DatabaseName}_{dataIndex}.ndf");
                    dataIndex++;
                }

                sb.Append($"MOVE {QuoteString(f.LogicalName)} TO {QuoteString(target)}, ");
            }

            sb.Append("REPLACE, RECOVERY;");
            return sb.ToString();
        }

        private static string QuoteName(string name) => "[" + name.Replace("]", "]]") + "]";
        private static string QuoteString(string s) => "N'" + s.Replace("'", "''") + "'";
    }
}
