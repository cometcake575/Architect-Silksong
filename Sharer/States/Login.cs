using System.Collections;
using Architect.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Sharer.States;

public class Login : MenuState
{
    public override MenuState ReturnState => SharerManager.HomeState;

    private Button _loginBtn;
    private Button _signupBtn;
    private Text _result;

    private InputField _userField;
    private InputField _pwField;
    
    public override void OnStart()
    {
        var userText = UIUtils.MakeLabel("Username Title", gameObject,
            new Vector2(-165, 85), 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        userText.fontSize = 20;
        userText.text = "Username:";
        userText.alignment = TextAnchor.MiddleLeft;
        
        (_userField, var userBoxLabel) = UIUtils.MakeTextbox("Username Input", gameObject,
            new Vector2(0, 55),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            1225, 75);
        userBoxLabel.textComponent.fontSize = 16;
        userBoxLabel.transform.localScale = Vector3.one;
        ((RectTransform)userBoxLabel.transform).sizeDelta /= 3;
        _userField.characterLimit = 20;
        
        var passText = UIUtils.MakeLabel("Password Title", gameObject,
            new Vector2(-165, 5), 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        passText.fontSize = 20;
        passText.text = "Password:";
        passText.alignment = TextAnchor.MiddleLeft;
        
        (_pwField, var pwBoxLabel) = UIUtils.MakeTextbox("Password Input", gameObject,
            new Vector2(0, -25),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            1225, 75);
        pwBoxLabel.textComponent.fontSize = 16;
        pwBoxLabel.transform.localScale = Vector3.one;
        ((RectTransform)pwBoxLabel.transform).sizeDelta /= 3;
        _pwField.inputType = InputField.InputType.Password;

        (_loginBtn, var loginLabel) = UIUtils.MakeTextButton("Log In", "Log In", gameObject,
            new Vector2(-100, -85),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(420, 80));
        loginLabel.textComponent.fontSize = 20;

        (_signupBtn, var signupLabel) = UIUtils.MakeTextButton("Sign Up", "Sign Up", gameObject,
            new Vector2(100, -85),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(420, 80));
        signupLabel.textComponent.fontSize = 20;
        
        _result = UIUtils.MakeLabel("Status", gameObject,
            new Vector2(0, -140), 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        _result.fontSize = 16;
        _result.alignment = TextAnchor.MiddleCenter;

        _loginBtn.onClick.AddListener(() => StartCoroutine(Login(false)));
        _signupBtn.onClick.AddListener(() => StartCoroutine(Login(true)));

        return;

        IEnumerator Login(bool signup)
        {
            _loginBtn.interactable = false;
            _signupBtn.interactable = false;
            
            yield return RequestManager.Login(signup, _userField.text, _pwField.text, _result);
            
            if (RequestManager.SharerKey != null)
            {
                yield return new WaitForSeconds(1);
                SharerManager.TransitionToState(SharerManager.HomeState);
            }
            else
            {
                _loginBtn.interactable = true;
                _signupBtn.interactable = true;
            }
        }
    }

    public override void OnOpen()
    {
        if (!didStart) return;
        _loginBtn.interactable = true;
        _signupBtn.interactable = true;
        _result.text = "";
        _userField.text = "";
        _pwField.text = "";
    }
}