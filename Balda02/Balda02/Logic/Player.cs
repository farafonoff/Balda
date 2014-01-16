using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Balda.Logic
{
    public enum PlayerType
    {
        Human,
        AI
    }
    public class Player : INotifyPropertyChanged
    {
        protected ObservableCollection<string> _words = new ObservableCollection<string>();
        public static TrieNode usedWords = new TrieNode();
        public Player(PlayerType type, string name)
        {
            this.type = type;
            this.name = name;
            if (type == PlayerType.AI)
            {
                ai = new AI();
            }
        }
        public bool skip;
        public string name { get; set; }
        public int score { get; set; }
        public ObservableCollection<string> words { get { return _words; } }
        public PlayerType type { get; set; }
        public AI ai;
        public virtual void addWord(string w)
        {
            if (w == null || w.Trim() == "") return;
            usedWords.add(w);
            score += w.Length;
            _words.Insert(0, w);
            NotifyPropertyChanged("score");
        }

        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
