using System;
using Balda.NET.Logic;
using System.Collections.Generic;
using System.Linq;
#if NETFX_CORE
using Balda02;
using Balda02.PlatformDependent;
#endif

namespace Balda.Logic
{
    public class HardnessLevel
    {
        public double deviationLevel;
        public int meanLength;
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
        public HardnessLevel(string n,int ml, double dv)
        {
            meanLength = ml;
            deviationLevel = dv;
            Name = n;
        }
        public static HardnessLevel Easy = new HardnessLevel(ResLoader.getRes("hdEasy"),3, 0.9);
        public static HardnessLevel Medium = new HardnessLevel(ResLoader.getRes("hdMedium"),4, 0.6);
        public static HardnessLevel Hard = new HardnessLevel(ResLoader.getRes("hdHard"),4, 0.3);
        public static HardnessLevel Full = new HardnessLevel(ResLoader.getRes("hdFull"), 99, 0);
        public static HardnessLevel[] HardnessLevels = new HardnessLevel[] { Easy, Medium, Hard, Full };
        public BruteResult Better(BruteResult b1, BruteResult b2)
        {
            if (b1 == null)
                return b2;
            if (b2 == null)
                return b1;
            if (b1.value < b2.value)
            {
                BruteResult br = b1;
                b1 = b2;
                b2 = br;
            }
            if (b2.value > meanLength) return b2;
            if (b1.value > meanLength)
            {
                if (b1.rnd > deviationLevel)
                    return b1;
                else 
                    if (b2.rnd < deviationLevel / 2)
                {
                    return b2;
                }
                else
                    return b1;
                    
            }
            return b1;
        }
        static double iabs(int v)
        {
            if (v < 0) return -v; else return v;
        }
    }
    public class BruteResult
    {
        static Random rng = new Random((int)(new DateTime().Ticks) % 1000000000);
        public double rnd;
        public int value;
        public string word;
        public List<LPosition> path;
        public LPosition addedPosition;
        public char addedChar;
        public BruteResult()
        {
            rnd = rng.NextDouble();
        }
    }
    public class AI
    {
        public HardnessLevel hardness = HardnessLevel.Easy;
        public BruteResult makeTurn(Field f)
        {
            List<LPosition> roots = new List<LPosition>();
            for (int i = 0; i < Field.FieldSize; ++i)
            {
                for (int j = 0; j < Field.FieldSize; ++j)
                {
                    LPosition lp = new LPosition(i, j);
                    if (f[lp].state == LetterState.Empty)
                    {
                        var around = lp.around();
                        if (around.Where(p => f[p].state != LetterState.Empty).Count() > 0)
                        {
                            roots.Add(lp);
                        }
                    }
                    else
                    {
                        roots.Add(lp);
                    }
                }
            }
            BruteResult total = null;
            foreach (LPosition root in roots)
            {
                TrieNode xroot = WordList.listRoot;
                BruteResult result = null;
                if (f[root].state != LetterState.Empty)
                {
                    if (xroot.children.ContainsKey(f[root].letter))
                    {
                        xroot = xroot.children[f[root].letter];
                    }
                    else continue;
                    result = bruteOld(f, root, false, xroot);
                }
                else
                {
                    result = bruteNew(f, root, false, xroot);
                }
                total = hardness.Better(total, result);
                if (result != null)
                    System.Diagnostics.Debug.WriteLine("##########" + result.word);
            }
            System.Diagnostics.Debug.WriteLine("=========");
            System.Diagnostics.Debug.WriteLine(total == null ? "Not found" : total.word);
            return total;
        }
        bool[,] visited = new bool[Field.FieldSize, Field.FieldSize];
        private BruteResult bruteNew(Field f, LPosition root, bool newLetterUsed, TrieNode wpos)
        {
            BruteResult result = null;
            visited[root.x, root.y] = true;
            foreach (var v in wpos.children)
            {
                //System.Diagnostics.Debug.WriteLine("=======>" + v.Key);
                BruteResult current = bruteOld(f, root, true, v.Value);
                if (current != null)
                {
                    current.word = v.Key + current.word;
                    current.addedChar = v.Key;
                    current.addedPosition = root;
                }
                result = hardness.Better(result, current);
            }
            if (result != null)
            {
                //result.path.Add(root);
                //f.highlight(root, LetterState.SelectedNew);
            }
            visited[root.x, root.y] = false;
            return result;
        }
        private BruteResult bruteOld(Field f, LPosition root, bool newLetterUsed, TrieNode wpos)
        {
            visited[root.x, root.y] = true;
            //System.Diagnostics.Debug.WriteLine(wpos.word);
            BruteResult result = null;
            if (wpos.terminal && newLetterUsed && !Player.usedWords.contains(wpos.word))
            {
                result = new BruteResult();
                result.path = new List<LPosition>();
                result.value = wpos.word.Length;
            }
            var around = root.around().Where(np => !visited[np.x, np.y]);
            if (newLetterUsed)
                around = around.Where(p => f[p].state != LetterState.Empty).ToList<LPosition>();
            foreach (var np in around)
            {
                if (f[np].state == LetterState.Empty)
                {
                    if (newLetterUsed) throw new ArgumentException("letter can't be empty here");
                    if (wpos.children.Count > 0)
                    {
                        result = hardness.Better(result, bruteNew(f, np, newLetterUsed, wpos));
                    }
                }
                else
                    if (wpos.children.ContainsKey(f[np].letter))
                    {
                        result = hardness.Better(result, bruteOld(f, np, newLetterUsed, wpos.children[f[np].letter]));
                    }
            }
            if (result != null)
            {
                result.path.Add(root);
                if (f[root].state != LetterState.Empty)
                {
                    result.word = f[root].letter + result.word;
                    //f.highlight(root, LetterState.Selected);
                }
            }
            visited[root.x, root.y] = false;
            return result;
        }
    }
}
