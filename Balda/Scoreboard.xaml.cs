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
using System.Collections.ObjectModel;

namespace Balda
{
    public class ScoreItem
    {
        public string lword {get;set;}
        public int score {get;set;}
    }
    public partial class Scoreboard : PhoneApplicationPage
    {
        public Scoreboard()
        {
            InitializeComponent();
            itemz = new ObservableCollection<ScoreItem>();
            DataContext = this;
        }
        public ObservableCollection<ScoreItem> itemz { get; set; }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            itemz.Clear();
            for (int i = 0; i < Scores.lwtab.Count; ++i)
            {
                if (Scores.scorez[i] > 0)
                {
                    itemz.Add(new ScoreItem() { lword = Scores.lwtab[i], score = Scores.scorez[i] });
                }
            }
        }
    }
}