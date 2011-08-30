namespace TSDServer
{
    partial class TtnForm
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
            this.txtLabel = new System.Windows.Forms.Label();
            this.dateLabel = new System.Windows.Forms.Label();
            this.docLabel = new System.Windows.Forms.Label();
            this.bkLabel = new System.Windows.Forms.Label();
            this.errLabel = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.PaleGreen;
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.txtLabel);
            this.panel1.Controls.Add(this.dateLabel);
            this.panel1.Controls.Add(this.docLabel);
            this.panel1.Controls.Add(this.bkLabel);
            this.panel1.Controls.Add(this.errLabel);
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(235, 256);
            // 
            // txtLabel
            // 
            this.txtLabel.BackColor = System.Drawing.Color.PaleGreen;
            this.txtLabel.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.txtLabel.ForeColor = System.Drawing.Color.Red;
            this.txtLabel.Location = new System.Drawing.Point(3, 151);
            this.txtLabel.Name = "txtLabel";
            this.txtLabel.Size = new System.Drawing.Size(229, 20);
            this.txtLabel.Text = "ТТН ПРИНЯТЬ";
            this.txtLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.txtLabel.Visible = false;
            // 
            // dateLabel
            // 
            this.dateLabel.BackColor = System.Drawing.Color.PaleGreen;
            this.dateLabel.ForeColor = System.Drawing.Color.Black;
            this.dateLabel.Location = new System.Drawing.Point(3, 131);
            this.dateLabel.Name = "dateLabel";
            this.dateLabel.Size = new System.Drawing.Size(229, 20);
            this.dateLabel.Text = "Дата:";
            this.dateLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.dateLabel.Visible = false;
            // 
            // docLabel
            // 
            this.docLabel.BackColor = System.Drawing.Color.PaleGreen;
            this.docLabel.ForeColor = System.Drawing.Color.Black;
            this.docLabel.Location = new System.Drawing.Point(3, 111);
            this.docLabel.Name = "docLabel";
            this.docLabel.Size = new System.Drawing.Size(229, 20);
            this.docLabel.Text = "ТТН №";
            this.docLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.docLabel.Visible = false;
            // 
            // bkLabel
            // 
            this.bkLabel.BackColor = System.Drawing.Color.PaleGreen;
            this.bkLabel.ForeColor = System.Drawing.Color.Black;
            this.bkLabel.Location = new System.Drawing.Point(3, 91);
            this.bkLabel.Name = "bkLabel";
            this.bkLabel.Size = new System.Drawing.Size(229, 20);
            this.bkLabel.Text = "ШК:";
            this.bkLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.bkLabel.Visible = false;
            // 
            // errLabel
            // 
            this.errLabel.BackColor = System.Drawing.Color.Red;
            this.errLabel.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.errLabel.ForeColor = System.Drawing.Color.Black;
            this.errLabel.Location = new System.Drawing.Point(3, 71);
            this.errLabel.Name = "errLabel";
            this.errLabel.Size = new System.Drawing.Size(229, 20);
            this.errLabel.Text = "ШК **** чужой ТТН!";
            this.errLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.errLabel.Visible = false;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(3, 36);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(232, 23);
            this.textBox1.TabIndex = 1;
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(0, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(238, 20);
            this.label1.Text = "Отсканируейте ШК ТТН";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.PaleGreen;
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 259);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(240, 36);
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Gold;
            this.label3.Location = new System.Drawing.Point(145, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 33);
            this.label3.Text = "F4 - Показать результат";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Red;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 33);
            this.label2.Text = "F1 - подсчет коробов";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(0, 236);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(229, 20);
            this.label4.Text = "Fn-Clr - выход";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TtnForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(240, 295);
            this.ControlBox = false;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "TtnForm";
            this.Text = "Прием машин";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.InventarForm_Load);
            this.Closed += new System.EventHandler(this.InventarForm_Closed);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label errLabel;
        private System.Windows.Forms.Label docLabel;
        private System.Windows.Forms.Label bkLabel;
        private System.Windows.Forms.Label txtLabel;
        private System.Windows.Forms.Label dateLabel;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
    }
}