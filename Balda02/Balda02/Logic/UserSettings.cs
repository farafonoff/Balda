using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using Balda.PlatformDependent;
#if NETFX_CORE
using Balda02;
using Balda02.PlatformDependent;
using System.Threading.Tasks;
#endif

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
        public static GameLanguage English = new GameLanguage() { 
            name = "English", 
            dfile="word_eng.txt",
            ufile="NewWords_eng.txt",
            alphabet = "abcdefghijklmnopqrstuvwxyz",
            keyboard = "qwertyuiop;asdfghjkl;zxcvbnm"
        };
        public static GameLanguage Russian = new GameLanguage() { 
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
    }
    public class UserSettings : INotifyPropertyChanged
    {
        public static GameLanguage[] supportedLangs = new GameLanguage[] { GameLanguage.English, GameLanguage.Russian };
        public static PlayerCountItem[] userCountList = new PlayerCountItem[] { new PlayerCountItem() { name = ResLoader.getRes("gm1"), count = 1 }, new PlayerCountItem() { name = ResLoader.getRes("gm2"), count = 2 } };
        static UserSettings()
        { 
            //init all alala
            getDUsername();
        }

        public static async Task<string> getDUsername()
        {
            string t = await Windows.System.UserProfile.UserInformation.GetDisplayNameAsync();
            displayName = t;
            if (displayName.Length == 0)
            {
                displayName = ResLoader.getRes("unYou");
            }
            return t;
        }

        static string displayName = "";

        public GameLanguage[] SupportedLangs { get { return supportedLangs; } }
        private GameLanguage _lng;
        public GameLanguage Language
        {
            get { return _lng; }
            set { _lng = value; notify("Language"); }
        }

        private string username = null;
        public string Username { get {
            if (username == null||username=="")
            {
                username = displayName;
            }
            return username; 
        } set { username = value; notify("Username"); } }

        async public void fetchUsername()
        {
            if (displayName != "")
            {
                Username = displayName;
            }
            else
            {
                Username = await getDUsername();
            }
        }



        private string user2name = ResLoader.getRes("unFriend");
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

        public void load()
        {
            try
            {
                Username = (string)SettingsWrapper.ApplicationSettings["prop_username"];
                if (Username == null || Username == "")
                {
                    fetchUsername();
                }
                User2name = (string)SettingsWrapper.ApplicationSettings["prop_username2"];
                PhoneLevel = PhoneLevelList[(int)SettingsWrapper.ApplicationSettings["prop_level"]];
                PlayersCount = userCountList[(int)SettingsWrapper.ApplicationSettings["prop_count"]];
                Language = SupportedLangs[(int)SettingsWrapper.ApplicationSettings["prop_lang"]];
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
        }



        public UserSettings()
        {
            Language = GameLanguage.getDefault();
            load();
        }
    }
}
