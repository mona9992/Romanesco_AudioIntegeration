using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LoginLogic : MonoBehaviour
{
    [SerializeField] private TMP_InputField _userNameTextField;
    [SerializeField] private TMP_Text _inputHelp;
    [SerializeField] private GameObject _inputFieldObject;
    [SerializeField] private GameObject _loginButtonObject;
    [SerializeField] private GameObject _loginScreenObject;
    [SerializeField] private GameObject _mainMenunObject;
    [SerializeField] private Image _inputFieldImage;
    [SerializeField] private Color _correctBlinkColor;
    
    private List<string> _characters = new List<string>() 
    { 
        "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", 
        "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
        "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", 
        "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
    };

    private void Start()
    {
        if (LeaderBoardManager.Instance.GetUserName().IsNullOrWhitespace())
        {
            ShowLogin();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && ! _mainMenunObject.activeInHierarchy)
        {
            LoginButton();
        }
    }

    private string GetInputFromUsernameField()
    {
        return _userNameTextField.text;
    }
    
    public void LoginButton()
    {
        if (GetInputFromUsernameField().Length < 3)
        {
            WrongLoginAnim();
            return;
        }
        LeaderBoardManager.Instance.Login(GetInputFromUsernameField());
        LoginAnim();
    }
    
    public void PlayButtonSound(){
        SoundManager.Post(SoundManager.Sound.ButtonSelect);
    }

    private void ShowLogin()
    {
        _loginScreenObject.SetActive(true);
        _mainMenunObject.SetActive(false);
    }

    private void ShowMainMenu()
    {
        _loginScreenObject.SetActive(false);
        _mainMenunObject.SetActive(true);
    }

    private void AnimateLoginToMainMenu()
    {
        //ShowMainMenu();
        _mainMenunObject.SetActive(true);
        _mainMenunObject.transform.position += Vector3.right * -1000;
        _mainMenunObject.transform.DOMoveX(_mainMenunObject.transform.position.x + 1000, 0.5f);
        _loginScreenObject.transform.DOMoveX(_loginScreenObject.transform.position.x + 1000, 0.5f).OnComplete(() =>
        {
            _loginScreenObject.SetActive(false);
        });
    }

    private void LoginAnim()
    {
        SoundManager.Post(SoundManager.Sound.LoginPass);
        _loginButtonObject.transform.DOScale(Vector3.zero, 0.5f);
        StartCoroutine(ScrambleLetters(1f, 0.1f));
    }

    private void WrongLoginAnim()
    {
        SoundManager.Post(SoundManager.Sound.LoginFail);
        StartCoroutine(WrongInput());
    }

    private IEnumerator WrongInput()
    {
        _inputHelp.text = "Name needs to be at least 3 letters";
        float blinkWaitTime = 0.3f;
        yield return new WaitForSeconds(blinkWaitTime);
        _inputFieldImage.color = Color.red;
        yield return new WaitForSeconds(blinkWaitTime);
        _inputFieldImage.color = Color.white;
        yield return new WaitForSeconds(blinkWaitTime);
        _inputFieldImage.color = Color.red;
        yield return new WaitForSeconds(blinkWaitTime);
        _inputFieldImage.color = Color.white;
    }

    private IEnumerator ScrambleLetters(float duration, float timePerSwitch)
    {
        //_inputHelp.text = "Success";
        float timeElapsed = 0;
        int nameLength = _userNameTextField.text.Length;
        string originalName = _userNameTextField.text;
        while (timeElapsed < duration)
        {
            _userNameTextField.text = "";
            for (int i = 0; i < nameLength; i++)
            {
                _userNameTextField.text += _characters[Random.Range(0, _characters.Count - 1)];
            }
            timeElapsed += timePerSwitch;
            yield return new WaitForSeconds(timePerSwitch);
        }

        _userNameTextField.text = originalName;
        float blinkWaitTime = 0.3f;
        
        _inputFieldImage.color = _correctBlinkColor;
        yield return new WaitForSeconds(blinkWaitTime);
        _inputFieldImage.color = Color.white;
        yield return new WaitForSeconds(blinkWaitTime);
        _inputFieldImage.color = _correctBlinkColor;
        yield return new WaitForSeconds(blinkWaitTime);
        _inputFieldImage.color = Color.white;
        yield return new WaitForSeconds(blinkWaitTime);
        _inputFieldImage.color = _correctBlinkColor;
        yield return new WaitForSeconds(blinkWaitTime);
        _inputFieldImage.color = Color.white;
        yield return new WaitForSeconds(blinkWaitTime);
        
        AnimateLoginToMainMenu();
    }
}
