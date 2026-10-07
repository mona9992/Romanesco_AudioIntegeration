using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Vector3 _posGoal;
    [SerializeField] private Vector3 _scaleGoal;
    [SerializeField] private string _gameSceneName = "Level";
    [SerializeField] private float _introSequenceDuration;
    [SerializeField] private Transform _menuTransform;
   
    private Vector3 _startPos;
    private Vector3 _startScale;
    private bool _introStarted;
    
    private IEnumerator LoadSceneEnumerator(string levelName)
    {
        yield return new WaitForSeconds(0.5f);
        if (levelName == "this") SceneManager.LoadScene(SceneManager.loadedSceneCount);
        SceneManager.LoadScene(levelName);
    }
    
    public void LoadScene(string levelName) // For Resume, Next Level and Back to Main Menu
    {
        StartCoroutine(LoadSceneEnumerator(levelName));
    }

    public void StartIntro()
    {
        if (!_introStarted)
        {
            StartCoroutine(Intro());
            _introStarted = true;
        }
        
    }

    private IEnumerator Intro()
    {
        _startPos = _menuTransform.transform.localPosition;
        _startScale = _menuTransform.transform.localScale;
        SoundManager.Post(SoundManager.Sound.SceneLoad);

        _menuTransform.DOLocalMove(_posGoal, _introSequenceDuration).SetEase(Ease.InOutSine);
        _menuTransform.DOScale(_scaleGoal, _introSequenceDuration).SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(_introSequenceDuration);
        
        LoadScene(_gameSceneName);

    }
    
    public void PlayButtonSound(){
        SoundManager.Post(SoundManager.Sound.ButtonSelect);
    }
    
    public void Close()
    {
        Application.Quit();
    }
}
