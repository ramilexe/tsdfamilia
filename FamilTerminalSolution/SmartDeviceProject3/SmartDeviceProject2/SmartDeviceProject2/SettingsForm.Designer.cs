﻿namespace TSDServer
{
    partial class SettingsForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button0 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.PaleGreen;
            this.button1.Dock = System.Windows.Forms.DockStyle.Top;
            this.button1.Location = new System.Drawing.Point(0, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(640, 35);
            this.button1.TabIndex = 0;
            this.button1.Text = "1 - Настройка принтера";
            // 
            // button3
            // 
            this.button2.BackColor = System.Drawing.Color.PaleGreen;
            this.button2.Dock = System.Windows.Forms.DockStyle.Top;
            this.button2.Enabled = true;
            this.button2.Location = new System.Drawing.Point(0, 35);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(640, 35);
            this.button2.TabIndex = 2;
            this.button2.Text = "2 - Очистка данных";
            // 
            // button4
            // 
            this.button3.BackColor = System.Drawing.Color.PaleGreen;
            this.button3.Dock = System.Windows.Forms.DockStyle.Top;
            this.button3.Enabled = true;
            this.button3.Location = new System.Drawing.Point(0, 70);
            this.button3.Text = "3 - Информация о данных";
            this.button3.Size = new System.Drawing.Size(640, 35);
            this.button3.TabIndex = 3;
            this.button3.Name = "button3";
            // 
            // button5
            // 
            this.button4.Dock = System.Windows.Forms.DockStyle.Top;
            this.button4.Enabled = false;
            this.button4.Location = new System.Drawing.Point(0, 105);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(640, 35);
            this.button4.TabIndex = 4;
            // 
            // button6
            // 
            this.button5.Dock = System.Windows.Forms.DockStyle.Top;
            this.button5.Enabled = false;
            this.button5.Location = new System.Drawing.Point(0, 140);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(640, 35);
            this.button5.TabIndex = 5;
            // 
            // button0
            // 
            this.button0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button0.Location = new System.Drawing.Point(0, 175);
            this.button0.Name = "button0";
            this.button0.Size = new System.Drawing.Size(640, 305);
            this.button0.TabIndex = 8;
            this.button0.Text = "0 - Назад";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(640, 480);
            this.ControlBox = false;
            this.Controls.Add(this.button0);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Menu = this.mainMenu1;
            this.Name = "SettingsForm";
            this.Text = "Настройки";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button0;
    }
}