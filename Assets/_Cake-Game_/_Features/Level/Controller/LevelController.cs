using DG.Tweening;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public bool TestMode = false;
    public int TestLevel = 0;
    [Space]
    public Level[] GameLevels;

    Level _currentLevel;
    int _currentLevelIndex;
    bool _gameOver;

    private void OnEnable()
    {
        EventManager.OnStartGame += LoadLevel;
        EventManager.OnStartGameLevel += StartLevel;
        EventManager.OnStopGameLevel += EndLevel;
        EventManager.OnWinLevel += OnLevelWin;
        EventManager.OnFailLevel += OnLevelFail;
    }

    private void OnDisable()
    {
        EventManager.OnStartGame -= LoadLevel;
        EventManager.OnStartGameLevel -= StartLevel;
        EventManager.OnStopGameLevel -= EndLevel;
        EventManager.OnWinLevel -= OnLevelWin;
        EventManager.OnFailLevel -= OnLevelFail;
    }

    private void Start()
    {
        int levelToLoad = Profile.Level;

        LoadLevel(levelToLoad);

        SoundController.Instance.PlayBgm();
    }

    void LoadLevel(int levelIndex)
    {
        _currentLevelIndex = levelIndex;

        if(TestMode)
            _currentLevelIndex = TestLevel;

        if(_currentLevelIndex >= GameLevels.Length)
            _currentLevelIndex = _currentLevelIndex % GameLevels.Length;

        if(_currentLevel != null)
        {
            Destroy(_currentLevel.gameObject);
            _currentLevel = null;
        }

        _currentLevel = Instantiate(GameLevels[_currentLevelIndex]);

        _gameOver = false;

        //_currentLevel.SetPlayState();

        StartLevel();

        //AnalyticsManager.Instance.LogLevelStart(levelIndex);
    }

    void StartLevel()
    {
        if(_currentLevel == null)
            return;

        //EventManager.DoFireHideAllUi();
        //EventManager.DoFireShowUiEvent(UiType.Hud, Profile.Level);

        _currentLevel.gameObject.SetActive(true);
    }

    void EndLevel()
    {
        if(_currentLevel == null)
            return;

        _currentLevel.gameObject.SetActive(false);
    }

    void OnLevelWin()
    {
        if(_gameOver)
            return;

        _gameOver = true;
    }

    void OnLevelFail()
    {
        if(_gameOver)
            return;

        _gameOver = true;

        EventManager.DoFireHideUiEvent(UiType.Hud);

        //_currentLevel.SetLoseState();
        DOVirtual.DelayedCall(1.15f, delegate
        {
            EventManager.DoFireShowUiEvent(UiType.LevelFail);

            //AnalyticsManager.Instance.LogLevelEnd(Profile.Level, false);
            //AdsManager.Ins.ShowInterstitialAd();
        });

        Profile.LevelsFinishedCounter++;
    }

}
