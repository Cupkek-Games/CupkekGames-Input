using System;
using UnityEngine;

namespace CupkekGames.Core
{
  public class EscapeActionListener : MonoBehaviour
  {
    public Guid EscapeKey;

    protected virtual void OnDisable()
    {
      InputEscapeManager.PopWithoutExecute(EscapeKey);
    }
  }
}