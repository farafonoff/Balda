using System;

namespace Balda.Logic
{
    public class GameException : Exception
    {
        public string title;
        public string text;
        public GameException(string s,string userAdvice)
        {
            title = s;
            text = userAdvice;
        }
    }
}
