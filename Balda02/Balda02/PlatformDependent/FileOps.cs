#if SILVERLIGHT
using System;
using System.Windows;
using System.IO;
using System.IO.IsolatedStorage;

namespace Balda.PlatformDependent
{
    public class FileOps
    {
        static IsolatedStorageFile myFile = IsolatedStorageFile.GetUserStoreForApplication();
        public static string getFromResources(string fname)
        {
            var ResrouceStream = Application.GetResourceStream(new Uri(fname, UriKind.Relative));
            string text = "";

            if (ResrouceStream != null)
            {

                using (Stream myFileStream = ResrouceStream.Stream)
                {
                    if (myFileStream.CanRead)
                    {
                        StreamReader myStreamReader = new StreamReader(myFileStream);
                        text = myStreamReader.ReadToEnd();
                    }

                }

            }
            return text;
            
        }
        public static string getFromStorage(string fname)
        {
            StreamReader reader = new StreamReader(new IsolatedStorageFileStream(fname, FileMode.Open, myFile));
            string text = reader.ReadToEnd();
            reader.Close();
            return text;
        }

        public static void ensureExists(string fname)
        {
            if (!myFile.FileExists(fname))
            {
                IsolatedStorageFileStream dataFile = myFile.CreateFile(fname);
                dataFile.Close();
            }

        }
        public static void appendLine(string fname, string line)
        {
            StreamWriter wr = new StreamWriter(new IsolatedStorageFileStream(fname, FileMode.Append, myFile));
            wr.WriteLine(line); //Wrting to the file
            wr.Close();
        }

    }
}
#endif