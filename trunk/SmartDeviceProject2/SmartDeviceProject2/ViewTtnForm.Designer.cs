namespace TSDServer
{
    partial class ViewTtnForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.lblTotalBox = new System.Windows.Forms.Label();
            this.lblAcceptedBox = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.treeView1);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(232, 215);
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(232, 215);
            this.treeView1.TabIndex = 0;
            this.treeView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeView1_KeyDown);
            // 
            // lblTotalBox
            // 
            this.lblTotalBox.Location = new System.Drawing.Point(3, 221);
            this.lblTotalBox.Name = "lblTotalBox";
            this.lblTotalBox.Size = new System.Drawing.Size(100, 20);
            this.lblTotalBox.Text = "Всего коробов:";
            // 
            // lblAcceptedBox
            // 
            this.lblAcceptedBox.Location = new System.Drawing.Point(3, 235);
            this.lblAcceptedBox.Name = "lblAcceptedBox";
            this.lblAcceptedBox.Size = new System.Drawing.Size(121, 20);
            this.lblAcceptedBox.Text = "Принято коробов:";
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Yellow;
            this.label1.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular);
            this.label1.Location = new System.Drawing.Point(2, 255);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(233, 14);
            this.label1.Text = "Fn-CLR или желтая кнопка выход";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ViewTtnForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(238, 270);
            this.ControlBox = false;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblAcceptedBox);
            this.Controls.Add(this.lblTotalBox);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.Name = "ViewTtnForm";
            this.Text = "Просмотр машины (ТТН)";
            this.Load += new System.EventHandler(this.ViewTtnForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ViewTtnForm_KeyDown);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Label lblTotalBox;
        private System.Windows.Forms.Label lblAcceptedBox;
        private System.Windows.Forms.Label label1;
    }
}