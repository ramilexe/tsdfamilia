namespace TSDServer
{
    partial class Form1
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
                loader.Dispose();
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.importGoodBtn = new System.Windows.Forms.Button();
            this.settingsBtn = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.открытьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.выходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            //this.productsDataSet1 = new TSDServer.ProductsDataSet();
            this.stopGoodBtn = new System.Windows.Forms.Button();
            this.importDocBtn = new System.Windows.Forms.Button();
            this.stopDocsBtn = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.uploadBtn = new System.Windows.Forms.Button();
            this.downloadBtn = new System.Windows.Forms.Button();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.statusStrip1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
           // ((System.ComponentModel.ISupportInitialize)(this.productsDataSet1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // importGoodBtn
            // 
            this.importGoodBtn.Location = new System.Drawing.Point(12, 12);
            this.importGoodBtn.Name = "importGoodBtn";
            this.importGoodBtn.Size = new System.Drawing.Size(174, 23);
            this.importGoodBtn.TabIndex = 0;
            this.importGoodBtn.Text = "Импорт файла товаров";
            this.importGoodBtn.UseVisualStyleBackColor = true;
            this.importGoodBtn.Click += new System.EventHandler(this.button1_Click);
            // 
            // settingsBtn
            // 
            this.settingsBtn.Location = new System.Drawing.Point(12, 128);
            this.settingsBtn.Name = "settingsBtn";
            this.settingsBtn.Size = new System.Drawing.Size(174, 23);
            this.settingsBtn.TabIndex = 2;
            this.settingsBtn.Text = "Настройки";
            this.settingsBtn.UseVisualStyleBackColor = true;
            this.settingsBtn.Click += new System.EventHandler(this.button3_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
            this.statusStrip1.Location = new System.Drawing.Point(0, 371);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(232, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Image = global::TSDServer.Properties.Resources.CriticalError;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(97, 17);
            this.toolStripStatusLabel1.Text = "Не подключен";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(0, 17);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.открытьToolStripMenuItem,
            this.toolStripMenuItem1,
            this.выходToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(132, 54);
            // 
            // открытьToolStripMenuItem
            // 
            this.открытьToolStripMenuItem.Name = "открытьToolStripMenuItem";
            this.открытьToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.открытьToolStripMenuItem.Text = "Открыть";
            this.открытьToolStripMenuItem.Click += new System.EventHandler(this.открытьToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(128, 6);
            // 
            // выходToolStripMenuItem
            // 
            this.выходToolStripMenuItem.Name = "выходToolStripMenuItem";
            this.выходToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.выходToolStripMenuItem.Text = "Выход";
            this.выходToolStripMenuItem.Click += new System.EventHandler(this.выходToolStripMenuItem_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "Сервер ТСД";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(notifyIcon1_MouseDoubleClick);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.richTextBox1.Location = new System.Drawing.Point(0, 180);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(232, 191);
            this.richTextBox1.TabIndex = 4;
            this.richTextBox1.Text = "";
            this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // productsDataSet1
            // 
            //this.productsDataSet1.DataSetName = "ProductsDataSet";
            //this.productsDataSet1.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            //// 
            // stopGoodBtn
            // 
            this.stopGoodBtn.Image = global::TSDServer.Properties.Resources.StopHS;
            this.stopGoodBtn.Location = new System.Drawing.Point(192, 12);
            this.stopGoodBtn.Name = "stopGoodBtn";
            this.stopGoodBtn.Size = new System.Drawing.Size(28, 23);
            this.stopGoodBtn.TabIndex = 5;
            this.stopGoodBtn.UseVisualStyleBackColor = true;
            this.stopGoodBtn.Click += new System.EventHandler(this.button4_Click);
            // 
            // importDocBtn
            // 
            this.importDocBtn.Location = new System.Drawing.Point(12, 41);
            this.importDocBtn.Name = "importDocBtn";
            this.importDocBtn.Size = new System.Drawing.Size(174, 23);
            this.importDocBtn.TabIndex = 6;
            this.importDocBtn.Text = "Импорт файла документов";
            this.importDocBtn.UseVisualStyleBackColor = true;
            this.importDocBtn.Click += new System.EventHandler(this.importDocBtn_Click);
            // 
            // stopDocsBtn
            // 
            this.stopDocsBtn.Image = global::TSDServer.Properties.Resources.StopHS;
            this.stopDocsBtn.Location = new System.Drawing.Point(192, 41);
            this.stopDocsBtn.Name = "stopDocsBtn";
            this.stopDocsBtn.Size = new System.Drawing.Size(28, 23);
            this.stopDocsBtn.TabIndex = 7;
            this.stopDocsBtn.UseVisualStyleBackColor = true;
            this.stopDocsBtn.Click += new System.EventHandler(this.button4_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.importGoodBtn);
            this.panel1.Controls.Add(this.stopGoodBtn);
            this.panel1.Controls.Add(this.importDocBtn);
            this.panel1.Controls.Add(this.stopDocsBtn);
            this.panel1.Controls.Add(this.uploadBtn);
            this.panel1.Controls.Add(this.downloadBtn);
            this.panel1.Controls.Add(this.settingsBtn);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(232, 180);
            this.panel1.TabIndex = 9;
            // 
            // uploadBtn
            // 
            this.uploadBtn.Location = new System.Drawing.Point(12, 70);
            this.uploadBtn.Name = "uploadBtn";
            this.uploadBtn.Size = new System.Drawing.Size(174, 23);
            this.uploadBtn.TabIndex = 1;
            this.uploadBtn.Text = "Выгрузить на терминал";
            this.uploadBtn.UseVisualStyleBackColor = true;
            this.uploadBtn.Click += new System.EventHandler(this.button2_Click);
            // 
            // downloadBtn
            // 
            this.downloadBtn.Location = new System.Drawing.Point(12, 99);
            this.downloadBtn.Name = "downloadBtn";
            this.downloadBtn.Size = new System.Drawing.Size(174, 23);
            this.downloadBtn.TabIndex = 9;
            this.downloadBtn.Text = "Загрузить с терминала";
            this.downloadBtn.UseVisualStyleBackColor = true;
            this.downloadBtn.Click += new System.EventHandler(this.downloadBtn_Click);
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter1.Location = new System.Drawing.Point(0, 170);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(232, 10);
            this.splitter1.TabIndex = 10;
            this.splitter1.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(232, 393);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "Form1";
            this.Text = "Сервер обмена с ТСД";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            //((System.ComponentModel.ISupportInitialize)(this.productsDataSet1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        

        #endregion

        private System.Windows.Forms.Button importGoodBtn;
        private System.Windows.Forms.Button settingsBtn;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ToolStripMenuItem открытьToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem выходToolStripMenuItem;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.Button stopGoodBtn;
        private System.Windows.Forms.Button importDocBtn;
        private System.Windows.Forms.Button stopDocsBtn;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button uploadBtn;
        private System.Windows.Forms.Button downloadBtn;
        private System.Windows.Forms.Splitter splitter1;
    }
}

