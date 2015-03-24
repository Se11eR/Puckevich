using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuckevichCore.Exceptions
{
    public class BassException : Exception
    {
        public BassException(string m)
            : base(m)
        {

        }
    }
}
