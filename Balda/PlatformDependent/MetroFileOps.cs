#if NETFX_CORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balda.PlatformDependent
{
    class FileOps
    {
        internal static void appendLine(string p, string w)
        {
            throw new NotImplementedException();
        }

        internal static string getFromResources(string p)
        {
            throw new NotImplementedException();
        }

        internal static string getFromStorage(string p)
        {
            throw new NotImplementedException();
        }

        internal static void ensureExists(string p)
        {
            throw new NotImplementedException();
        }
    }
}
#endif