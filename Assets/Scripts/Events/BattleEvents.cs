using Assets.Scripts.Throwables;

public static class BattleEvents
{
    public delegate void ItemEvent(ItemPickup item);
    public static event ItemEvent OnItemPickedUp;

    public static void InvokeItemPickedUpEvent(ItemPickup item)
    {
        if (OnItemPickedUp != null) OnItemPickedUp(item);
    }

    public static void ClearAllEventListeners()
    {
        OnItemPickedUp = null;
    }
}