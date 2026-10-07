using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _powerUpTimeIndicatorBar;
    [SerializeField] private GameObject _powerUpTimeIndicatorContentHolder;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _addScoreText;
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private TMP_Text _powerUpHintText;
    [SerializeField] private TMP_Text _powerUpNameText;
    [SerializeField] private float _moveLengthPowerUpHintAnim;
    [SerializeField] private float _durationPowerUpHintAnim;
    
    [SerializeField] private Color _refreshTimerFlashColor;
    [SerializeField] private float _refreshTimerFlashDuration;
    [SerializeField] private float _refreshTimerScaleMultiplierDuration;
    
    [SerializeField] private TMP_Text _finalScoreText;
    [SerializeField] private TMP_Text _roomsCleardText;
    [SerializeField] private TMP_Text _streakBonusText;
    [SerializeField] private TMP_Text _enemiesKilledText;
    [SerializeField] private TMP_Text _timeDeductionText;
    

    // Menus
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _gameOverMenu;
    [SerializeField] private GameObject _leaderboard;
    
    [SerializeField] private GameObject _gameOverVisualEffectPrefab;
    
    [SerializeField] private CameraManager _cameraManager;

    [SerializeField] private TMP_Text _streakTextUI;
    [SerializeField] private List<Image> _streakSquares;

    [SerializeField] private GameObject _pauseMenuMainSection;
    [SerializeField] private GameObject _pauseMenuOptionsSection;
    [SerializeField] private LevelController levelController;
    
    private Vector3 _scoreTextStartPos;
    private Vector3 _timeTextStartPos;
    private Vector3 _upgradeHintStartPos;

    private Sequence _upgradeHintSequenceStart;
    private Sequence _upgradeHintSequenceReturn;
    
    private Vector2 _powerUpTimeIndicatorStartPos;
    private Vector2 _powerUpTimeIndicatorHolderStartPos;
    private float _powerUpTimeIndicatorLength;
    private float _powerUpTimeIndicatorHolderLength;

    public GameManager gameManager;
    

    private bool _scoreBreakdownRunning;
    
    private static readonly int GameOver = Animator.StringToHash("GameOver");

    private List<TMP_Text> _currentlyCountingNumbersInField = new List<TMP_Text>();
    private int _currentlyShownScore;
    private bool _currentlyShowingAddScore;

    private Color _squareStartingColor;
    private Color _squareLastColor;
    private Vector3 _startingSquareScale;
    private bool _newStreakStageAnimRunning;

    public static UIManager Instance;
    public bool _currentlyAnimatingPauseMenu;

    private bool _currentlySwappingBetweenMenus;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        GameStateManager.GameStateChanged += ReactToGameStateChange;
        PlayerController.OnPlayerDeath += GameOverCondition;
        ScoreManager.ScoreAdded += ShowScoreAdded;
        StreakLogic.StreakReached += DisplayStreakUI;
        
    }

    private void OnDestroy()
    {
        GameStateManager.GameStateChanged -= ReactToGameStateChange;
        PlayerController.OnPlayerDeath -= GameOverCondition;
        ScoreManager.ScoreAdded -= ShowScoreAdded;
        StreakLogic.StreakReached -= DisplayStreakUI;
    }

    private void Start ()
    {
        _powerUpTimeIndicatorStartPos = _powerUpTimeIndicatorBar.transform.localPosition;
        _powerUpTimeIndicatorHolderStartPos = _powerUpTimeIndicatorContentHolder.transform.localPosition;
        _powerUpTimeIndicatorLength = _powerUpTimeIndicatorBar.GetComponent<RectTransform>().rect.width;
        _powerUpTimeIndicatorHolderLength = _powerUpTimeIndicatorContentHolder.GetComponent<RectTransform>().rect.width;
        
        _scoreTextStartPos = _scoreText.transform.position;
        _timeTextStartPos = _timeText.transform.position;
        _upgradeHintStartPos = _powerUpHintText.transform.position;
        _squareStartingColor = _streakSquares[0].color;
        _startingSquareScale = _streakSquares[0].transform.localScale;
        _squareLastColor = StreakLogic.Instance.GetCurrentStreakColor();

    }

    private void Update()
    {
        if (GameStateManager.Instance.IsPaused())
        {
            return;
        }
        UpdateUIText();
    }

    private void UpdateUIText()
    {
        _timeText.text = "Room: " + levelController._totalLevelIndex.ToString();
    }

    private void ReactToGameStateChange(GameStateManager.GameState newState)
    {
        switch (newState)
        {
            case GameStateManager.GameState.Paused:
                Pause();
                break;
            case GameStateManager.GameState.Running:
                Resume();
                break;
        }
    }
    
    private void OnRectTransformDimensionsChange()
    {
        _cameraManager.ResetCamPos();
    }

    private void Pause()
    {
        SoundManager.PauseAllPlayingSounds();
        SoundManager.Post(SoundManager.Sound.StartGame);
        StartCoroutine(_cameraManager.BattleTransition(1, true));
        StartCoroutine(SetActiveDelayed(1, true));
        StartCoroutine(SetAnimatingBoolForTime(1));
        
    }
    
    public void Resume()
    {
        SoundManager.Post(SoundManager.Sound.StartGame);
        SoundManager.ResumeAllPlayingSounds();
        _pauseMenu.SetActive(false);
        StartCoroutine(_cameraManager.BattleTransition(1, false));
        StartCoroutine(SetAnimatingBoolForTime(1));
    }
    
    public void GameOverCondition()
    {
        SoundManager.StopAllPlayingSounds();
        SoundManager.Post(SoundManager.Sound.PlayerDeath);
        _gameOverVisualEffectPrefab.GetComponent<Animator>().SetTrigger(GameOver);
        _gameOverMenu.SetActive(true);
        _scoreText.enabled = false;
        _timeText.enabled = false;
        
        gameManager.GetComponent<StatisticManager>().RegisterAnalytics();
        GameStateManager.Instance.SetGameState(GameStateManager.GameState.GameOver);
        ScoreManager.Instance.SaveScoreForPlayer();
        
        RunGameOverTextAnimation();
    }

    private void RunGameOverTextAnimation()
    {
        if (_scoreBreakdownRunning)
        {
            return;
        }
        _scoreBreakdownRunning = true;
        
        var finalScoreBreakdown = ScoreManager.Instance.GetEndScoreBreakdownList();
        
        _finalScoreText.enabled = true;

        const float duration = 0.5f;
        
        StartCoroutine(CountNumberToEnd(_enemiesKilledText,"Enemy Score: ", duration, 0, finalScoreBreakdown[0]));
        
        StartCoroutine(CountNumberToEnd(_streakBonusText,"Streak Score: ", duration, 0, finalScoreBreakdown[1], duration));
        
        StartCoroutine(CountNumberToEnd(_roomsCleardText,"Room Score: ", duration, 0, finalScoreBreakdown[2], duration * 2f));
        
        StartCoroutine(CountNumberToEnd(_timeDeductionText,"Bounce Score: ", duration, 0, finalScoreBreakdown[3], duration * 3f));
        
        StartCoroutine(CountNumberToEnd(_finalScoreText,"Total Score: ", duration, 0, ScoreManager.Instance.GetCurrentDisplayScore(), duration * 4f));
        
    }
    
    private IEnumerator CountNumberToEnd(TMP_Text textField,  string staticText, float duration, int start, int end, float waitTime = 0, bool scaleUp = true)
    {
        _currentlyCountingNumbersInField.Add(textField);
        
        yield return new WaitForSeconds(waitTime);

        if (scaleUp)
        {
            Vector3 saveScale = textField.transform.localScale;
            textField.transform.localScale = Vector3.zero;
            textField.transform.DOScale(saveScale, 0.25f);
        }
        
        textField.gameObject.SetActive(true);
        textField.text = staticText;
        
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            textField.text = staticText + Mathf.Lerp(start, end, (elapsedTime / duration)).ToString("N0");
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        textField.text = staticText + end.ToString("N0");
        _currentlyCountingNumbersInField.Remove(textField);
    }

    public void DisplayLeaderboard()
    {
        _leaderboard.SetActive(true);
    }
    
    public void HideLeaderboard()
    {
        _leaderboard.SetActive(false);
    }

    private IEnumerator SetActiveDelayed(float delay, bool active)
    {
        yield return new WaitForSeconds(delay);
        _pauseMenu.SetActive(active);
    }

    private IEnumerator SetAnimatingBoolForTime(float time)
    {
        _currentlyAnimatingPauseMenu = true;
        yield return new WaitForSeconds(time);
        _currentlyAnimatingPauseMenu = false;
    }
    
    public void ShowPowerUpUI(BasePowerUpHolder data)
    {
        _powerUpNameText.text = data.DisplayName;
        _powerUpHintText.text = data.TutorialText;

        ShowPowerUpNameWithAnim();
        ShowUpgradeHintWithAnim(data.TutorialTextDuration);
    }

    private void ShowPowerUpNameWithAnim()
    {
        _powerUpTimeIndicatorContentHolder.transform.DOLocalMoveX(_powerUpTimeIndicatorHolderStartPos.x + _powerUpTimeIndicatorHolderLength, _durationPowerUpHintAnim).OnComplete(
            () =>
            {
                _powerUpTimeIndicatorBar.SetActive(true);
            });
    }

    public void HidePowerUpUI()
    { 
        _powerUpNameText.text = "";
        HidePowerUpNameWithAnim();
    }

    private void HidePowerUpNameWithAnim()
    {
        _powerUpTimeIndicatorBar.SetActive(false);
        _powerUpTimeIndicatorContentHolder.transform.DOLocalMoveX(_powerUpTimeIndicatorHolderStartPos.x, _durationPowerUpHintAnim);
    }

    private void ShowUpgradeHintWithAnim(float durationTillEnd)
    {
        _upgradeHintSequenceStart = DOTween.Sequence();
        _upgradeHintSequenceStart.Insert(0, _scoreText.transform.DOMoveX(_scoreTextStartPos.x + _moveLengthPowerUpHintAnim, _durationPowerUpHintAnim));
        _upgradeHintSequenceStart.Insert(0, _powerUpHintText.transform.DOMoveX(_upgradeHintStartPos.x + _moveLengthPowerUpHintAnim, _durationPowerUpHintAnim));
        _upgradeHintSequenceStart.Insert(0, _timeText.transform.DOMoveX(_timeTextStartPos.x + _moveLengthPowerUpHintAnim, _durationPowerUpHintAnim));
        
        Invoke(nameof(HideUpgradeHintWithAnim), durationTillEnd);

    }
    
    private void HideUpgradeHintWithAnim()
    {
        _upgradeHintSequenceReturn = DOTween.Sequence();
        _upgradeHintSequenceReturn.Insert(0, _scoreText.transform.DOMoveX(_scoreTextStartPos.x, _durationPowerUpHintAnim));
        _upgradeHintSequenceReturn.Insert(0, _powerUpHintText.transform.DOMoveX(_upgradeHintStartPos.x , _durationPowerUpHintAnim));
        _upgradeHintSequenceReturn.Insert(0, _timeText.transform.DOMoveX(_timeTextStartPos.x, _durationPowerUpHintAnim));
    }
    
    public void PositionBarToPercent(float percentage)
    {
        _powerUpTimeIndicatorBar.transform.localPosition = new Vector3(_powerUpTimeIndicatorStartPos.x + _powerUpTimeIndicatorLength * percentage, _powerUpTimeIndicatorStartPos.y);
    }

    public void RefreshTimerEffect()
    {
        Image loadingBarImage = _powerUpTimeIndicatorBar.GetComponent<Image>();
        Color saveColor = loadingBarImage.color;
        loadingBarImage.color = _refreshTimerFlashColor;

        loadingBarImage.transform.DOPunchScale(loadingBarImage.transform.localScale * _refreshTimerScaleMultiplierDuration, _refreshTimerFlashDuration).OnComplete(() =>
        {
            loadingBarImage.color = saveColor;
        });
    }
    public void PlayButtonSound(){
        SoundManager.Post(SoundManager.Sound.ButtonSelect);
    }

    private void ShowScoreAdded(int scoreAdded)
    {
        UpdateScoreDisplay();
        
        // Commented since it was too much happening 
        /*if (_currentlyShowingAddScore)
        {
            int currentlyShownInt = int.Parse(_addScoreText.text.Substring(1,_addScoreText.text.Length - 2));
            _addScoreText.text = "+" + (currentlyShownInt + scoreAdded).ToString();
            return;
        }
        
        _addScoreText.text = "+" + scoreAdded;
        _addScoreText.gameObject.SetActive(true);
        Vector3 saveScale = _addScoreText.transform.localScale;
        _addScoreText.transform.localScale = Vector3.zero;
        _currentlyShowingAddScore = true;
        _addScoreText.transform.DOScale(saveScale, 0.5f).OnComplete(() =>
        {
            _currentlyShowingAddScore = false;
            _addScoreText.gameObject.SetActive(false);
            UpdateScoreDisplay();
        });*/
    }

    private void UpdateScoreDisplay()
    {
        if (_currentlyCountingNumbersInField.Contains(_scoreText))
        {
            return;
        }
        StartCoroutine(CountNumberToEnd(_scoreText, "Score: ", 0.5f, _currentlyShownScore,
            ScoreManager.Instance.GetCurrentDisplayScore(), 0, false));
        _currentlyShownScore = ScoreManager.Instance.GetCurrentDisplayScore();
    }

    private void SquareSpawnAnim(Image squareRenderer)
    {
        if (_newStreakStageAnimRunning)
        {
            return;
        }
        
        Transform t = squareRenderer.transform;
        squareRenderer.color = StreakLogic.Instance.GetCurrentStreakColor();
        t.localScale = Vector3.zero;
        t.DOScale(_startingSquareScale * 1.25f, 0.25f).OnComplete(() =>
        {
            t.DOScale(_startingSquareScale, 0.125f).OnComplete(() => 
            {
                if (_streakSquares.IndexOf(squareRenderer) == _streakSquares.Count - 1)
                {
                    //StartCoroutine(StreakNextStepReached());
                    SquareDespawnAnim(true);
                }
            });
        });
    }

    private void CleanUpSquaresFromIndex(int index)
    {
        for(int i = 0; i < _streakSquares.Count; i++)
        {
            if (i < index + 1)
            {
                _streakSquares[i].color = StreakLogic.Instance.GetCurrentStreakColor();
                _streakSquares[i].transform.localScale = _startingSquareScale;
            }
            else
            {
                //_streakSquares[i].color = _squareStartingColor;
                _streakSquares[i].transform.localScale = _startingSquareScale;
            }
        }
    }

    private void ChangeAllSquaresColor(Color color)
    {
        foreach (var square in _streakSquares)
        {
            square.color = color;
        }
    }
    
    private IEnumerator StreakNextStepReached()
    {
        _newStreakStageAnimRunning = true;
        Color newColor = _squareLastColor;
        yield return new WaitForSeconds(0.1f);
        ChangeAllSquaresColor(_squareStartingColor);
        yield return new WaitForSeconds(0.1f);
        ChangeAllSquaresColor(newColor);
        yield return new WaitForSeconds(0.1f);
        ChangeAllSquaresColor(_squareStartingColor);
        yield return new WaitForSeconds(0.1f);
        ChangeAllSquaresColor(newColor);
        yield return new WaitForSeconds(0.1f);
        SquareDespawnAnim(true);
       
    }
    

    private void SquareDespawnAnim(bool nextStep = false)
    {
        foreach (var square in _streakSquares)
        {
            Transform t = square.transform;
            t.DOScale(Vector3.zero, 0.125f).OnComplete(() =>
            {
                if (!nextStep)
                {
                    square.color = _squareStartingColor;
                }
                t.DOScale(_startingSquareScale, 0.125f).OnComplete(() =>
                {
                    if (nextStep)
                    {
                        square.color = StreakLogic.Instance.GetPreviousStreakColor();
                        _newStreakStageAnimRunning = false;
                        //CleanUpSquaresFromIndex((StreakLogic.Instance.CurrentStreak()%5) - 1);
                    }
                });
               
            });
            
        }
    }
    
    private void DisplayStreakUI(int newStreak)
    {
        _streakTextUI.text = "Streak X" + newStreak;
        _streakTextUI.color = StreakLogic.Instance.GetCurrentStreakColor();

        if (newStreak == 0)
        {
            SquareDespawnAnim();
            return;
        }

        int index = (newStreak-1)%5;
        SquareSpawnAnim(_streakSquares[index]);
        CleanUpSquaresFromIndex(index);

    }

    public void ShowMainPauseMenu()
    {
        if (_currentlySwappingBetweenMenus)
        {
            return;
        }
        SwapByMove(_pauseMenuMainSection, _pauseMenuOptionsSection, 1000f, 0.35f);
        
    }
    
    public void ShowOptionsPauseMenu()
    {
        if (_currentlySwappingBetweenMenus)
        {
            return;
        }
        SwapByMove(_pauseMenuOptionsSection, _pauseMenuMainSection, -1000f, 0.35f);
    }

    private void SwapByMove(GameObject moveIn, GameObject moveOut, float distance, float time)
    {
        _currentlySwappingBetweenMenus = true;
        Vector3 moveOutOrgPos = moveOut.transform.position;
        moveOut.transform.DOMoveX(moveOutOrgPos.x + distance, time).OnComplete(() =>
        {
            moveOut.SetActive(false);
            moveOut.transform.position = moveOutOrgPos;
            
            Vector3 moveInOrgPos = moveIn.transform.position;
            moveIn.SetActive(true);
            moveIn.transform.position -= Vector3.right * -distance;
            moveIn.transform.DOMoveX(moveInOrgPos.x, time);
            _currentlySwappingBetweenMenus = false;
        });
        
       
    }
}
