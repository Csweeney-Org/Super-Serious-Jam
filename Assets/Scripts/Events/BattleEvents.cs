using Assets.Scripts.Throwables;
using System;

public static class BattleEvents
{
    public delegate void ItemEvent(ItemPickup item);
    public delegate void ToppleEvent(SpinCharacterController spinner);
    public delegate void EndGameEvent(bool gameIsWon);

    public static event ItemEvent OnItemPickedUp;
    public static event ItemEvent OnItemDropped;

    public static event ToppleEvent OnToppleEvent;
    public static event EndGameEvent OnEndGameEvent;
    public static event Action OnStartGameEvent;

    public static void InvokeItemPickedUpEvent(ItemPickup item)
    {
        if (OnItemPickedUp != null) OnItemPickedUp(item);
    }
    public static void InvokeItemDroppedEvent(ItemPickup item)
    {
        OnItemDropped?.Invoke(item);
    }
    public static void InvokeEndGameEvent(bool gameWasWon)
    {
        OnEndGameEvent?.Invoke(gameWasWon);
    }
    public static void InvokeStartGameEvent()
    {
        OnStartGameEvent?.Invoke();
    }
    public static void InvokeToppleEvent(SpinCharacterController spinner)
    {
        OnToppleEvent?.Invoke(spinner);
    }

    public static void ClearAllEventListeners()
    {
        OnItemPickedUp = null;
        OnItemDropped = null;
        OnEndGameEvent = null;
        OnStartGameEvent = null;
    }
}