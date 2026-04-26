using System;
using UnityEngine;

namespace CupkekGames.Core
{
  public class EscapeAction
  {
    private Guid _escapeKey;
    public Guid EscapeKey => _escapeKey;
    public event Action OnEscape;

    public EscapeAction(GameObject parent, bool push)
    {
      _escapeKey = Guid.NewGuid();

      EscapeActionListener listener = parent.AddComponent<EscapeActionListener>();
      listener.EscapeKey = _escapeKey;

      Push();
    }

    public void Push(int insert = -1)
    {
      InputEscapeManager.Push(OnEscapeInner, _escapeKey, insert);
    }

    private void OnEscapeInner()
    {
      OnEscape?.Invoke();
    }

    public void Escape()
    {
      InputEscapeManager.Pop(_escapeKey);
    }

    public void Dispose()
    {
      InputEscapeManager.PopWithoutExecute(EscapeKey);
    }
  }
}