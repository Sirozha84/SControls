﻿using System;
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

        [Browsable(true)]
        public bool TabStop { get; set; } = true;

        public event DrawDelegate Draw = null;
        private StringFormat format = new StringFormat();
        int headHeight;     // Высота шапки
        int width;          // Ширина видимого поля
        int height;         // Высота видимого поля (без шапки)
        int itemWidth;      // Ширина элемента
        int itemHeight;     // Высота элемента
        int select = -1;    // Выбранный элемент (последнее нажатие)
        bool arows = false; // Пользователь нажимает на стрелки

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
            Style = Styles.Icons;
            ItemsCount = 10;
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
                itemHeight = LineHeight;
                sc = ItemsCount;
            }
            if (Style == Styles.Icons)
            {
                itemWidth = IconWight;
                itemHeight = IconHeight;
                sc = (int)Math.Ceiling((double)ItemsCount / ((Width - vScroll.Width) / IconWight));
                if (sc < 0) sc = ItemsCount;
            }
            w = itemWidth;
            h = sc * itemHeight;
            
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
                    vScroll.Maximum = sc - 1;
                    vScroll.SmallChange = 1;
                    vScroll.LargeChange = height / itemHeight;
                }
                if (ScrollType == ScrollTypes.ByPixel)
                {
                    vScroll.Maximum = sc * itemHeight;
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
                            if (i == select) g.FillRectangle(SystemBrushes.Highlight, cell); 
                            g.DrawString(Items[i].Text[j], Font, (i == select ? SystemBrushes.HighlightText : SystemBrushes.ControlText), cell, format);
                            x += Columns[j].Width;
                        }
                    }
                }
                if (Style == Styles.Lines)
                {
                    if (i < ItemsCount)
                    {
                        Rectangle cell = new Rectangle(x, y, itemWidth - 1, itemHeight - 1);
                        if (i == select) g.FillRectangle(SystemBrushes.Highlight, cell);
                        g.DrawString(i.ToString(), Font, (i == select ? SystemBrushes.HighlightText : SystemBrushes.ControlText), cell, format);
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
                            if (n == select) g.FillRectangle(SystemBrushes.Highlight, cell);
                            g.DrawString(n.ToString(), Font, (n == select ? SystemBrushes.HighlightText : SystemBrushes.ControlText), cell, format);
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
            if (arows) Focus();
            arows = false;
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

        private void SList_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void SList_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void hScroll_Enter(object sender, EventArgs e)
        {
        }

        private void SList_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            int maxc = 0;
            int maxr = 0;
            if (Style == Styles.Table & Items.Count > 0)
                maxr = Items.Count - 1;
            if (Style == Styles.Lines & ItemsCount > 0)
                maxr = ItemsCount - 1;
            if (Style == Styles.Icons & ItemsCount > 0)
            {
                maxc = width / itemWidth;
                maxr = (int)Math.Ceiling((double)ItemsCount / maxc);
            }

            if (e.KeyCode == Keys.Up) select = select < 0 ? 0 : select-1;
            if (e.KeyCode == Keys.Down) select = select > maxr ? 0 : select+1;
            if (select < 0) select = 0;
            if (select > maxr) select = maxr;
            
            if (e.KeyCode == Keys.Up | e.KeyCode == Keys.Down) arows = true;
            if (e.KeyCode == Keys.Left | e.KeyCode == Keys.Right) arows = true;

            Invalidate();
        }
    }
}
