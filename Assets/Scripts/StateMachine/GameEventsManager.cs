using System.Collections.Generic;
using UnityEngine;

public class GameEventsManager : MonoSingleton<GameEventsManager>
{
    public delegate void GameEventListener(GameEventData data);

    private Dictionary<string, GameEventListener> eventHandlers;
    private GameEventListener globalEventHandler;

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    protected void Start()
    {
        GameEventData appStartData = new()
        {
            eventName = GameEvents.AppStart
        };
        PostEvent(appStartData);
    }

    private void Init()
    {
        if (eventHandlers == null || eventHandlers.Count == 0)
        {
            eventHandlers = new Dictionary<string, GameEventListener>();
        }
    }

    public void PostEvent(GameEventData context = null)
    {
        if (context == null)
        {
            Debug.Log("null event posted");
            return;
        }
        if (eventHandlers.ContainsKey(context.eventName))
        {
            eventHandlers[context.eventName]?.Invoke(context);
        }
        globalEventHandler?.Invoke(context);
    }

    //should never be called on Awake() methods
    public void AddGlobalListener(GameEventListener listener)
    {
        globalEventHandler += listener;
    }

    //should never be called on Awake() methods
    public void AddListener(GameEventListener listener, string ev)
    {
        GameEventListener handler;
        if (!eventHandlers.TryGetValue(ev, out handler))
        {
            eventHandlers.Add(ev, null);
        }
        eventHandlers[ev] += listener;
    }


    public void RemoveGlobalListener(GameEventListener listener)
    {
        if (globalEventHandler != null && listener != null)
        {
            globalEventHandler -= listener;
        }
    }

    public void Clear()
    {
        eventHandlers.Clear();
        globalEventHandler = null;
        Init();
    }

    public void RemoveListener(GameEventListener listener, string ev)
    {
        if (eventHandlers != null && eventHandlers.ContainsKey(ev))
        {
            eventHandlers[ev] -= listener;
        }
    }
}