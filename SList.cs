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
        int headHeight;     // Высота шапки
        int width;          // Ширина видимого поля
        int height;         // Высота видимого поля (без шапки)
        int itemWidth;      // Ширина элемента
        int itemHeight;     // Высота элемента

        public SList()
        {
            InitializeComponent();
            DoubleBuffered = true;
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
        }


        public override void Refresh()
        {
            base.Refresh();
            OnResize(null);
        }

        protected override void OnResize(EventArgs e)
        {
            //ScrollType = ScrollTypes.ByString;
            //Style = Styles.Icons;
            //ItemsCount = 1;
            base.OnResize(e);


            //Вычиление размеров всего полотна данных
            int w;
            int h;
            int sc = 0;     // Количество строк всего
            if (Style == Styles.Table)
            {
                itemWidth = 0;
                foreach (Column col in Columns)
                    itemWidth += col.Width;
                itemHeight = TabStringHeight;
                sc = Items.Count;
            }
            if (Style == Styles.Lines)
            {
                itemWidth = LineWidth;
                itemHeight = TabStringHeight;
                sc = ItemsCount;
            }
            if (Style == Styles.Icons)
            {
                itemWidth = IconWight;
                itemHeight = IconHeight;
                sc = (int)Math.Ceiling((double)ItemsCount / (width - vScroll.Width) / IconWight);
            }
            w = itemWidth;
            h = sc * itemHeight;
            bool vs = true;
            bool hs = true;
            if (w < Width - vScroll.Width) hs = false;
            if (h < Height - hScroll.Height) vs = false;
            if (itemWidth < Width & h < Height) { vs = false; hs = false; }
            vScroll.Height = Height - (hs ? hScroll.Height : 0) - 2;
            hScroll.Width = Width - (vs ? vScroll.Width : 0) - 2;
            vScroll.Visible = vs;
            hScroll.Visible = hs;
            headHeight = (Style == Styles.Table ? HeadHeight : 0);

            width = Width - (vScroll.Visible ? vScroll.Width : 0);// - 1;
            height = Height - headHeight - (hScroll.Visible ? hScroll.Height : 0);// - 1;
            if (width < 0) width = 0;
            if (height < 0) height = 0;
            
            if (vs)
            {
                if (ScrollType == ScrollTypes.ByString)
                {
                    vScroll.Maximum = sc - 1;
                    vScroll.SmallChange = 1;
                    vScroll.LargeChange = 1;
                }
                if (ScrollType == ScrollTypes.ByPixel)
                {
                    vScroll.Maximum = sc * itemHeight;
                    vScroll.SmallChange = itemHeight;
                    vScroll.LargeChange = height;
                }
            }
            else
            {
                vScroll.Value = 0;
            }
            if (hs)
            {
                hScroll.Maximum = w;
                hScroll.SmallChange = 16;
                hScroll.LargeChange = width;
            }
            else
            {
                hScroll.Value = 0;
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
            //g.DrawString(vScroll.Value.ToString()+" - "+ vScroll.LargeChange.ToString()+" - " + vScroll.Maximum.ToString(), Font, Brushes.Black, 30, 30);
            //g.DrawString(hScroll.Value.ToString()+" - "+ hScroll.LargeChange.ToString()+" - " + hScroll.Maximum.ToString(), Font, Brushes.Black, 30, 40);
            int ys = headHeight;
            int first = vScroll.Value / itemHeight;//  (ScrollType == ScrollTypes.ByString ? itemHeidht : 1);
            for (int i = first; i < first + height / itemHeight + 2; i++)
            {
                if (i < Items.Count)
                {
                    int y = ys - vScroll.Value * (ScrollType == ScrollTypes.ByString ? itemHeight : 1) + i * itemHeight;
                    int x = -hScroll.Value;
                    for (int j = 0; j < Columns.Count; j++)
                    {
                        Rectangle cell = new Rectangle(x, y, Columns[j].Width - 1, itemHeight - 1);
                        g.DrawString(Items[i].Text[j], Font, Brushes.Black, cell, format);
                        g.DrawRectangle(Pens.Black, cell);
                        x += Columns[j].Width;
                    }
                }
            }


            //Рисование шапки, если стиль - таблица
            if (Style == Styles.Table)
            {
                g.FillRectangle(new SolidBrush(BackColor), head);
                g.DrawLine(Pens.Gray, 0, headHeight - 1, width - 1, headHeight - 1);

                int x = 0;
                foreach (Column col in Columns)
                {
                    Rectangle h = new Rectangle(x - hScroll.Value, 0, col.Width, headHeight);
                    g.DrawString(col.Text, Font, Brushes.Black, h, format);
                    g.DrawLine(Pens.Gray, x - hScroll.Value + col.Width - 1, 0, x - hScroll.Value + col.Width - 1, headHeight - 1);
                    //g.DrawLine(Pens.LightGray, x - hScroll.Value + col.Width - 1, headHeight, x - hScroll.Value + col.Width - 1, headHeight + height - 1);
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

        private void vScrolling(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void hScrolling(object sender, EventArgs e)
        {
            Invalidate();
        }


    }
}
