using System;
using System.Data;
using System.Collections.Generic;
using System.Text;

namespace Familia.TSDClient
{
    public class BaseTableAdapter
    {
        private object locker = new object();
        protected string[] _fileList;
        public string[] FileList
        {
            get
            {
                return _fileList;
            }

        }
        protected FamilTsdDB.DataTable table = null;
        protected System.Data.DataTable _sysdatemdatatable;
        protected bool _disposed = false;
        private bool _opened = false;

        public bool Opened
        {
            get { return _opened; }
            set { _opened = value; }
        }
        public BaseTableAdapter(System.Data.DataTable sysdatemdatatable)
        {
            _sysdatemdatatable = sysdatemdatatable;
        }

        protected virtual void Init()
        {
            
            table = new FamilTsdDB.DataTable(_sysdatemdatatable);
            table.ReadTableDef();
            _fileList = table.FileList.ToArray();
            _opened = true;
        }
        public void Update()
        {
            lock (locker)
            {
                if (table == null)
                    Init();
                //table = new FamilTsdDB.DataTable(_sysdatemdatatable);
                table.Write();
            }
        }

        public void Update(System.Data.DataTable outerTable)
        {
            lock (locker)
            {
                if (table == null)
                    Init();
                    //table = new FamilTsdDB.DataTable(outerTable);
                table.Write();
            }
        }
        public void Fill()
        {
            lock (locker)
            {
                try
                {
                    table.ReadTableDef();
                    this.table.Fill(_sysdatemdatatable);
                }
                catch { };
                _fileList = table.FileList.ToArray();
            }
        }

        public void Fill(System.Data.DataTable outertable)
        {
            lock (locker)
            {
                try
                {
                    table.ReadTableDef();
                    this.table.Fill(outertable);
                }
                catch { };
                _fileList = table.FileList.ToArray();
            }
        }

        public void Open()
        {
            Init();
        }

        public void Close()
        {
            lock (locker)
            {
                table.Dispose();
                _opened = false;
            }
        }

        public virtual void Clean()
        {
            _sysdatemdatatable.Clear();
            //try
            //{
            //    table.Dispose();
            //    foreach (string fileName in _fileList)
            //        System.IO.File.Delete(fileName);
            //}
            //catch { }
            //Init();
        }
        #region IDisposable Members

        public void Dispose()
        {
            if (!_disposed)
            {
                table.Dispose();
                table = null;
                _disposed = true;
            }

        }

        #endregion

    }
}
