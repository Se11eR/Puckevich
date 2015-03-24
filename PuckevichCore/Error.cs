using System;
using PuckevichCore.Exceptions;
using Un4seen.Bass;

namespace PuckevichCore
{
    internal class Error
    {
        internal static void HandleBASSError(string BASSfuncName, object errorCode)
        {
            throw new BassException(BASSfuncName + "(): error " + errorCode.ToString());
        }

        internal static void HandleBASSError(string BASSfuncName)
        {
            throw new BassException(BASSfuncName + "(): error " + Bass.BASS_ErrorGetCode());
        }
    }
}
