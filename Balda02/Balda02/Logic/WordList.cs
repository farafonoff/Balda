using System;
using System.Linq;
using System.Collections.Generic;
using Balda.NET.Logic;
using Balda.PlatformDependent;
using System.Threading.Tasks;

namespace Balda.Logic
{
    public class WordList
    {
        public static TrieNode listRoot = new TrieNode();
        public static bool contains(string w)
        {
            string sw = w.ToUpper();
            if (listRoot.contains(sw)) return true;
            else return false;
        }
        static GameLanguage lang = null;
        public static void add(string w)
        {
            string sw = w.ToUpper();
            listRoot.add(sw);
            FileOps.appendLine(lang.ufile, w);
        }
        public static string getWordOfLength(int length)
        {
            return findWordLenRec(listRoot, length);
        }

        static string findWordLenRec(TrieNode r, int len)
        {
            if (len == 0 && r.terminal) return r.word;
            var letters = r.children.Keys.ToList();
            letters.Shuffle();
            foreach (char l in letters)
            {
                string tr = findWordLenRec(r.children[l], len - 1);
                if (tr != null) return tr;
            }
            return null;
        }
        
        async public static Task ReadFileContents()
        {
            UserSettings uss = new UserSettings();
            if (uss.Language == lang) return;
            lang = uss.Language;
            listRoot = new TrieNode();

            //this verse is loaded for the first time so fill it from the text file

            addContents(await FileOps.getFromResources("Data/" + lang.dfile));
            await FileOps.ensureExists(lang.ufile);
            addContents(await FileOps.getFromStorage(lang.ufile));
            return;

        }

        

        private static void addContents(string text)
        {
            string[] parts = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string w in parts)
            {
                listRoot.add(w.Trim().ToUpper());
            }

        }
    }
}
