using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuckevichCore.Exceptions
{
    public class AuthIDException : AuthException
    {
        public AuthIDException(string m) : base(m)
        {
        }
    }
}
