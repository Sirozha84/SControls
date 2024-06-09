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
        public enum ScrollTypes { ByString, ByPixel }

        [Browsable(true)]
        public Styles Style { get; set; } = Styles.Table;

        [Browsable(true)]
        public ScrollTypes ScrollType { get; set; } = ScrollTypes.ByPixel;

        [Browsable(true)]
        public List<Column> Columns { get; set; } = new List<Column>();

        [Browsable(true)]
        public int HeadHeight { get; set; } = 24;

        [Browsable(true)]
        public List<SListItem> Items { get; set; } = new List<SListItem>();

        [Browsable(true)]
        public int ItemsCount { get; set; } = 0;

        [Browsable(true)]
        public int TabStringHeight { get; set; } = 24;

        [Browsable(true)]
        public int LineWidth { get; set; } = 128;

        [Browsable(true)]
        public int LineHeight { get; set; } = 48;

        [Browsable(true)]
        public int IconWight { get; set; } = 64;

        [Browsable(true)]
        public int IconHeight { get; set; } = 64;

        public event DrawDelegate Draw = null;
        private StringFormat format = new StringFormat();
        int headHeight;
        int width;
        int height;

        public SList()
        {
            InitializeComponent();
            DoubleBuffered = true;
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
        }

        protected override void OnResize(EventArgs e)
        {
            //Style = Styles.Icons;
            //ItemsCount = 1;
            base.OnResize(e);

            headHeight = (Style == Styles.Table ? HeadHeight : 0);
            width = Width - (vScroll.Visible ? vScroll.Width : 0) - 1;
            height = Height - headHeight - (hScroll.Visible ? hScroll.Height : 0) - 1;

            int w = 0;
            int h = 0;
            int sv = 0;     // Количество видимых строк
            int sc = 0;     // Количество строк всего
            if (Style == Styles.Table)
            {
                foreach (Column col in Columns)
                    w += col.Width;
                sv = (height - headHeight) / TabStringHeight;
                sc = Items.Count;
                h = sc * TabStringHeight + HeadHeight;
            }
            if (Style == Styles.Lines)
            {
                w = LineWidth;
                sv = height / LineHeight;
                sc = ItemsCount;
                h = sc * LineHeight;
            }
            if (Style == Styles.Icons)
            {
                w = 0;
                sv = height / IconHeight;
                sc = (int)Math.Ceiling((double)ItemsCount / (Width - vScroll.Width) / IconWight);
                h = sc * IconHeight;
            }            
            bool vs = true;
            bool hs = true;
            if (w < Width - vScroll.Width) hs = false;
            if (h < Height - hScroll.Height) vs = false;
            if (w < Width & h < Height) { vs = false; hs = false; }
            vScroll.Height = Height - (hs ? hScroll.Height : 0) - 2;
            hScroll.Width = Width - (vs ? vScroll.Width : 0) - 2;
            vScroll.Visible = vs;
            hScroll.Visible = hs;
            if (vs)
            {
                if (ScrollType == ScrollTypes.ByString)
                {
                    vScroll.Maximum = sc-sv;
                }
                if (ScrollType == ScrollTypes.ByPixel)
                {
                    vScroll.Maximum = sc-sv;
                    vScroll.LargeChange = sv;
                }
            }

            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Rectangle head = new Rectangle(0, 0, width, headHeight);
            Rectangle field = new Rectangle(0, headHeight, width, height);
            Rectangle border = new Rectangle(0, 0, Width - 1, Height - 1);

            Graphics g = e.Graphics;
            g.FillRectangle(Brushes.White, field);
            //g.FillRectangle(Brushes.LightGreen, field);

            //Draw?.Invoke(g); //Вызвать рисование пользователем
            //g.DrawString(vScroll.Value.ToString(), Font, Brushes.Black, 0, 30);
            int y = 24;
            foreach (SListItem item in Items)
            {
                g.DrawString(item.Text[0], Font, Brushes.Black, -hScroll.Value, y - vScroll.Value* TabStringHeight);
                y += TabStringHeight;
            }


            //Рисование шапки, если стиль - таблица
            if (Style == Styles.Table)
            {
                g.FillRectangle(new SolidBrush(BackColor), head);
                g.DrawLine(Pens.Gray, 0, headHeight - 1, width - 1, headHeight - 1);

                int x = 0;
                foreach (Column col in Columns)
                {
                    Rectangle h = new Rectangle(x, 0, col.Width, headHeight);
                    g.DrawString(col.Text, Font, Brushes.Black, h, format);
                    g.DrawLine(Pens.Gray, x + col.Width - 1, 0, x + col.Width - 1, headHeight - 1);
                    g.DrawLine(Pens.LightGray, x + col.Width - 1, headHeight, x + col.Width - 1, headHeight + height - 1);
                    x += col.Width;
                    //Cursor = Cursors.VSplit;
                }
            }

            //Внешняя рамка
            g.DrawRectangle(Pens.Black, border);
        }


        protected override void OnChangeUICues(UICuesEventArgs e)
        {
            base.OnChangeUICues(e);
            Invalidate();
        }

        private void vScroll_ValueChanged(object sender, EventArgs e)
        {
            Invalidate();
        }
    }
}
