using System;
using System.Linq;
using System.Collections.Generic;
using Balda.NET.Logic;
using Balda.PlatformDependent;

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
        public static GameLanguage lang = null;
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
        
        public static string ReadFileContents()
        {
            UserSettings uss = new UserSettings();
            if (uss.Language == lang) return "";
            lang = uss.Language;
            return ReReadFiles();
        }

        public static string ReReadFiles()
        {
            listRoot = new TrieNode();

            //this verse is loaded for the first time so fill it from the text file

            //addContents(FileOps.getFromResources());
            addContents(getResList("Data/" + lang.dfile));
            addContents(getFileList(lang.ufile));
            return string.Empty;
        }


        public static string[] getFileList(string fname)
        {
            FileOps.ensureExists(lang.ufile);
            string text = FileOps.getFromStorage(fname);
            string[] parts = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            return parts;
        }

        public static string[] getResList(string fname)
        {
            string text = FileOps.getFromResources(fname);
            string[] parts = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            return parts;
        }        

        private static void addContents(string[] parts)
        {
            
            foreach (string w in parts)
            {
                listRoot.add(w.Trim().ToUpper());
            }

        }
    }
}
