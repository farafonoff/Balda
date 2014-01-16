#if NETFX_CORE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Balda.PlatformDependent
{
    class FileOps
    {
        internal static async void appendLine(string p, string w)
        {
            await ensureExists(p);
            StorageFolder sf = ApplicationData.Current.RoamingFolder;
            using (Stream sm = await sf.OpenStreamForWriteAsync(p,CreationCollisionOption.OpenIfExists))
            {
                using (StreamWriter sr = new StreamWriter(sm))
                {
                    await sr.WriteLineAsync(w);
                }
            }
        }

        async internal static Task<string> getFromResources(string p)
        {
            string myFile = Path.Combine(Package.Current.InstalledLocation.Path, p);

            StorageFolder myFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(myFile));//await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(myFile));

            using (Stream s = await myFolder.OpenStreamForReadAsync(Path.GetFileName(myFile)))
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    string r = await sr.ReadToEndAsync();
                    return r;
                }
            }

        }

        async internal static Task<string> getFromStorage(string p)
        {
            await ensureExists(p);
            StorageFolder sf = ApplicationData.Current.RoamingFolder;
            using (Stream sm = await sf.OpenStreamForReadAsync(p))
            {
                using (StreamReader sr = new StreamReader(sm))
                {
                    string r = await sr.ReadToEndAsync();
                    return r;
                }
            }
        }

        async static internal Task ensureExists(string p)
        {
            StorageFolder sf = ApplicationData.Current.RoamingFolder;
            using (Stream st = await sf.OpenStreamForWriteAsync(p, CreationCollisionOption.OpenIfExists)) ;
        }
    }
}
#endif