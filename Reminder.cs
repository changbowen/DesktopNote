using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopNote
{
    class Reminder
    {
        public DateTime Time { get; set; }
        public int StartPos { get; set; }
        public int EndPos { get; set; }
        public string Comment { get; set; }
    }
}
