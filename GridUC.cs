using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Familia.TSDClient
{
    public partial class GridUC : UserControl
    {
        int _rows =0;
        public int Rows
        {
            get
            {
                return _rows;
            }
            set
            {
                _rows = value;
            }
        }

        int _cols = 0;
        public int Columns
        {
            get
            {
                return _cols;
            }
            set
            {
                _cols = value;
            }
        }

        Panel[,] panels = new Panel[0,0];

        public GridUC()
        {
            InitializeComponent();
            
        }

        protected override ControlCollection CreateControlsInstance()
        {
            panels = new Panel[_rows, _cols];
            for (int i = 0; i < _rows; i++)
                for (int j = 0; j < _cols; j++)
                {
                    panels[i, j] = new Panel();
                    Rectangle rect = GetRect(i,j);
                    panels[i, j].Width = rect.Width;
                    panels[i, j].Height = rect.Width;
                    panels[i, j].Top = rect.Top;
                    panels[i, j].Left = rect.Left;

                }

            return base.CreateControlsInstance();
        }

        protected override void OnResize(EventArgs e)
        {
            if (panels.GetLength(0) < _rows || panels.GetLength(1) < _cols)
            {
                panels = new Panel[_rows, _cols];
                
            }
            

            for (int i = 0; i < _rows; i++)
                for (int j = 0; j < _cols; j++)
                {
                    if (panels[i,j] == null)
                        panels[i, j] = new Panel();

                    Rectangle rect = GetRect(i, j);
                    panels[i, j].Width = rect.Width;
                    panels[i, j].Height = rect.Width;
                    panels[i, j].Top = rect.Top;
                    panels[i, j].Left = rect.Left;

                }
            base.OnResize(e);

        }

        public override void Refresh()
        {
            for (int i = 0; i < _rows; i++)
                for (int j = 0; j < _cols; j++)
                {
                    Rectangle rect = GetRect(i, j);
                    panels[i, j].Width = rect.Width;
                    panels[i, j].Height = rect.Width;
                    panels[i, j].Top = rect.Top;
                    panels[i, j].Left = rect.Left;

                }

            base.Refresh();
        }
        public Point GetPosition(int i, int j)
        {
            int height = this.Height/_rows;
            int width = this.Width/_cols;

            int x = height * i;
            int y = width * j;

            return new Point(x, y);

        }

        public Rectangle GetRect(int i, int j)
        {
            int height = this.Height / _rows;
            int width = this.Width / _cols;

            int x = height * i;
            int y = width * j;

            return new Rectangle(x, y, width, height);

        }
    }
}
