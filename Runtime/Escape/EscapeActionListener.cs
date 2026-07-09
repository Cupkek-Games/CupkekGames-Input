using System;
using UnityEngine;

namespace CupkekGames.Input
{
  public class EscapeActionListener : MonoBehaviour
  {
    [NonSerialized] public Guid EscapeKey;

    protected virtual void OnDisable()
    {
      InputEscapeManager.PopWithoutExecute(EscapeKey);
    }
  }
}