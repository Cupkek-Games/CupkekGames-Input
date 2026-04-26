using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

namespace CupkekGames.Core
{
    public struct InputEscapeEntry
    {
        public Guid Key;
        public Action Action;
        public InputEscapeEntry(Guid key, Action action)
        {
            Key = key;
            Action = action;
        }
    }
}
