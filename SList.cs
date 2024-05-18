using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Common;
using static System.Windows.Forms.LinkLabel;

namespace SControls
{
    public delegate void DrawDelegate(Graphics g);
    public partial class SList: UserControl
    {
        public enum Styles { Table, Lines, Icons }
        
        [Browsable(true)]
        public Styles Style { get; set; }

        [Browsable(true)]
        public List<Column> Columns { get; set; }

        public event DrawDelegate Draw = null;

        const int hh = 20;  //Высота шапки
        private StringFormat format = new StringFormat();

        public SList()
        {
            Style = Styles.Table;
            Columns = new List<Column>();
            InitializeComponent();
            DoubleBuffered = true;
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            int width = Width - 18; //1+17 - это ширина скролбарров, потом как-то красивше сделать
            int height = Height - 38;   //1+20+17
            Rectangle head = new Rectangle(0, 0, width, hh);
            Rectangle field = new Rectangle(0, hh, width, height);
            Rectangle border = new Rectangle(0, 0, Width - 1, Height - 1);

            g.FillRectangle(new SolidBrush(BackColor), head);
            g.DrawLine(Pens.Gray, 0, hh - 1, width, hh - 1);
            g.FillRectangle(Brushes.White, field);
            //g.DrawRectangle(Pens.Black, field);

            int x = 0;
            foreach (Column col in Columns)
            {
                Rectangle h = new Rectangle(x, 0, col.Width, hh);
                g.DrawString(col.Text, Font, Brushes.Black, h, format);
                g.DrawLine(Pens.Gray, x + col.Width - 1, 0, x + col.Width - 1, hh - 1);
                g.DrawLine(Pens.LightGray, x + col.Width - 1, hh, x + col.Width - 1, Height - 18);
                x += col.Width;
                //Cursor = Cursors.VSplit;
            }
            Draw?.Invoke(g);
            g.DrawRectangle(Pens.Black, border);
        }


        protected override void OnChangeUICues(UICuesEventArgs e)
        {
            base.OnChangeUICues(e);
            Invalidate();
        }
    }
}
