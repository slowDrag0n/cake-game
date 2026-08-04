using UnityEngine.Events;

/// <summary>
/// Events fired by the game. The SDK may listen to some of these.
/// Your GameManager fires these — do not fire them from SDK code.
/// </summary>
public static class GameEvents
{
    // ── Gameplay ──────────────────────────────────────────────────────────

    /// <summary>Fire when a level starts.</summary>
    public static UnityAction OnLevelStart;

    /// <summary>Fire when a level is completed.</summary>
    public static UnityAction OnLevelComplete;

    /// <summary>Fire when a level is failed.</summary>
    public static UnityAction OnLevelFailed;

    /// <summary>Fire when a level is restarted.</summary>
    public static UnityAction OnLevelRestart;

    /// <summary>Fire when a level has been spawned/loaded.</summary>
    public static UnityAction OnLevelSpawned;

    // ── App ───────────────────────────────────────────────────────────────

    /// <summary>Fire when the player lacks a required resource (e.g. no internet). Passes a message string.</summary>
    public static UnityAction<string> OnNotEnoughResources;

    /// <summary>Fire when ads have been removed (after IAP or other grant).</summary>
    public static UnityAction OnAdsRemoved;

    /// <summary>Fire when the player taps an IAP button before the purchase flow starts.</summary>
    public static UnityAction OnIAPClicked;
}
