using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OpenNETCF.Desktop.Communication
{
    public partial class OpenDeviceFileDialog : Form
    {
        RAPI rapi = new RAPI();
        public OpenDeviceFileDialog()
        {
            InitializeComponent();
        }

        private void OpenDeviceFileDialog_Load(object sender, EventArgs e)
        {
            if (rapi.DevicePresent)
            {
                rapi.CheckConnection();
                if (!rapi.Connected)
                {
                    rapi.Connect();
                    FileList fl = rapi.EnumFiles("*.*");
                    TreeNode baseNode = treeView1.Nodes.Add("\\");
                    baseNode.SelectedImageIndex = 0;
                    baseNode.ImageIndex = 0;
                    foreach (FileInformation fi in fl)
                    {
                        System.IO.FileAttributes fa =
                            (System.IO.FileAttributes)fi.FileAttributes;

                        TreeNode tn = baseNode.Nodes.Add(fi.FileName);
                        if (fa == System.IO.FileAttributes.Directory)
                        {
                            tn.SelectedImageIndex = 1;
                            tn.ImageIndex = 1;
                        }
                        if (fa == System.IO.FileAttributes.Temporary)
                        {
                            tn.SelectedImageIndex = 1;
                            tn.ImageIndex = 1;
                        }
                    }

                }
            }
        }
    }
}
