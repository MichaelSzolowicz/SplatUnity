using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// Events that can cause a global change in game state which multiple objects may need to react to.
/// </summary>
public enum GameEvent
{
    BalloonPopped
}

/// <summary>
/// For publishing gloabl events that affect the state of multiple objects in the game.
/// </summary>
public static class GameEventBus
{
    private static readonly IDictionary<GameEvent, UnityEvent> events = new Dictionary<GameEvent, UnityEvent>();

    public static void Subscribe(GameEvent eventType, UnityAction listener)
    {
        UnityEvent gameEvent;

        if (events.TryGetValue(eventType, out gameEvent))
        {
            gameEvent.AddListener(listener);
        }
        else
        {
            gameEvent = new UnityEvent();
            gameEvent.AddListener(listener);
            events.Add(eventType, gameEvent);
        }
    }

    public static void Unsubscribe(GameEvent eventType, UnityAction unlistener)
    {
        UnityEvent gameEvent;

        if (events.TryGetValue(eventType, out gameEvent))
        {
            gameEvent.RemoveListener(unlistener);
        }
    }

    public static void Publish(GameEvent eventType)
    {
        UnityEvent gameEvent;

        if (events.TryGetValue(eventType, out gameEvent))
        {
            gameEvent.Invoke();
        }
    }
}