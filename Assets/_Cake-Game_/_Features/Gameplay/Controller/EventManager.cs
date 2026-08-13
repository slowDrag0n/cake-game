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

    public delegate LevelCharacter GetLevelCharacter();
    public static GetLevelCharacter OnGetLevelCharacter;
    public static LevelCharacter DoFireGetLevelCharacter() => OnGetLevelCharacter?.Invoke();

    public delegate string GetLevelName();
    public static GetLevelName OnGetLevelName;
    public static string DoFireGetLevelName() => OnGetLevelName?.Invoke();

    #endregion
}
