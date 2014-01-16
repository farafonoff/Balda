using System;
using System.Collections.Generic;
using System.Linq;
using Balda.Logic;
using System.ComponentModel;
using System.Windows;
using System.Threading;
using System.Runtime.Serialization;
using Balda.PlatformDependent;
#if SILVERLIGHT
using System.Windows.Threading;
#else
using Windows.UI.Xaml;
using Balda02;
using Balda02.PlatformDependent;
#endif

namespace Balda.NET.Logic
{
    public enum LetterState
    {
        Old,
        New,
        Selected,
        SelectedNew,
        Empty
    }
    public enum FieldState
    {
        AddingLetter,
        ComposingWord,
        NotStarted,
        OtherTurn
    }
    public class Letter
    {
        public LetterState state;
        private char _ll;
        public char letter
        {
            get
            {
                return _ll;
            }
            set
            {
                _ll = Char.ToUpper(value);
            }
        }
        public Letter(char ll)
        {
            letter = ll;
            state = LetterState.New;
        }
        public Letter(char ll, LetterState ls)
        {
            letter = ll;
            state = ls;
        }
    }
    public enum CompleteResult
    {
        OK,
        UNKNOWN,
        USED
    }
    public class Field : INotifyPropertyChanged
    {
        private FieldState _stte = FieldState.NotStarted;
        public FieldState state
        {
            get { return _stte; }
            set { _stte = value; notify("state"); }
        }
        public LPosition clickPosition;
        public const int FieldSize = 5;
        private Letter[,] _lets = new Letter[FieldSize, FieldSize];
        private Player[] _playerz;
        public Player[] players { get { return _playerz; } }
        private Player _currentPlayer = null;
        MyBgWorker aiWorker = new MyBgWorker();
        //BackgroundWorker aiWorker = new BackgroundWorker();
        public Player currentPlayer
        {
            get
            {
                return _currentPlayer;
            }
            set
            {
                _currentPlayer = value;
                notify("currentPlayer");
            }
        }
        private bool _gmOver = false;
        public bool gameOver { get { return _gmOver; } set { _gmOver = value; notify("gameOver"); } }
        private Player _wPlayer;
        private Player _lPlayer;
        private bool _drwGame;
        private bool _wnGame;
        public Player WinnerPlayer { get { return _wPlayer; } set { _wPlayer = value; notify("WinnerPlayer"); } }
        public Player LooserPlayer { get { return _lPlayer; } set { _lPlayer = value; notify("LooserPlayer"); } }
        public bool DrawGame { get { return _drwGame; } set { _drwGame = value; notify("DrawGame"); } }
        public bool WinGame { get { return _wnGame; } set { _wnGame = value; notify("WinGame"); } }
        public Letter this[int x, int y]
        {
            get
            {
                Letter res = _lets[x, y];
                if (res == null)
                {
                    _lets[x, y] = new Letter(' ', LetterState.Empty);
                    return _lets[x, y];
                }
                else return res;
            }
            set
            {
                _lets[x, y] = value;
                notifyUpdated(x, y);
            }
        }
        public Letter this[LPosition pos]
        {
            get
            {
                return this[pos.x, pos.y];
            }
            set
            {
                this[pos.x, pos.y] = value;
            }
        }

        DispatcherTimer delayedSwitchTurn;
        string initialWord;
        public Field()
            : this(new string('A', FieldSize))
        {
        }
        public Field(string iword)
        {
            if (iword.Length != FieldSize)
            {
                throw new ArgumentException("Initial word length must be " + FieldSize);
            }
            iword = iword.ToUpper();
            initialWord = iword;
            for (int i = 0; i < FieldSize; ++i)
            {
                this[i, FieldSize / 2] = new Letter(iword[i], LetterState.Old);
            }
            Player.usedWords.add(iword);
            aiWorker.work = aiWorker_DoWork;
            //aiWorker.DoWork += new DoWorkEventHandler(aiWorker_DoWork);
            aiWorker.onComplete += aiWorker_RunWorkerCompleted;
            delayedSwitchTurn = new DispatcherTimer();
#if SILVERLIGHT
            delayedSwitchTurn.Tick += new EventHandler(delayedSwitchTurn_Tick);
#else
            delayedSwitchTurn.Tick +=delayedSwitchTurn_Tick;
#endif
            setupPlayers();
        }

        private void delayedSwitchTurn_Tick(object sender, object e)
        {
            if (state == FieldState.OtherTurn)
            {
                UiThreadRunner.runOnUiThread(() =>
                {
                    if (state == FieldState.OtherTurn)
                    {
                        switchTurn();
                    }
                }
                    );
            }
            delayedSwitchTurn.Stop();
        }

        public bool demo = false;

        void setupPlayers()
        {
            UserSettings us = new UserSettings();
            _playerz = new Player[2] {
                new Player(PlayerType.Human,us.Username),
                us.PlayersCount.count==2?new Player(PlayerType.Human,us.User2name):new Player(PlayerType.AI,us.PhoneLevel.Name+" "+ResLoader.getRes("tPhone"))
            };
            if (us.PlayersCount.count == 1)
            {
                _playerz[1].ai.hardness = us.PhoneLevel;
            }
            demo = false;
            notify("players");
        }

        public void setupPlayersDemo()
        {
            UserSettings us = new UserSettings();
            HardnessLevel l1 = HardnessLevel.Medium;
            HardnessLevel l2 = HardnessLevel.Full;
            _playerz = new Player[2] {
                new Player(PlayerType.AI,l1.Name),
                new Player(PlayerType.AI,l2.Name)
            };
            _playerz[0].ai.hardness = l1;
            _playerz[1].ai.hardness = l2;
            demo = true;
            notify("players");
        }

        int aiWordDisplayTime { get { int delay = aiWord.Length * 1000; if (demo) delay /= 2; return delay; } }
        private bool _aiwadded = false;
        public bool aiWordAdded { get { return _aiwadded; } set { _aiwadded = value; notify("aiWordAdded"); } }
        private string _aiword = "";
        public string aiWord { get { return _aiword; } set { _aiword = value; notify("aiWord"); } }

        public List<LPosition> aiWordPath = null;
        void aiWorker_RunWorkerCompleted(object sender, BgWorkerEventArgs e)
        {
            BruteResult br = (BruteResult)e.Result;
            if (br != null)
            {
                UiThreadRunner.runOnUiThread(() =>
                {
                    foreach (var pos in br.path)
                    {
                        this[pos] = new Letter(this[pos].letter, LetterState.Selected);
                    }
                    aiWordPath = br.path;
                    this[br.addedPosition] = new Letter(br.addedChar, LetterState.SelectedNew);
                    currentPlayer.addWord(br.word); removeSkips();
                    aiWordAdded = true;
                    aiWord = br.word;
                    delaySwitch(aiWordDisplayTime);
                });
            }
            else
            {
                currentPlayer.skip = true;
                delaySwitch(100);
            }
        }



        void delaySwitch(long ms)
        {
            delayedSwitchTurn.Interval = TimeSpan.FromMilliseconds(ms);
            delayedSwitchTurn.Start();
            //delayedSwitchTurn.Change(ms, System.Threading.Timeout.Infinite);
        }

        public void finishDelaySwitch()
        {
            if (!aiWorker.IsBusy)
            {
                //delayedSwitchTurn.Interval = TimeSpan.FromMilliseconds(1);
                delayedSwitchTurn.Stop();
                if (state == FieldState.OtherTurn)
                {
                    delayedSwitchTurn_Tick(this, null);
                }
                //delayedSwitchTurn.Change(1, System.Threading.Timeout.Infinite);
            }
        }

        void aiWorker_DoWork(object sender, BgWorkerEventArgs e)
        {
            AI ai = (AI)e.Argument;
            if (ai == null) return;
            BruteResult br = ai.makeTurn(this);
            e.Result = br;
        }
        public WordBuilder _builder;
        public WordBuilder getWordBuilder()
        {
            _builder = new WordBuilder(this);
            notifyNewBuilder();
            return _builder;
        }
        public delegate void UpdatedHandler(Field sender, FieldUpdatedEventArgs args);
        public event UpdatedHandler updated;
        void notifyNewBuilder()
        {

        }
        void notifyUpdated(int x, int y)
        {
            if (updated != null)
            {
                FieldUpdatedEventArgs fuea = new FieldUpdatedEventArgs();
                LPosition pos = new LPosition(x, y);
                fuea.lp = pos;
                updated(this, fuea);
            }
        }

        internal void select(LPosition lPosition)
        {

            clickPosition = lPosition;
            switch (this[lPosition].state)
            {
                case LetterState.Empty:
                    if (state == FieldState.AddingLetter)
                    {
                        if (lPosition.around().Where(let => this[let].state != LetterState.Empty).Count() == 0)
                            throw new GameException(ResLoader.getRes("fcfNearTitle"), ResLoader.getRes("fcfNearText"));
                        this[lPosition].state = LetterState.New;
                    }
                    break;
                case LetterState.New:
                    if (state == FieldState.ComposingWord)
                    {
                        this[lPosition].state = LetterState.SelectedNew;
                        _builder.add(lPosition);
                    }
                    break;
                case LetterState.Old:
                    if (state == FieldState.ComposingWord)
                    {
                        this[lPosition].state = LetterState.Selected;
                        _builder.add(lPosition);
                    }
                    break;
                case LetterState.Selected:
                case LetterState.SelectedNew:
                    break;
                default:
                    throw new NotImplementedException();
            }
            notifyUpdated(lPosition.x, lPosition.y);
        }

        public void putKey(string p)
        {
            if (clickPosition == null) throw new ArgumentNullException("position not selected");
            if (state != FieldState.AddingLetter) throw new Exception("Invalid field state");
            this[clickPosition] = new Letter(p[0], LetterState.New);
            clickPosition = null;
            state = FieldState.ComposingWord;
            _builder = new WordBuilder(this);
        }
        public void start()
        {
            currentPlayer = players[0];
            state = FieldState.AddingLetter;
            makeAiTurn(currentPlayer.ai);
        }

        void makeAiTurn(AI ai)
        {
            if (ai == null) return;
            state = FieldState.OtherTurn;
            aiWorker.Run(ai);
            //aiWorker.RunWorkerAsync(ai);
        }
        public void removeSkips()
        {
            players[0].skip = false;
            players[1].skip = false;
        }

        public string lastWord;
        public CompleteResult complete()
        {
            if (state == FieldState.ComposingWord && _builder != null)
            {
                string word = _builder.complete();
                if (Player.usedWords.contains(word)) return CompleteResult.USED;
                if (WordList.contains(word))
                {
                    currentPlayer.addWord(word); removeSkips();
                    return CompleteResult.OK;
                }
                else
                {
                    lastWord = word;
                    return CompleteResult.UNKNOWN;
                }
            }
            throw new ArgumentException("Invalid game state");
        }
        bool clearField(bool commit)
        {
            bool modified = false;
            int free = 0;
            _builder = null;
            for (int i = 0; i < Field.FieldSize; ++i)
            {
                for (int j = 0; j < Field.FieldSize; ++j)
                {
                    var cell = this[i, j];
                    if (cell.state == LetterState.Selected)
                    {
                        this[i, j] = new Letter(cell.letter, LetterState.Old);
                        modified = true;
                    }
                    if (cell.state == LetterState.New)
                    {
                        this[i, j] = new Letter(' ', LetterState.Empty);
                        modified = true;
                    }
                    if (cell.state == LetterState.SelectedNew)
                    {
                        if (commit)
                        {
                            this[i, j] = new Letter(cell.letter, LetterState.Old);
                        }
                        else
                        {
                            this[i, j] = new Letter(' ', LetterState.Empty);
                        }
                        modified = true;
                    }
                    if (cell.state == LetterState.Empty)
                    {
                        ++free;
                    }
                }
            }
            if (free == 0)
            {
                players[0].skip = true;
                players[1].skip = true;
            }
            if (currentPlayer.type == PlayerType.AI)
            {
                state = FieldState.OtherTurn;
            }
            else
            {
                state = FieldState.AddingLetter;
            }
            return modified;
        }
        public void switchTurn()
        {
            if (state == FieldState.NotStarted) return;
            if (currentPlayer == players[0])
            {
                currentPlayer = players[1];
            }
            else
                currentPlayer = players[0];
            clearField(true);
            aiWordAdded = false;
            if (players[0].skip && players[1].skip)
            {
                _fld_onGameOver();
                state = FieldState.NotStarted;
                gameOver = true;
            }
            else
                makeAiTurn(currentPlayer.ai);
        }

        void _fld_onGameOver()
        {
            Field _fld = this;
            if (_fld.players[0].score > _fld.players[1].score)
            {
                WinnerPlayer = _fld.players[0];
                LooserPlayer = _fld.players[1];
            }
            else if (_fld.players[0].score < _fld.players[1].score)
            {
                WinnerPlayer = _fld.players[1];
                LooserPlayer = _fld.players[0];
            }

            if (WinnerPlayer == null)
            {
                DrawGame = true;
            }
            else
                WinGame = true;
        }


        internal bool cancel()
        {
            if (state == FieldState.OtherTurn)
            {
                finishDelaySwitch();
                return true;
            }
            else
                return clearField(false);
        }

        internal void highlight(LPosition root, LetterState letterState)
        {
            if (onHighlight != null)
            {
                onHighlight(this, new HighLightEventArgs(root, letterState));
            }
        }
        public class HighLightEventArgs : EventArgs
        {
            public HighLightEventArgs(LPosition r, LetterState s)
            {
                root = r;
                state = s;
            }
            public LPosition root;
            public LetterState state;
        }
        public delegate void HighLightEventHandler(Field sender, HighLightEventArgs args);
        public event HighLightEventHandler onHighlight;

        internal void skip()
        {
            if (state == FieldState.OtherTurn)
            {
                finishDelaySwitch();
                return;
            }
            if (state == FieldState.AddingLetter || state == FieldState.ComposingWord)
            {
                currentPlayer.skip = true;
                switchTurn();
            }
        }

        internal void SaveState(Dictionary<string,object> settings)
        {
            Dictionary<string, object> pstate = new Dictionary<string, object>();
            if (aiWordAdded && state == FieldState.OtherTurn) switchTurn();
            List<char> lcells = new List<char>();
            List<int> scells = new List<int>();
            for (int i = 0; i < FieldSize; ++i)
            {
                for (int j = 0; j < FieldSize; ++j)
                {
                    lcells.Add(_lets[i, j].letter);
                    scells.Add((int)_lets[i, j].state);
                }
            }
            pstate["chars"] = lcells.Flatten();
            pstate["states"] = scells.Flatten();
            pstate["state"] = (int)this.state;
            if (currentPlayer == players[0])
            {
                pstate["player"] = 0;
            }
            else
                pstate["player"] = 1;
            pstate["player0_words"] = string.Join("|", players[0].words.Reverse());
            pstate["player1_words"] = string.Join("|", players[1].words.Reverse());

            pstate["player0_skip"] = players[0].skip;
            pstate["player1_skip"] = players[1].skip;
            pstate["initialWord"] = initialWord;

            settings["lastState"] = pstate;
        }

        internal void loadState(Dictionary<string,object> settings)
        {
            try
            {

                Dictionary<string, object> pstate = (Dictionary<string, object>)settings["lastState"];
                List<char> lcells = new List<char>();
                List<int> scells = new List<int>();
                lcells.ToChars((string)pstate["chars"]);
                scells.ToInts((string)pstate["states"]);
                int il = 0;
                for (int i = 0; i < FieldSize; ++i)
                {
                    for (int j = 0; j < FieldSize; ++j)
                    {
                        this[i, j] = new Letter(lcells[il], (LetterState)scells[il]);
                        ++il;
                    }
                }
                //List<string> twords = new List<string>();
                string[] twords = ((string)pstate["player0_words"]).Split('|');
                foreach (string tw in twords) { players[0].addWord(tw); }
                twords = ((string)pstate["player1_words"]).Split('|');
                foreach (string tw in twords) { players[1].addWord(tw); }
                players[0].skip = (bool)pstate["player0_skip"];
                players[1].skip = (bool)pstate["player1_skip"];
                this.state = (FieldState)pstate["state"];
                initialWord = (string)pstate["initialWord"];
                Player.usedWords.add(initialWord);
            }
            catch (Exception ex)
            {
                //не удалось
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    //throw ex;
                }
            }
        }

        private void notify(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


    }

    public class FieldUpdatedEventArgs : EventArgs
    {
        public LPosition lp;
    }
    [DataContract(Name = "LPosition", Namespace = "farafonoff@gmail.com")]
    public class LPosition
    {
        [DataMember()]
        public int x { get; set; }
        [DataMember()]
        public int y { get; set; }
        public LPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        internal bool isNear(LPosition pos)
        {
            int dx = abz(pos.x - x);
            int dy = abz(pos.y - y);
            if ((dx == 1 || dy == 1) && (dx + dy == 1)) return true;
            return false;
        }
        static int abz(int v)
        {
            if (v < 0) return -v; else return v;

        }

        public List<LPosition> around()
        {
            return around(this);
        }
        public static List<LPosition> around(LPosition pos)
        {
            List<LPosition> result = new List<LPosition>();
            if (pos.x > 0) result.Add(new LPosition(pos.x - 1, pos.y));
            if (pos.x < Field.FieldSize - 1) result.Add(new LPosition(pos.x + 1, pos.y));
            if (pos.y > 0) result.Add(new LPosition(pos.x, pos.y - 1));
            if (pos.y < Field.FieldSize - 1) result.Add(new LPosition(pos.x, pos.y + 1));
            return result;
        }
    }

}
