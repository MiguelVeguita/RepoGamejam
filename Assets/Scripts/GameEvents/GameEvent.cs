using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Void Event", menuName = "Game Events/Void Event")]
public class GameEvent : ScriptableObject
{
    private List<GameEventListener> listeners;

    private void OnEnable()
    {
        listeners = new List<GameEventListener>();
    }

    private void OnDisable()
    {
        listeners.Clear();
    }
    private void EnsureListeners()
    {
        if (listeners == null)
            listeners = new List<GameEventListener>();
    }
    public void SetUp()
    {
        listeners = new List<GameEventListener>();
    }

    public void Raise()
    {
        foreach (GameEventListener sub in listeners)
        {
            sub.OnEventRaised();
        }
    }

    public void Register(GameEventListener newListener)
    {
        if (listeners.Contains(newListener)) return;

        listeners.Add(newListener);
    }

    public void Unregister(GameEventListener newListener)
    {
        if (!listeners.Contains(newListener)) return;

        listeners.Remove(newListener);
    }
}