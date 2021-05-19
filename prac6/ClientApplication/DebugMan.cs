using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApplication
{
    public class DebugMan //TODO remove
    {
        public static void Line(string text)
        {
            Debug.WriteLine($"|DEBUG: {text}");
        }
    }
}
