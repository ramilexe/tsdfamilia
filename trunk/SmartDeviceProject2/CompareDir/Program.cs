using System;

using System.Collections.Generic;
using System.Windows.Forms;

namespace CompareDir
{
    static class Program
    {
        public static SettingsDataSet Settings = null;

        public static SettingsDataSet.TypedSettingsRow Default
        {
            get
            {
                return Settings.TypedSettings[0];
            }
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {
            Settings = new SettingsDataSet();
            string settingFilePath = string.Empty;
            //try
            //{
            string _startupPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().GetName().CodeBase);
            settingFilePath = System.IO.Path.Combine(_startupPath, "DiffSettings.xml");

            if (System.IO.File.Exists(settingFilePath))
            {
                Settings.ReadXml(settingFilePath);
                if (Settings.TypedSettings.Rows.Count == 0)
                {
                    SettingsDataSet.TypedSettingsRow r =
                    Settings.TypedSettings.NewTypedSettingsRow();

                    r.Catalog1 = _startupPath;
                    r.Catalog2 = "\\Program Files\\tsdfamilia\\";
                    r.ExeProgramPath = "\\Program Files\\tsdfamilia\\TsdClient.exe";
                    Settings.TypedSettings.AddTypedSettingsRow(r);
                    Settings.WriteXml(settingFilePath);
                }
            }
            else
            {
                SettingsDataSet.TypedSettingsRow r = 
                    Settings.TypedSettings.NewTypedSettingsRow();

                r.Catalog1 = _startupPath;
                r.Catalog2 = "\\Program Files\\tsdfamilia\\";
                r.ExeProgramPath = "\\Program Files\\tsdfamilia\\TsdClient.exe";
                Settings.TypedSettings.AddTypedSettingsRow(r);
                Settings.WriteXml(settingFilePath);
            }
            Settings.AcceptChanges();




            Application.Run(new Form1());
        }
    }
}