using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bowling_Runway
{
    public class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            // הפעלת Double Buffering
            this.DoubleBuffered = true;

            // הפעלת תכונות ציור משופרות למניעת ריצוד
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint, true);
            this.UpdateStyles();
        }
    }
}
