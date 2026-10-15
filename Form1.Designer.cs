namespace Ebenisterie
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

       

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblConnexion = new Label();
            btnExporter = new Button();
            txtLog = new RichTextBox();
            progressBar1 = new ProgressBar();
            lblTitre = new Label();
            SuspendLayout();
            // 
            // lblConnexion
            // 
            lblConnexion.AutoSize = true;
            lblConnexion.Location = new Point(25, 53);
            lblConnexion.Name = "lblConnexion";
            lblConnexion.Size = new Size(75, 15);
            lblConnexion.TabIndex = 0;
            lblConnexion.Text = "Verification...";
            // 
            // btnExporter
            // 
            btnExporter.Location = new Point(40, 352);
            btnExporter.Name = "btnExporter";
            btnExporter.Size = new Size(294, 48);
            btnExporter.TabIndex = 2;
            btnExporter.Text = "Exporter";
            btnExporter.UseVisualStyleBackColor = true;
            btnExporter.Click += btnExporter_Click;
            // 
            // txtLog
            // 
            txtLog.Location = new Point(25, 71);
            txtLog.Name = "txtLog";
            txtLog.Size = new Size(320, 221);
            txtLog.TabIndex = 4;
            txtLog.Text = "";
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(25, 298);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(320, 23);
            progressBar1.TabIndex = 5;
            // 
            // lblTitre
            // 
            lblTitre.AutoSize = true;
            lblTitre.Location = new Point(106, 24);
            lblTitre.Name = "lblTitre";
            lblTitre.Size = new Size(152, 15);
            lblTitre.TabIndex = 0;
            lblTitre.Text = "Export SQL Server vers Excel";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(372, 450);
            Controls.Add(progressBar1);
            Controls.Add(txtLog);
            Controls.Add(btnExporter);
            Controls.Add(lblTitre);
            Controls.Add(lblConnexion);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }


        #endregion

        private Label lblConnexion;
        private Button btnExporter;
        private RichTextBox txtLog;
        private ProgressBar progressBar1;
        private FolderBrowserDialog folderBrowserDialog1;
        private Label lblTitre;
        private Label lblDossier;
        private Label label3;
        private TextBox txtNomFichier;
    }
}
