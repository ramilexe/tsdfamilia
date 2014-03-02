namespace TSDServer
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
                BTPrintClass.PrintClass.Disconnect();
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.productsDataSet1 = new TSDServer.ProductsDataSet();
            this.productsDataSet2 = new TSDServer.ProductsDataSet();
            this.button0 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.productsDataSet1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.productsDataSet2)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Plum;
            this.button1.Dock = System.Windows.Forms.DockStyle.Top;
            this.button1.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Regular);
            this.button1.Location = new System.Drawing.Point(0, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(238, 40);
            this.button1.TabIndex = 0;
            this.button1.Text = "1 - Контроль цен";
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.PaleGreen;
            this.button2.Dock = System.Windows.Forms.DockStyle.Top;
            this.button2.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Regular);
            this.button2.Location = new System.Drawing.Point(0, 40);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(238, 40);
            this.button2.TabIndex = 1;
            this.button2.Text = "2 - Инвентаризация";
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.PaleGreen;
            this.button3.Dock = System.Windows.Forms.DockStyle.Top;
            this.button3.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Regular);
            this.button3.Location = new System.Drawing.Point(0, 80);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(238, 40);
            this.button3.TabIndex = 2;
            this.button3.Text = "3 - Прием короба внутрит.";
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.PaleGreen;
            this.button4.Dock = System.Windows.Forms.DockStyle.Top;
            this.button4.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Regular);
            this.button4.Location = new System.Drawing.Point(0, 120);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(238, 40);
            this.button4.TabIndex = 3;
            this.button4.Text = "4 - Прием машины";
            // 
            // button5
            // 
            this.button5.BackColor = System.Drawing.Color.PaleGreen;
            this.button5.Dock = System.Windows.Forms.DockStyle.Top;
            this.button5.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Regular);
            this.button5.Location = new System.Drawing.Point(0, 160);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(238, 46);
            this.button5.TabIndex = 4;
            this.button5.Text = "5 - Настройки";
            // 
            // button6
            // 
            this.button6.BackColor = System.Drawing.Color.PaleGreen;
            this.button6.Dock = System.Windows.Forms.DockStyle.Top;
            this.button6.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Regular);
            this.button6.Location = new System.Drawing.Point(0, 206);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(238, 40);
            this.button6.TabIndex = 5;
            this.button6.Text = "6  - Накладные";
            // 
            // productsDataSet1
            // 
            this.productsDataSet1.DataSetName = "ProductsDataSet";
            this.productsDataSet1.Prefix = "";
            this.productsDataSet1.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // productsDataSet2
            // 
            this.productsDataSet2.DataSetName = "ProductsDataSet";
            this.productsDataSet2.Prefix = "";
            this.productsDataSet2.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // button0
            // 
            this.button0.BackColor = System.Drawing.Color.PaleGreen;
            this.button0.Dock = System.Windows.Forms.DockStyle.Top;
            this.button0.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Regular);
            this.button0.Location = new System.Drawing.Point(0, 246);
            this.button0.Name = "button0";
            this.button0.Size = new System.Drawing.Size(238, 40);
            this.button0.TabIndex = 6;
            this.button0.Text = "0  - Выход";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(238, 295);
            this.Controls.Add(this.button0);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "MainForm";
            this.Text = "Терминал сбора данных";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Closed += new System.EventHandler(this.Form1_Closed);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form1_KeyPress);
            ((System.ComponentModel.ISupportInitialize)(this.productsDataSet1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.productsDataSet2)).EndInit();
            this.ResumeLayout(false);

        }

        

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private ProductsDataSet productsDataSet1;
        private ProductsDataSet productsDataSet2;
        private System.Windows.Forms.Button button0;

    }
}

