namespace Familia.TSDClient
{
    partial class ViewProductForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu1;

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
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.closeFrmBtn = new Bananamama.BackOffice.TSD.Client.ImageButton();
            this.printBtn = new Bananamama.BackOffice.TSD.Client.ImageButton();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.textBox5);
            this.panel1.Controls.Add(this.textBox4);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.textBox3);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.textBox2);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.printBtn);
            this.panel1.Controls.Add(this.closeFrmBtn);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(221, 258);
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 258);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(221, 100);
            // 
            // closeFrmBtn
            // 
            this.closeFrmBtn.BackColor = System.Drawing.Color.PaleGreen;
            this.closeFrmBtn.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.closeFrmBtn.Location = new System.Drawing.Point(0, 217);
            this.closeFrmBtn.Name = "closeFrmBtn";
            this.closeFrmBtn.Size = new System.Drawing.Size(221, 41);
            this.closeFrmBtn.TabIndex = 7;
            this.closeFrmBtn.Text = "Назад";
            this.closeFrmBtn.Click += new System.EventHandler(this.closeFrmBtn_Click);
            this.closeFrmBtn.KeyDown += new System.Windows.Forms.KeyEventHandler(this.closeFrmBtn_KeyDown);
            // 
            // printBtn
            // 
            this.printBtn.BackColor = System.Drawing.Color.PaleGreen;
            this.printBtn.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.printBtn.Location = new System.Drawing.Point(0, 176);
            this.printBtn.Name = "printBtn";
            this.printBtn.Size = new System.Drawing.Size(221, 41);
            this.printBtn.TabIndex = 6;
            this.printBtn.Text = "Печать";
            this.printBtn.KeyDown += new System.Windows.Forms.KeyEventHandler(this.printBtn_KeyDown);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(0, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 20);
            this.label1.Text = "Название:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(63, 7);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(158, 23);
            this.textBox1.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(0, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 20);
            this.label2.Text = "Новая цена";
            // 
            // textBox2
            // 
            this.textBox2.Enabled = false;
            this.textBox2.Location = new System.Drawing.Point(76, 59);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(145, 23);
            this.textBox2.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(0, 85);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 20);
            this.label3.Text = "Старая цена";
            // 
            // textBox3
            // 
            this.textBox3.Enabled = false;
            this.textBox3.Location = new System.Drawing.Point(76, 84);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(145, 23);
            this.textBox3.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(0, 110);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 20);
            this.label4.Text = "Артикул";
            // 
            // textBox4
            // 
            this.textBox4.Enabled = false;
            this.textBox4.Location = new System.Drawing.Point(52, 110);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(169, 23);
            this.textBox4.TabIndex = 5;
            // 
            // textBox5
            // 
            this.textBox5.Enabled = false;
            this.textBox5.Location = new System.Drawing.Point(0, 33);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(221, 23);
            this.textBox5.TabIndex = 2;
            // 
            // ViewProductForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(238, 295);
            this.ControlBox = false;
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Menu = this.mainMenu1;
            this.Name = "ViewProductForm";
            this.Text = "Контроль цен";
            this.Load += new System.EventHandler(this.ViewProductForm_Load);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.ViewProductForm_Closing);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Splitter splitter1;
        private Bananamama.BackOffice.TSD.Client.ImageButton printBtn;
        private Bananamama.BackOffice.TSD.Client.ImageButton closeFrmBtn;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox5;
    }
}