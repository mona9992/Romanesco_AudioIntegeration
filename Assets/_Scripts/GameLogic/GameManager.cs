using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    
    public float _timeSinceGameStarted;
    
    // Cursor
    [SerializeField] private Texture2D _cursorTexture;
    private const CursorMode CursorMode = UnityEngine.CursorMode.Auto;
    private Vector2 _hotSpot = Vector2.zero;
    
    [SerializeField] private GameObject _gameOverVisualEffectPrefab;
    [SerializeField] private Vector2 _roomBounds;

    private bool _currentlyAnimatingPauseMenu;
    
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
    }
    
    private void Start()
    {
        InitializeValuesAndReferences();
        SetupCursor();
    }

    private void InitializeValuesAndReferences()
    {
        SoundManager.Reset();
        SoundManager.Post(SoundManager.Sound.StartGame);
        //SoundManager.PlayRandomSound(SoundManager.Sound.Music, this.transform, true, SoundManager.SoundType.Music);     
        Time.timeScale = 1;
        _timeSinceGameStarted = 0;
        
        //_scoreManager = GameObject.FindWithTag("UnityPlugin").GetComponent<ScoreManager>();
        
        GameStateManager.Instance.SetGameState(GameStateManager.GameState.Running);
    }
    
    private void SetupCursor()
    {
        _hotSpot = new Vector2(_cursorTexture.width / 2f, _cursorTexture.height / 2f);
        Cursor.SetCursor(_cursorTexture, _hotSpot, CursorMode);
    }

    
    void Update()
    {
        
        if (  ( Input.GetKeyDown(KeyCode.P) ||  Input.GetKeyDown(KeyCode.Escape) )  && !UIManager.Instance._currentlyAnimatingPauseMenu && !GameStateManager.Instance.IsGameOver())
        {
            if (GameStateManager.Instance.IsRunning() )
            {
                GameStateManager.Instance.SetGameState(GameStateManager.GameState.Paused);
            }
            else
            {
                GameStateManager.Instance.SetGameState(GameStateManager.GameState.Running);
            }
        }

        if (GameStateManager.Instance.IsPaused())
        {
            return;
        }
        
        _timeSinceGameStarted += Time.deltaTime;
    }

    public void ResumeGame()
    {
        GameStateManager.Instance.SetGameState(GameStateManager.GameState.Running);
    }

    public void GoToLevel(string levelName) // For Resume, Next Level and Back to Main Menu
    {
        if (levelName == "this")
        {
            SoundManager.Post(SoundManager.Sound.ButtonSelect);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }
        SceneManager.LoadScene(levelName);
    }

    public void ExitGame() 
    {
        Application.Quit();
    }
    
    public Vector2 GetRoomBounds()
    {
        return _roomBounds;
    }

    public void SwitchUser()
    {
        DEVHACKS.Instance.RestartScene();
    }
}
