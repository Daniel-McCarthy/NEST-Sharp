using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NEST.Classes
{

    class Rom
    {
        private byte[] romData;
        public Rom(byte[] fileData)
        {
            romData = fileData;
        }
    }
}
