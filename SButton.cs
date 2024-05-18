using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ControlTest
{
    public partial class SButton : UserControl
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Bindable(true)]
        
        public override string Text { get; set; }
        
        StringFormat format = new StringFormat();
        bool mouse; // Наведён ли курсор
        bool click; // Нажата ли мышь
        bool push;  // Кнопка нажата

        public SButton()
        {
            InitializeComponent();
            DoubleBuffered = true;
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Rectangle rect0 = new Rectangle(0, 0, Width - 1, Height - 1);
            Rectangle rect = new Rectangle(0, push ? 1 : 0, Width - 1, Height - 1);
            if (mouse) g.FillRectangle(Brushes.White, rect);
            if (push) g.FillRectangle(Brushes.LightGray, rect);
            g.DrawRectangle(Pens.Black, rect0);
            g.DrawRectangle(Pens.Black, rect);
            g.DrawString(Text, Font, new SolidBrush(ForeColor), rect, format);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            mouse = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            mouse = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            click = true;
            push = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            click = false;
            push = false;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (click)
                push = e.Location.X > 0 & e.Location.X < Width & e.Location.Y > 0 & e.Location.Y < Height;
            Invalidate();
        }
    }
}
