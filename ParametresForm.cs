using Ebenisterie.Services;

namespace Ebenisterie
{
    /// <summary>
    /// Fenêtre pour choisir le serveur SQL de ce PC.
    /// Construite en code (pas de fichier Designer).
    /// </summary>
    public class ParametresForm : Form
    {
        private readonly ComboBox cboServeur = new ComboBox();
        private readonly Label lblResultat = new Label();

        /// <summary>Serveur choisi par l'utilisateur.</summary>
        public string Serveur => cboServeur.Text.Trim();

        public ParametresForm(string serveurActuel)
        {
            Text = "Paramètres - " + ParametresApp.NomApplication;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(420, 175);

            var lblServeur = new Label
            {
                Text = "Serveur SQL de ce PC :",
                Location = new Point(15, 15),
                AutoSize = true
            };

            cboServeur.Location = new Point(15, 38);
            cboServeur.Size = new Size(390, 23);
            cboServeur.DropDownStyle = ComboBoxStyle.DropDown; // liste + saisie libre
            foreach (string serveur in ParametresApp.InstancesLocales())
                cboServeur.Items.Add(serveur);
            cboServeur.Text = serveurActuel;

            var lblAide = new Label
            {
                Text = @"Ex. : localhost\SQLEXPRESS, ou localhost pour l'instance par défaut",
                Location = new Point(15, 66),
                AutoSize = true,
                ForeColor = SystemColors.GrayText
            };

            lblResultat.Location = new Point(15, 90);
            lblResultat.Size = new Size(390, 34);

            var btnTester = new Button
            {
                Text = "Tester",
                Location = new Point(15, 135),
                Size = new Size(90, 28)
            };

            var btnEnregistrer = new Button
            {
                Text = "Enregistrer",
                Location = new Point(220, 135),
                Size = new Size(90, 28)
            };

            var btnAnnuler = new Button
            {
                Text = "Annuler",
                Location = new Point(315, 135),
                Size = new Size(90, 28),
                DialogResult = DialogResult.Cancel
            };

            btnTester.Click += async (_, _) => await TesterAsync();

            // On n'enregistre que si la connexion fonctionne
            btnEnregistrer.Click += async (_, _) =>
            {
                if (await TesterAsync())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            };

            AcceptButton = btnEnregistrer;
            CancelButton = btnAnnuler;

            Controls.AddRange(new Control[]
            {
                lblServeur, cboServeur, lblAide, lblResultat,
                btnTester, btnEnregistrer, btnAnnuler
            });
        }

        private async Task<bool> TesterAsync()
        {
            string serveur = Serveur;

            if (serveur.Length == 0)
            {
                lblResultat.ForeColor = Color.Red;
                lblResultat.Text = "Indique un serveur.";
                return false;
            }

            lblResultat.ForeColor = SystemColors.ControlText;
            lblResultat.Text = "Test en cours...";
            UseWaitCursor = true;

            var (ok, message) = await Task.Run(() =>
            {
                bool resultat = ParametresApp.TesterServeur(serveur, out string msg);
                return (resultat, msg);
            });

            UseWaitCursor = false;
            lblResultat.ForeColor = ok ? Color.Green : Color.Red;
            lblResultat.Text = message;

            return ok;
        }
    }
}
