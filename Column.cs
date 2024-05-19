using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SControls
{
    [Serializable]
    public class Column
    {
        public string Text { get; set; }
        public int Width { get; set; }
        public Column()
        {
            Text = "Колонка";
            Width = 60;
        }
        public Column(string text, int width)
        {
            this.Text = text;
            this.Width = width;
        }
    }
}
