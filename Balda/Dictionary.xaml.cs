using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Balda.Logic;
using Balda.PlatformDependent;
using Microsoft.Phone.Tasks;
using System.Text;

namespace Balda
{
    public partial class Dictionary : PhoneApplicationPage
    {
        public class CheckString
        {
            public string word {get;set;}
            public bool check {get;set;}
            public bool blacklisted { get; set; }
            public CheckString(string w)
            {
                if (w.EndsWith("!"))
                {
                    blacklisted = true;
                    check = true;
                    word = w.Substring(0, w.Length - 1);
                }
                else
                    word = w;
            }
        }
        public List<CheckString> gameWords { get; set; }
        public List<CheckString> dicWords { get; set; }
        public Dictionary()
        {
            InitializeComponent();
            loadContent();
        }

        class csc : IEqualityComparer<CheckString>
        {

            public bool Equals(CheckString x, CheckString y)
            {
                return x.word.Equals(y.word);
            }

            public int GetHashCode(CheckString obj)
            {
                return obj.word.GetHashCode();
            }
        }


        void loadContent()
        {
            DataContext = null;
            gameWords = MainPage.field.players[0].words.
                Union(MainPage.field.players[1].words).
                Where(s => s.Length != 0).OrderBy(s => s).
                OrderBy(s => s).
                Select(s => new CheckString(s)).
                Distinct(new csc()).
                ToList();
            gameWords.Insert(0,new CheckString(MainPage.field.initialWord));
            dicWords = WordList.getFileList(WordList.lang.ufile).
                Where(s => s.Length != 0).
                OrderBy(s => s).
                Select(s => new CheckString(s)).
                Distinct(new csc()).
                ToList();
            DataContext = this;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //I. clear user file and write normal words
            FileOps.clearFile(WordList.lang.ufile);
            foreach(var w in dicWords.Where(w => !w.check&&!w.blacklisted))
            {
                WordList.add(w.word);
            }
            //II. blacklist words
            foreach (var w in gameWords.Where(w => w.check))
            {
                WordList.add(w.word+"!");
            }
            //III. old blacklisted words
            foreach (var w in dicWords.Where(w => w.blacklisted&&w.check))
            {
                WordList.add(w.word + "!");
            }
            loadContent();
            WordList.ReReadFiles();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            EmailComposeTask emailcomposer = new EmailComposeTask();
            emailcomposer.To = "farafonoff@gmail.com";
            emailcomposer.Subject = "[WordList]";
            StringBuilder bsb = new StringBuilder();
            var words = WordList.getFileList(WordList.lang.ufile).
                Where(s => s.Length != 0).
                OrderBy(s => s);
            foreach(var word in words)
            {
                bsb.AppendLine(word);
            }
            emailcomposer.Body = bsb.ToString();
            emailcomposer.Show();
        }
    }
}