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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
        public int LineWidth { get; set; } = 256;

        [Browsable(true)]
        public int LineHeight { get; set; } = 64;

        [Browsable(true)]
        public int IconWight { get; set; } = 64;

        [Browsable(true)]
        public int IconHeight { get; set; } = 64;
        public List<int> SelectedItems { get; set; } = new List<int>();

        public event DrawDelegate Draw = null;
        private StringFormat format = new StringFormat();
        int headHeight;         // Высота шапки
        int width;              // Ширина видимого поля
        int height;             // Высота видимого поля (без шапки)
        int itemWidth;          // Ширина элемента
        int itemHeight;         // Высота элемента
        int select = -1;        // Выбранный элемент (последнее нажатие)
        int firstSelect = -1;   // Первый выбранный элемент
        bool arrows = false;    // Пользователь нажимает на стрелки
        bool shift = false;     // Пользователь жмёт Shift
        bool ctrl = false;      // Пользователь жмёт Control

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
            //Style = Styles.Lines;
            //Style = Styles.Icons;
            ItemsCount = 10;

            base.OnResize(e);

            //Вычиление размеров всего полотна данных
            int w;
            int h;
            int rows = 0;   // Количество строк
            if (Style == Styles.Table)
            {
                itemWidth = 0;
                foreach (Column col in Columns)
                    itemWidth += col.Width;
                itemHeight = TabStringHeight;
                rows = Items.Count;
            }
            if (Style == Styles.Lines)
            {
                itemWidth = LineWidth;
                itemHeight = LineHeight;
                rows = ItemsCount;
            }
            if (Style == Styles.Icons)
            {
                itemWidth = IconWight;
                itemHeight = IconHeight;
                rows = (int)Math.Ceiling((double)ItemsCount / ((Width - vScroll.Width) / IconWight));
                if (rows < 0) rows = ItemsCount;
            }
            w = itemWidth;
            h = rows * itemHeight;
            
            // Включение/выключение скроллбаров
            bool vs = true;
            bool hs = true;
            if (w < Width - vScroll.Width) hs = false;
            if (h < Height - hScroll.Height - headHeight) vs = false;
            if (w < Width & h < Height) { vs = false; hs = false; }
            vScroll.Height = Height - (hs ? hScroll.Height : 0) - 2;
            hScroll.Width = Width - (vs ? vScroll.Width : 0) - 2;
            vScroll.Visible = vs;
            hScroll.Visible = hs;

            // Вычисление размеров видимого поля
            headHeight = (Style == Styles.Table ? HeadHeight : 0);
            width = Width - (vs ? vScroll.Width : 0);
            height = Height - headHeight - (hs ? hScroll.Height : 0);
            if (width < 0) width = 0;
            if (height < 0) height = 0;

            // Коррекция размеров элементов
            if (Style == Styles.Lines && itemWidth < width) itemWidth = width - 1;
            if (Style == Styles.Icons && IconWight < width) itemWidth = (width - 1) / (width / IconWight);

            // Задание параметров скроллбаров
            if (vs)
            {
                if (ScrollType == ScrollTypes.ByString)
                {
                    vScroll.Maximum = rows - 1;
                    vScroll.SmallChange = 1;
                    vScroll.LargeChange = height / itemHeight;
                }
                if (ScrollType == ScrollTypes.ByPixel)
                {
                    vScroll.Maximum = rows * itemHeight;
                    vScroll.SmallChange = itemHeight;
                    vScroll.LargeChange = height;
                }
                if (vScroll.Value > vScroll.Maximum - vScroll.LargeChange + 1)
                    vScroll.Value = vScroll.Maximum >= vScroll.LargeChange ? vScroll.Maximum - vScroll.LargeChange + 1: 0;
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
            if (vs & hs)
            {
                panel.Left = vScroll.Left;
                panel.Top = hScroll.Top;
                panel.Width = vScroll.Width;
                panel.Height = hScroll.Height;
                panel.Visible = true;
            }
            else
                panel.Visible = false;
            
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

            //Draw?.Invoke(g); //Вызвать рисование пользователем
            int ys = headHeight;
            int first = vScroll.Value / (ScrollType == ScrollTypes.ByPixel ? itemHeight : 1);
            for (int i = first; i < first + height / itemHeight + 2; i++)
            {
                int y = ys - vScroll.Value * (ScrollType == ScrollTypes.ByString ? itemHeight : 1) + i * itemHeight;
                int x = -hScroll.Value;
                if (Style == Styles.Table)
                {
                    if (i < Items.Count)
                    {
                        for (int j = 0; j < Columns.Count; j++)
                        {
                            Rectangle cell = new Rectangle(x, y, Columns[j].Width - 1, itemHeight - 1);
                            bool sel = SelectedItems.IndexOf(i) >= 0;
                            if (sel) g.FillRectangle(SystemBrushes.Highlight, cell); 
                            g.DrawString(Items[i].Text[j], Font, (sel ? SystemBrushes.HighlightText : SystemBrushes.ControlText), cell, format);
                            x += Columns[j].Width;
                        }
                    }
                }
                if (Style == Styles.Lines)
                {
                    if (i < ItemsCount)
                    {
                        Rectangle cell = new Rectangle(x, y, itemWidth - 1, itemHeight - 1);
                        bool sel = SelectedItems.IndexOf(i) >= 0;
                        if (sel) g.FillRectangle(SystemBrushes.Highlight, cell);
                        g.DrawString(i.ToString(), Font, (sel ? SystemBrushes.HighlightText : SystemBrushes.ControlText), cell, format);
                        
                    }
                }
                if (Style == Styles.Icons)
                {
                    int c = width / itemWidth;
                    if (c < 1) c = 1;
                    int n = i * c;
                    for (int j = 0; j < c; j++)
                    {
                        if (n < ItemsCount)
                        {
                            Rectangle cell = new Rectangle(x, y, itemWidth - 1, itemHeight - 1);
                            bool sel = SelectedItems.IndexOf(n) >= 0;
                            if (sel) g.FillRectangle(SystemBrushes.Highlight, cell);
                            g.DrawString(n.ToString(), Font, (sel ? SystemBrushes.HighlightText : SystemBrushes.ControlText), cell, format);
                        }
                        n++;
                        x += itemWidth;
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
                    g.DrawLine(Pens.LightGray, x - hScroll.Value + col.Width - 1, headHeight, x - hScroll.Value + col.Width - 1, headHeight + height - 1);
                    x += col.Width;
                    //Cursor = Cursors.VSplit;
                }
            }

            //Внешняя рамка
            g.DrawRectangle(Focused? Pens.Black: Pens.Gray, border);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            if (arrows) Focus();
            arrows = false;
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

        private void SList_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            int max = (Style == Styles.Table ? Items.Count : ItemsCount) - 1;
            int ud = 1;
            int lr = 0;
            if (Style == Styles.Icons)
            {
                ud = width / itemWidth;
                lr = 1;
            }
            int pg = ud * (height / itemHeight);

            arrows = e.KeyCode == Keys.Up | e.KeyCode == Keys.Down | e.KeyCode == Keys.Left | e.KeyCode == Keys.Right |
                e.KeyCode == Keys.Home | e.KeyCode == Keys.End | e.KeyCode == Keys.PageUp | e.KeyCode == Keys.PageDown;
            if (e.KeyCode == Keys.ShiftKey) shift = true;
            if (e.KeyCode == Keys.ControlKey) ctrl = true;

            if (arrows & select < 0) select = 0;

            if (e.KeyCode == Keys.Up) select -= ud;
            if (e.KeyCode == Keys.Down) select += ud;
            if (e.KeyCode == Keys.Left) select -= lr;
            if (e.KeyCode == Keys.Right) select += lr;
            if (e.KeyCode == Keys.Home) select = 0;
            if (e.KeyCode == Keys.End) select = max;
            if (e.KeyCode == Keys.PageUp) select -= pg;
            if (e.KeyCode == Keys.PageDown) select += pg;

            if (arrows)
            {
                if (select < 0) select = 0;
                if (select > max) select = max;
                ctrl = false;
                Selection();


                //Корректировка скролл-баров
                int i = Style == Styles.Icons ? select / ud : select;
                if (select >= 0)
                {
                    if (vScroll.Value > i * itemHeight) vScroll.Value = i * itemHeight;
                    int min = height - itemHeight - 1;
                    if (vScroll.Value < i * itemHeight - min) vScroll.Value = i * itemHeight - min;
                }
            }
            Invalidate();
        }

        private void SList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey) shift = false;
            if (e.KeyCode == Keys.ControlKey) ctrl = false;
        }

        private void SList_MouseDown(object sender, MouseEventArgs e)
        {
            int max = (Style == Styles.Table ? Items.Count : ItemsCount) - 1;
            int cols = Style == Styles.Icons ? width / itemWidth : 1;
            select = (e.Y + vScroll.Value - headHeight) / itemHeight * cols + e.X / itemWidth;
            if (select > max) select = -1;
            Selection();

            //Корректировка скролл-баров
            int i = Style == Styles.Icons ? select / cols : select;
            
            if (select >= 0)
            {
                if (vScroll.Value > i * itemHeight) vScroll.Value = i * itemHeight;
                int min = height - itemHeight - 1;
                if (vScroll.Value < i * itemHeight - min) vScroll.Value = i * itemHeight - min;
            }

            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            int i = vScroll.Value - e.Delta;
            if (i < 0) i = 0;
            if (i > vScroll.Maximum - vScroll.LargeChange) i = vScroll.Maximum - vScroll.LargeChange;
            vScroll.Value = i;
        }

        void Selection()
        {
            if (shift & ctrl) return;
            if (!ctrl) SelectedItems.Clear();
            if (select < 0)
            {
                firstSelect = -1;
                return;
            }

            if (shift)
            {
                if (firstSelect < 0) firstSelect = 0;
                if (firstSelect < select)
                    for (int j = firstSelect; j <= select; j++)
                        SelectedItems.Add(j);
                else
                    for (int j = firstSelect; j >= select; j--)
                        SelectedItems.Add(j);
            }
            if (ctrl)
            {
                firstSelect = select;
                if (SelectedItems.IndexOf(select) < 0)
                    SelectedItems.Add(select);
                else
                    SelectedItems.Remove(select);
            }
            if (!shift & !ctrl) 
            {
                firstSelect = select;
                SelectedItems.Add(select);
            }
        }

    }
}
