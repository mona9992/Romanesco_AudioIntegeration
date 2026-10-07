using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class DisplayLeaderBoardLogic : MonoBehaviour
{
    private List<PlayerLeaderboardEntry> _leaderboardEntries;
    
    [SerializeField] private List<TMP_Text> _names;
    [SerializeField] private List<TMP_Text> _ranking;
    [SerializeField] private List<TMP_Text> _positions;

    [SerializeField] private TMP_Text _ownName;
    [SerializeField] private TMP_Text _ownScore;
    [SerializeField] private TMP_Text _ownRank;

    private List<TMP_Text> _allText = new List<TMP_Text>();

    private string _currentLeaderBoardName;

    private const string _defaultLeaderBoardName = "Score";
    private const string _testLeaderBoardName = "Score";
    
    
    [SerializeField] private Color _ownColor;

    private readonly List<string> _characters = new List<string>() 
    { 
        "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", 
        "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
        "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", 
        "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
    };
    
    private readonly List<string> _numberAndSymbols = new List<string>() 
    { 
        "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
    };
    
    private void Awake()
    {
        LeaderBoardManager.OnLeaderBoardRetrieved += SaveLeaderBoardEntries;
        LeaderBoardManager.OnLeaderBoardAroundPlayerRetrieved += ReactToPlayerDataRetrieved;
    }

    private void OnDestroy()
    {
        LeaderBoardManager.OnLeaderBoardRetrieved -= SaveLeaderBoardEntries;
        LeaderBoardManager.OnLeaderBoardAroundPlayerRetrieved -= ReactToPlayerDataRetrieved;
    }

    void Start()
    {
        InitLeaderBoardLogic();
        
        _allText.AddRange(_names);
        _allText.AddRange(_ranking);
        _allText.AddRange(_positions);
        _allText.Add(_ownName);
        _allText.Add(_ownScore);
        _allText.Add(_ownRank);
    }

    private void InitLeaderBoardLogic()
    {
        LeaderBoardManager.Instance.GetLeaderboard();
    }
    
    private void SaveLeaderBoardEntries( List<PlayerLeaderboardEntry> entries)
    {
        _leaderboardEntries = entries;
        DisplayEntries();
        LeaderBoardManager.Instance.GetPlayerInfo();
    }

    private void ReactToPlayerDataRetrieved(GetLeaderboardAroundPlayerResult results)
    {
        int pos = results.Leaderboard[0].Position;
        PlayerLeaderboardEntry entry = results.Leaderboard[0];
        
        if ( pos < 10)
        {
            _names[pos].text = entry.DisplayName;
            _ranking[pos].text = entry.StatValue.ToString();
            
            _names[pos].color = _ownColor;
            _ranking[pos].color = _ownColor;
            _positions[pos].color = _ownColor;
            
        }
        else
        {
            _ownName.text = entry.DisplayName;
            _ownScore.text = entry.StatValue.ToString();
            _ownRank.text = (entry.Position + 1).ToString();
        }
        
        PlayScrambleAnimation();
        EnableAll();

    }

    private void DisplayEntries()
    {
        
        for (int i = 0; i < _names.Count; i++)
        {
            _names[i].text = "";
            _ranking[i].text = "";
        }

        foreach (var entry in _leaderboardEntries)
        {
            if (entry.Position < 10)
            {
                _names[entry.Position].text = entry.DisplayName;
                _ranking[entry.Position].text = entry.StatValue.ToString();
            }
        }

        DisableAll();

    }

    private void SetTextObjectActiveStatus(bool newActive, List<TMP_Text> listToSet)
    {
        foreach (var text in listToSet)
        {
            text.enabled = newActive;
        }
    }

    private void DisableAll()
    {
        SetTextObjectActiveStatus(false, _names);
        SetTextObjectActiveStatus(false, _ranking);
        SetTextObjectActiveStatus(false, _positions);
        _ownScore.enabled = false;
        _ownName.enabled = false;
        _ownRank.enabled = false;
    }

    private void EnableAll()
    {
        SetTextObjectActiveStatus(true, _names);
        SetTextObjectActiveStatus(true, _ranking);
        SetTextObjectActiveStatus(true, _positions);
        _ownScore.enabled = true;
        _ownName.enabled = true;
        _ownRank.enabled = true;
    }
    
    private IEnumerator ScrambleLetters(float duration, float timePerSwitch, TMP_Text textField, List<string> scrambleOptions)
    {
        float timeElapsed = 0;
        int textLength = textField.text.Length;
        float timePerLetter = duration / textLength;
        string originalText = textField.text;

        if (originalText.Length == 0)
        {
            yield break;
        }

        textField.text = "";
        textField.enabled = true;
        
        int startIndex = 0;
        while (timeElapsed < duration)
        {
            textField.text = originalText[..startIndex];
            for (int i = startIndex; i < textLength; i++)
            {
                textField.text += scrambleOptions[Random.Range(0, scrambleOptions.Count - 1)];
            }
            timeElapsed += timePerSwitch;

            startIndex = Mathf.Clamp ((int)(timeElapsed / timePerLetter), 0 , textLength-1);
            
            yield return new WaitForSeconds(timePerSwitch);
        }

        textField.text = originalText;
    }
    
    private void PlayScrambleAnimation()
    {
        SoundManager.Post(SoundManager.Sound.Shuffle);
        const float duration = 1.5f;
        const float timePerSwitch = 0.05f;
        
        foreach (var nameField in _names)
        {
            StartCoroutine(ScrambleLetters(duration, timePerSwitch, nameField, _characters));
        }
        
        foreach (var scoreField in _ranking)
        {
            StartCoroutine(ScrambleLetters(duration, timePerSwitch, scoreField, _numberAndSymbols));
        }
        
        foreach (var positionField in _positions)
        {
            StartCoroutine(ScrambleLetters(duration, timePerSwitch, positionField, _numberAndSymbols));
        }

        StartCoroutine(ScrambleLetters(duration, timePerSwitch, _ownName,  _characters));
        StartCoroutine(ScrambleLetters(duration, timePerSwitch, _ownScore, _numberAndSymbols));
        StartCoroutine(ScrambleLetters(duration, timePerSwitch, _ownRank,  _numberAndSymbols));

        foreach (var text in _allText)
        {
            text.DOFade(0, 0);
            text.DOFade(1, duration).SetEase(Ease.InOutSine);
        }
    }

    public void SwitchLeaderboard(string newLeaderBoard)
    {
        PlayFabManager.Instance.SwitchLeaderBoard(newLeaderBoard);
        InitLeaderBoardLogic();
        SwitchAnim();
        ResetUI();
    }

    private void ResetUI()
    {
        for (int i = 0; i < _names.Count; i++)
        {
            _names[i].text = "";
            _ranking[i].text = "";
            _names[i].color = Color.white;
            _positions[i].color = Color.white;
            
            _ownRank.text = "";
            _ownName.text = "";
            _ownScore.text = "";
            
            _ownRank.color = Color.white;
            _ownName.color = Color.white;
            _ownScore.color = Color.white;
        }
    }

    private void SwitchAnim()
    {
       Debug.Log("Switched");
    }

    private void PlayPositionFlyInAnimation()
    {
        foreach (var positionField in _positions)
        {
            PlaySinglePosAnim(positionField);
        }
        PlaySinglePosAnim(_ownRank);
    }

    private static void PlaySinglePosAnim(TMP_Text field)
    {
        field.enabled = true;
        Vector3 startPos = field.transform.position;
        field.transform.position = startPos + Vector3.left * 250f;
        field.transform.DOMoveX(startPos.x, 0.5f);
    }

}
