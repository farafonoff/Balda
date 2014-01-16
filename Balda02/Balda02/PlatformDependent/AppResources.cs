using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace Balda02.PlatformDependent
{
    class ResLoader
    {
        static ResourceLoader loader = new ResourceLoader();
        public static string getRes(string rn)
        {
            return loader.GetString(rn);
        }

    }
}
