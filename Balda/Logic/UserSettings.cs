using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using Balda.PlatformDependent;

namespace Balda.Logic
{
    public class PlayerCountItem
    {
        public string name { get; set; }
        public int count { get; set; }
        public override string ToString()
        {
            return name;
        }
    }
    public class GameLanguage
    {
        public string name;
        public string dfile;
        public string ufile;
        public string alphabet;
        public string keyboard;
        public static GameLanguage English = new GameLanguage()
        {
            name = "English",
            dfile = "word_eng.txt",
            ufile = "NewWords_eng.txt",
            alphabet = "abcdefghijklmnopqrstuvwxyz",
            keyboard = "qwertyuiop;asdfghjkl;zxcvbnm"
        };
        public static GameLanguage Russian = new GameLanguage()
        {
            name = "Русский",
            dfile = "word_rus.txt",
            ufile = "NewWords_rus.txt",
            alphabet = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя",
            keyboard = "йцукенгшщзхъ;фывапролджэ;ячсмитьбюё"
        };
        public override string ToString()
        {
            return name;
        }

        internal static GameLanguage getDefault()
        {
            string clang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

            switch (clang.ToLower())
            {
                case "ru": return Russian; 
                case "en": return English; 
                default: return English; 
            }
        }

        internal int index(char p)
        {
            if (this == Russian)
            {
                return (p - 0x400);
            }
            else
                return p;
        }
    }
    public class UserSettings : INotifyPropertyChanged
    {
        public static GameLanguage[] supportedLangs = new GameLanguage[] { GameLanguage.English, GameLanguage.Russian };
        public GameLanguage[] SupportedLangs { get { return supportedLangs; } }
        private GameLanguage _lng;
        public GameLanguage Language
        {
            get { return _lng; }
            set { _lng = value; notify("Language"); }
        }
        public static PlayerCountItem[] userCountList = new PlayerCountItem[] { new PlayerCountItem() { name = AppResources.gm1, count = 1 }, new PlayerCountItem() { name = AppResources.gm2, count = 2 } };

        private string username = AppResources.plYou;
        public string Username { get { return username; } set { username = value; notify("Username"); } }

        private string user2name = AppResources.plFriend;
        public string User2name { get { return user2name; } set { user2name = value; notify("User2name"); } }

        private PlayerCountItem usercount = userCountList[0];
        public PlayerCountItem PlayersCount { get { return usercount; } set { usercount = value; notify("PlayersCount"); notify("isTwoPlayers"); notify("isComputer"); } }

        public PlayerCountItem[] PlayersCountList
        {
            get
            {
                return userCountList;
            }
        }

        public HardnessLevel[] PhoneLevelList
        {
            get
            {
                return HardnessLevel.HardnessLevels;
            }
        }

        HardnessLevel phLevel = HardnessLevel.Medium;

        public HardnessLevel PhoneLevel
        {
            get { return phLevel; }
            set { phLevel = value; notify("PhoneLevel"); }
        }

        public bool isTwoPlayers { get { return PlayersCount.count == 2; } }
        public bool isComputer { get { return PlayersCount.count == 1; } }

        private void notify(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        int[] _fsizes = new int[]{3,4,5,6,7};

        public int[] FieldSizes { get { return _fsizes; } }

        public int FieldSize { get; set; }
        public void load()
        {
            try
            {
                Username = (string)SettingsWrapper.ApplicationSettings["prop_username"];
                User2name = (string)SettingsWrapper.ApplicationSettings["prop_username2"];
                PhoneLevel = PhoneLevelList[(int)SettingsWrapper.ApplicationSettings["prop_level"]];
                PlayersCount = userCountList[(int)SettingsWrapper.ApplicationSettings["prop_count"]];
                Language = SupportedLangs[(int)SettingsWrapper.ApplicationSettings["prop_lang"]];
                FieldSize = (int)SettingsWrapper.ApplicationSettings["prop_size"];
                if (FieldSize == 0) FieldSize = 5;
            }
            catch (KeyNotFoundException ex)
            {
                save();
            }
        }

        public void save()
        {
            SettingsWrapper.ApplicationSettings["prop_username"] = Username;
            SettingsWrapper.ApplicationSettings["prop_username2"] = User2name;
            SettingsWrapper.ApplicationSettings["prop_level"] = PhoneLevelList.ToList().IndexOf(PhoneLevel);
            SettingsWrapper.ApplicationSettings["prop_count"] = userCountList.ToList().IndexOf(PlayersCount);
            SettingsWrapper.ApplicationSettings["prop_lang"] = SupportedLangs.ToList().IndexOf(Language);
            SettingsWrapper.ApplicationSettings["prop_size"] = FieldSize;
        }
        
        public UserSettings()
        {
            Language = GameLanguage.getDefault();
            load();
        }
    }
}
