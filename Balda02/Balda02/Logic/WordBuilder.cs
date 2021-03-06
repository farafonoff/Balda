﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Balda.Logic;
#if NETFX_CORE
using Balda02;
using Balda02.PlatformDependent;
#endif

namespace Balda.NET.Logic
{
    public class WordBuilder
    {
        StringBuilder word = new StringBuilder();
        public List<LPosition> path = new List<LPosition>();
        bool newUsed;
        Field _fld;
        public WordBuilder(Field grid)
        {
            _fld = grid;
        }

        private void add(Letter letter)
        {
            word.Append(letter.letter);
            if (letter.state == LetterState.SelectedNew) newUsed = true;
            //throw new NotImplementedException();
        }
        LPosition lastpos = null;
        internal void add(LPosition pos)
        {
            if (lastpos == null || lastpos.isNear(pos))
            {
                var letter = _fld[pos];
                lastpos = pos;
                add(letter);
                path.Add(pos);
            }
            else throw new GameException(ResLoader.getRes("wbNearTitle"), ResLoader.getRes("wbNearText"));
        }
        internal string complete()
        {
            if (newUsed == true)
            {
                return word.ToString();
            }
            else
                throw new GameException(ResLoader.getRes("wbUnusedNewTitle"), ResLoader.getRes("wbUnusedNewText"));
        }
    }
}
