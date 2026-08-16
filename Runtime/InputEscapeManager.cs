using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Scripting.LifecycleManagement;

namespace CupkekGames.Input
{
  public static partial class InputEscapeManager
  {
    // State. Owned by ResetStaticState below rather than [AutoStaticsCleanup]: the
    // generated cleanup restores a field's initializer, which for InputEscapeEvent is
    // null — that would drop the built-in pop handler instead of re-installing it.
    [NoAutoStaticsCleanup]
    private static readonly List<InputEscapeEntry> _escapeList = new();
    [NoAutoStaticsCleanup]
    private static bool _isBlocked = false;
    // Events
    [NoAutoStaticsCleanup]
    public static Action InputEscapeEvent;

    static InputEscapeManager()
    {
      InputEscapeEvent += OnEscape;
    }

    // With "Enter Play Mode Without Domain Reload" these statics survive
    // between play sessions; the previous session's escape stack and
    // listeners would leak into the next one.
    [NoAutoStaticsCleanup]
    private static readonly DelegateAutoCleanup _autoCleanup =
      DelegateAutoCleanup.CreateForPlayMode(ResetStaticState, "CupkekGames.Input.InputEscapeManager");

    private static void ResetStaticState()
    {
      _escapeList.Clear();
      _isBlocked = false;
      InputEscapeEvent = OnEscape; // drop stale listeners, keep the built-in pop handler
    }

    public static void OnEscape()
    {
      Pop();
    }

    public static void Push(Action action, Guid? key = null, int insert = -1)
    {
      InputEscapeEntry entry;
      if (key.HasValue)
      {
        entry = new InputEscapeEntry(key.Value, action);
      }
      else
      {
        entry = new InputEscapeEntry(Guid.NewGuid(), action);
      }

      if (GetEntry(entry.Key) != -1)
      {
        // key already exists, do not push again
        return;
      }

      if (insert >= 0)
      {
        _escapeList.Insert(insert, entry);
      }
      else
      {
        _escapeList.Add(entry);
      }
    }

    public static void Pop() => PopInternal(null, true);

    public static void Pop(Guid key) => PopInternal(key, true);

    public static void PopWithoutExecute(Guid key) => PopInternal(key, false);

    private static void PopInternal(Guid? key, bool execute)
    {
      if (_escapeList.Count == 0)
      {
        return;
      }

      if (execute && _isBlocked)
      {
        return;
      }

      int index;

      if (key.HasValue)
      {
        index = GetEntry(key.Value);
      }
      else
      {
        index = _escapeList.Count - 1;
      }

      if (index != -1)
      {
        InputEscapeEntry entry = _escapeList[index];
        _escapeList.RemoveAt(index);

        if (execute)
        {
          entry.Action.Invoke();
        }
      }
    }

    public static void SetBlocked(bool value)
    {
      _isBlocked = value;
    }

    private static int GetEntry(Guid key)
    {
      return _escapeList.FindIndex(e => e.Key == key);
    }
  }
}
