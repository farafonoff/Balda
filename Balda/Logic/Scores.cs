using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Balda.NET.Logic;
using System.Collections.Generic;

namespace Balda.Logic
{
    public class Scores
    {
        public static List<string> lwtab = new List<string>();
        public static List<int> scorez = new List<int>();
        public static void addScore(Field fd,Dictionary<string,object> settings)
        {
            if (settings.ContainsKey("score_lwords"))
            {
                lwtab = (List<string>)settings["score_lwords"];
                scorez = (List<int>)settings["score_scorez"];
            }
            else
            {
                lwtab = new List<string>();
                scorez = new List<int>();
            }
            if (fd == null) return;
            if (fd.players == null) return;
            if (fd.players.Length < 1) return;
            if (fd.players[0] == null) return;
            if (fd.players[0].score == 0) return;
            if (fd.players[0].ai != null) return;
            int i = 0;
            while (i < 10 && i < scorez.Count)
            {
                if (scorez[i] < fd.players[0].score)
                {
                    break;
                }
                ++i;
            }
            if (i < scorez.Count)
            {
                scorez.Insert(i, fd.players[0].score);
                lwtab.Insert(i, fd.players[0].longestWord);
            }
            else if (scorez.Count<10)
            {
                scorez.Add(fd.players[0].score);
                lwtab.Add(fd.players[0].longestWord);
            }
            while (scorez.Count > 10)
            {
                scorez.RemoveAt(10);
                lwtab.RemoveAt(10);
            }
            settings["score_lwords"] = lwtab;
            settings["score_scorez"] = scorez;
        }
    }
}
