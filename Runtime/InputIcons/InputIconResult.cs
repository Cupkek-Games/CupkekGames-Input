
using System;
using UnityEngine;

namespace CupkekGames.Core
{
    [Serializable]
    public class InputIconResult
    {
        [SerializeField] public Sprite Icon = null;
        [SerializeField] public bool Slice = false;
        [SerializeField] public bool Square = false;
    }
}
