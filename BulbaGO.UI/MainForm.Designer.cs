namespace BulbaGO.UI
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TopSplit = new System.Windows.Forms.SplitContainer();
            this.BottomSplit = new System.Windows.Forms.SplitContainer();
            this.BotsList = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.TopSplit)).BeginInit();
            this.TopSplit.Panel2.SuspendLayout();
            this.TopSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BottomSplit)).BeginInit();
            this.BottomSplit.Panel1.SuspendLayout();
            this.BottomSplit.SuspendLayout();
            this.SuspendLayout();
            // 
            // TopSplit
            // 
            this.TopSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TopSplit.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.TopSplit.IsSplitterFixed = true;
            this.TopSplit.Location = new System.Drawing.Point(0, 0);
            this.TopSplit.Name = "TopSplit";
            this.TopSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // TopSplit.Panel1
            // 
            this.TopSplit.Panel1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            // 
            // TopSplit.Panel2
            // 
            this.TopSplit.Panel2.Controls.Add(this.BottomSplit);
            this.TopSplit.Size = new System.Drawing.Size(1395, 867);
            this.TopSplit.SplitterDistance = 100;
            this.TopSplit.SplitterWidth = 1;
            this.TopSplit.TabIndex = 0;
            // 
            // BottomSplit
            // 
            this.BottomSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BottomSplit.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.BottomSplit.IsSplitterFixed = true;
            this.BottomSplit.Location = new System.Drawing.Point(0, 0);
            this.BottomSplit.Name = "BottomSplit";
            // 
            // BottomSplit.Panel1
            // 
            this.BottomSplit.Panel1.Controls.Add(this.BotsList);
            this.BottomSplit.Size = new System.Drawing.Size(1395, 766);
            this.BottomSplit.SplitterDistance = 450;
            this.BottomSplit.SplitterWidth = 1;
            this.BottomSplit.TabIndex = 0;
            // 
            // BotsList
            // 
            this.BotsList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BotsList.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BotsList.FormattingEnabled = true;
            this.BotsList.ItemHeight = 42;
            this.BotsList.Location = new System.Drawing.Point(0, 0);
            this.BotsList.Name = "BotsList";
            this.BotsList.Size = new System.Drawing.Size(450, 766);
            this.BotsList.TabIndex = 0;
            this.BotsList.SelectedIndexChanged += new System.EventHandler(this.BotsList_SelectedIndexChanged);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(1395, 867);
            this.Controls.Add(this.TopSplit);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BulbaGO - Multibot Manager for PokemonGo";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.TopSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.TopSplit)).EndInit();
            this.TopSplit.ResumeLayout(false);
            this.BottomSplit.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.BottomSplit)).EndInit();
            this.BottomSplit.ResumeLayout(false);
            this.ResumeLayout(false);

        }


        #endregion

        private System.Windows.Forms.SplitContainer TopSplit;
        private System.Windows.Forms.SplitContainer BottomSplit;
        private System.Windows.Forms.ListBox BotsList;
    }
}

