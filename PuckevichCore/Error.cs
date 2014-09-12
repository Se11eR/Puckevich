using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;

namespace PuckevichCore
{
    internal class Error
    {
        internal static void HandleBASSError(string BASSfuncName, object errorCode)
        {
            throw new Exception(BASSfuncName + "(): error " + errorCode.ToString());
        }

        internal static void HandleBASSError(string BASSfuncName)
        {
            throw new Exception(BASSfuncName + "(): error " + Bass.BASS_ErrorGetCode());
        }
    }
}
