using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SControls
{
    [Serializable]
    public class SListItem
    {
        public List<string> Text = new List<string>();
        
        public SListItem()
        {
        }

        public SListItem(string Text)
        {
            this.Text.Add(Text);
        }

        public SListItem(string[] Text)
        {
            this.Text = Text.ToList();
        }
    }
}
