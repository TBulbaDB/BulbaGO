namespace BulbaGO.UI
{
    partial class BotControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.BotSplit = new System.Windows.Forms.SplitContainer();
            this.BotName = new System.Windows.Forms.Label();
            this.Start = new System.Windows.Forms.Button();
            this.BotLogRich = new System.Windows.Forms.RichTextBox();
            this.Stop = new System.Windows.Forms.Button();
            this.BotProcessTitle = new System.Windows.Forms.Label();
            this.ConsoleHolder = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.BotSplit)).BeginInit();
            this.BotSplit.Panel1.SuspendLayout();
            this.BotSplit.Panel2.SuspendLayout();
            this.BotSplit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BotSplit
            // 
            this.BotSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BotSplit.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.BotSplit.Location = new System.Drawing.Point(0, 0);
            this.BotSplit.Name = "BotSplit";
            this.BotSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // BotSplit.Panel1
            // 
            this.BotSplit.Panel1.Controls.Add(this.ConsoleHolder);
            this.BotSplit.Panel1.Controls.Add(this.BotProcessTitle);
            this.BotSplit.Panel1.Controls.Add(this.Stop);
            this.BotSplit.Panel1.Controls.Add(this.Start);
            this.BotSplit.Panel1.Controls.Add(this.BotName);
            // 
            // BotSplit.Panel2
            // 
            this.BotSplit.Panel2.Controls.Add(this.BotLogRich);
            this.BotSplit.Size = new System.Drawing.Size(1336, 1146);
            this.BotSplit.SplitterDistance = 128;
            this.BotSplit.TabIndex = 0;
            // 
            // BotName
            // 
            this.BotName.AutoSize = true;
            this.BotName.Location = new System.Drawing.Point(21, 21);
            this.BotName.Name = "BotName";
            this.BotName.Size = new System.Drawing.Size(100, 25);
            this.BotName.TabIndex = 0;
            this.BotName.Text = "BotName";
            // 
            // Start
            // 
            this.Start.Location = new System.Drawing.Point(165, 21);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(117, 49);
            this.Start.TabIndex = 1;
            this.Start.Text = "Start";
            this.Start.UseVisualStyleBackColor = true;
            this.Start.Click += new System.EventHandler(this.Start_Click);
            // 
            // BotLogRich
            // 
            this.BotLogRich.BackColor = System.Drawing.Color.Black;
            this.BotLogRich.DetectUrls = false;
            this.BotLogRich.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BotLogRich.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BotLogRich.Location = new System.Drawing.Point(0, 0);
            this.BotLogRich.Name = "BotLogRich";
            this.BotLogRich.ReadOnly = true;
            this.BotLogRich.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.BotLogRich.Size = new System.Drawing.Size(1336, 1014);
            this.BotLogRich.TabIndex = 1;
            this.BotLogRich.Text = "";
            // 
            // Stop
            // 
            this.Stop.Location = new System.Drawing.Point(312, 21);
            this.Stop.Name = "Stop";
            this.Stop.Size = new System.Drawing.Size(117, 49);
            this.Stop.TabIndex = 2;
            this.Stop.Text = "Stop";
            this.Stop.UseVisualStyleBackColor = true;
            this.Stop.Click += new System.EventHandler(this.Stop_Click);
            // 
            // BotProcessTitle
            // 
            this.BotProcessTitle.AutoSize = true;
            this.BotProcessTitle.Location = new System.Drawing.Point(26, 82);
            this.BotProcessTitle.Name = "BotProcessTitle";
            this.BotProcessTitle.Size = new System.Drawing.Size(0, 25);
            this.BotProcessTitle.TabIndex = 3;
            // 
            // ConsoleHolder
            // 
            this.ConsoleHolder.Location = new System.Drawing.Point(1243, 3);
            this.ConsoleHolder.Name = "ConsoleHolder";
            this.ConsoleHolder.Size = new System.Drawing.Size(90, 38);
            this.ConsoleHolder.TabIndex = 4;
            // 
            // BotControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BotSplit);
            this.Name = "BotControl";
            this.Size = new System.Drawing.Size(1336, 1146);
            this.Load += new System.EventHandler(this.BotControl_Load);
            this.BotSplit.Panel1.ResumeLayout(false);
            this.BotSplit.Panel1.PerformLayout();
            this.BotSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.BotSplit)).EndInit();
            this.BotSplit.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer BotSplit;
        private System.Windows.Forms.Label BotName;
        private System.Windows.Forms.Button Start;
        private System.Windows.Forms.RichTextBox BotLogRich;
        private System.Windows.Forms.Button Stop;
        private System.Windows.Forms.Label BotProcessTitle;
        private System.Windows.Forms.Panel ConsoleHolder;
    }
}
