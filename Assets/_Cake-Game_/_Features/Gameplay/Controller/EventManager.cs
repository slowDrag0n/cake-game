using System.Collections.Generic;
using UnityEngine;

public static partial class EventManager
{

    #region LEVEL EVENTS

    public delegate void StartGame(int lv);
    public static StartGame OnStartGame;
    public static void DoFireStartGame(int lv) => OnStartGame?.Invoke(lv);

    public delegate void StartGameLevel();
    public static StartGameLevel OnStartGameLevel;
    public static void DoFireStartGameLevel() => OnStartGameLevel?.Invoke();

    public delegate void StopGameLevel();
    public static StopGameLevel OnStopGameLevel;
    public static void DoFireStopGameLevel() => OnStopGameLevel?.Invoke();

    public delegate void WinLevel();
    public static WinLevel OnWinLevel;
    public static void DoFireWinLevel() => OnWinLevel?.Invoke();

    public delegate void FailLevel();
    public static FailLevel OnFailLevel;
    public static void DoFireFailLevel() => OnFailLevel?.Invoke();

    public delegate Transform GetLevelTransform();
    public static GetLevelTransform OnGetLevelTransform;
    public static Transform DoFireGetLevelTransform() => OnGetLevelTransform?.Invoke();

    public delegate void EarnedCash(uint amount);
    public static EarnedCash OnEarnedCash;
    public static void DoFireEarnedCash(uint amount) => OnEarnedCash?.Invoke(amount);

    public delegate void SpendCash(uint amount);
    public static SpendCash OnSpendCash;
    public static void DoFireSpendCash(uint amount) => OnSpendCash?.Invoke(amount);

    public delegate void SketchingStarted();
    public static SketchingStarted OnSketchingStarted;
    public static void DoFireSketchingStarted() => OnSketchingStarted?.Invoke();

    public delegate void SketchingEnded();
    public static SketchingEnded OnSketchingEnded;
    public static void DoFireSketchingEnded() => OnSketchingEnded?.Invoke();

    public delegate void ShowLevelTutorial();
    public static ShowLevelTutorial OnShowLevelTutorial;
    public static void DoFireShowLevelTutorial() => OnShowLevelTutorial?.Invoke();

    #endregion



    #region GAMEPLAY EVENTS

    public delegate void StartRopeRetraction();
    public static StartRopeRetraction OnStartRopeRetraction;
    public static void DoFireStartRopeRetraction() => OnStartRopeRetraction?.Invoke();

    public delegate void HighlightRope(bool highlight);
    public static HighlightRope OnHighlightRope;
    public static void DoFireHighlightRope(bool highlight) => OnHighlightRope?.Invoke(highlight);

    public delegate void HandleFingerUp();
    public static HandleFingerUp OnHandleFingerUp;
    public static void DoFireHandleFingerUp() => OnHandleFingerUp?.Invoke();

    public delegate void ShakeCamera();
    public static ShakeCamera OnShakeCamera;
    public static void DoFireShakeCamera() => OnShakeCamera?.Invoke();

    #endregion
}
