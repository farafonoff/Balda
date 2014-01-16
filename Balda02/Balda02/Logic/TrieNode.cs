using System;
using System.Collections.Generic;
using System.Text;

namespace Balda.Logic
{
    class еёComparer : IComparer<string>
    {
        public static char rep(char c)
        {
            bool up = false;
            if (char.IsUpper(c))
            {
                up = true;
                c = char.ToLower(c);
            }
            char ret;
            if (c != 'ё')
            {
                ret = c;
            }
            else ret = 'е';
            if (up) ret = char.ToUpper(ret);
            return ret;
        }
        public static char irep(char c)
        {
            bool up = false;
            if (char.IsUpper(c))
            {
                up = true;
                c = char.ToLower(c);
            }
            char ret;
            if (c != 'е')
            {
                ret = c;
            }
            else ret = 'ё';
            if (up) ret = char.ToUpper(ret);
            return ret;
        }
        public static string rep(string s)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; ++i)
            {
                sb.Append(rep(s[i]));
            }
            return sb.ToString();
        }
        public int Compare(string x, string y)
        {
            return rep(x).CompareTo(rep(y));
        }
    }
    public class TrieNode
    {
        //char content;
        //List<TrieNode> children = new List<TrieNode>();
        public bool terminal = false;
        public Dictionary<char, TrieNode> children = new Dictionary<char, TrieNode>();
        TrieNode parent = null;
        char value;
        public void add(string s)
        {
            if (s != null && s.Length > 0)
            {
                TrieNode chld;// = children[s[0]];
                if (children.ContainsKey(s[0]))
                {
                    chld = children[s[0]];
                }
                else
                {
                    chld = new TrieNode();
                    children[s[0]] = chld;
                    chld.parent = this;
                    chld.value = s[0];
                }
                chld.add(s.Substring(1));
            }
            else
                terminal = true;
        }

        internal bool contains(string s)
        {
            if (s==null||s.Length == 0) return terminal;
            char rc = еёComparer.rep(s[0]);
            char cc = еёComparer.irep(s[0]);
            return (children.ContainsKey(rc)&&children[rc].contains(s.Substring(1))) || (children.ContainsKey(cc)&&children[cc].contains(s.Substring(1)));
        }

        public string word { get { return parent == null ? "" : parent.word + value.ToString()/*+(terminal?"!":"")*/; } }
    }
}
