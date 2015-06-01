using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClasses;

namespace ServerGUI
{
    class GuiWcfConnector : IGuiWcfConnector
    {
        private MainWindow window;

        public MainWindow Window
        {
            set
            {
                this.window = value;
            }
            get
            {
                return this.window;
            }
        }
        public void SendLiverEvent(String ev)
        {
            this.window.ParseLiverEvent(ev);
        }

        public void FlagLiverEvent()
        {

        }
    }
}
