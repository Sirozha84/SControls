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
        public Styles Style { get; set; } = Styles.Table;

        [Browsable(true)]
        public List<Column> Columns { get; set; } = new List<Column>();

        [Browsable(true)]
        public int HeadHeight { get; set; } = 24;

        [Browsable(true)]
        public List<SListItem> Items { get; set; } = new List<SListItem>();

        public event DrawDelegate Draw = null;

        private StringFormat format = new StringFormat();

        public SList()
        {
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
            vScroll.Visible = false;
            hScroll.Visible = false;
            int width = Width - (vScroll.Visible ? vScroll.Width : 0) - 1;
            int height = Height - HeadHeight - (hScroll.Visible ? hScroll.Height : 0) - 1;
            Rectangle head = new Rectangle(0, 0, width, HeadHeight);
            Rectangle field = new Rectangle(0, HeadHeight, width, height);
            Rectangle border = new Rectangle(0, 0, Width - 1, Height - 1);


            g.FillRectangle(Brushes.White, field);
            //g.FillRectangle(Brushes.LightGreen, field);


            //Рисование шапки, если стиль - таблица
            if (Style == Styles.Table)
            {
                g.FillRectangle(new SolidBrush(BackColor), head);
                g.DrawLine(Pens.Gray, 0, HeadHeight - 1, width - 1, HeadHeight - 1);

                int x = 0;
                foreach (Column col in Columns)
                {
                    Rectangle h = new Rectangle(x, 0, col.Width, HeadHeight);
                    g.DrawString(col.Text, Font, Brushes.Black, h, format);
                    g.DrawLine(Pens.Gray, x + col.Width - 1, 0, x + col.Width - 1, HeadHeight - 1);
                    g.DrawLine(Pens.LightGray, x + col.Width - 1, HeadHeight, x + col.Width - 1, HeadHeight + height - 1);
                    x += col.Width;
                    //Cursor = Cursors.VSplit;
                }
            }


            //Draw?.Invoke(g); //Вызвать рисование пользователем
            g.DrawRectangle(Pens.Black, border);
        }


        protected override void OnChangeUICues(UICuesEventArgs e)
        {
            base.OnChangeUICues(e);
            Invalidate();
        }
    }
}
