using System.Collections;
using Architect.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Sharer.States;

public class Login : MenuState
{
    public override MenuState ReturnState => SharerManager.HomeState;

    public override void OnStart()
    {
        var userText = UIUtils.MakeLabel("Username Title", gameObject,
            new Vector2(-165, 85), 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        userText.fontSize = 20;
        userText.text = "Username:";
        userText.alignment = TextAnchor.MiddleLeft;
        
        var (userBox, userBoxLabel) = UIUtils.MakeTextbox("Username Input", gameObject,
            new Vector2(0, 55),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            1225, 75);
        userBoxLabel.textComponent.fontSize = 16;
        userBoxLabel.transform.localScale = Vector3.one;
        ((RectTransform)userBoxLabel.transform).sizeDelta /= 3;
        
        var passText = UIUtils.MakeLabel("Password Title", gameObject,
            new Vector2(-165, 5), 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        passText.fontSize = 20;
        passText.text = "Password:";
        passText.alignment = TextAnchor.MiddleLeft;
        
        var (pwBox, pwBoxLabel) = UIUtils.MakeTextbox("Password Input", gameObject,
            new Vector2(0, -25),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            1225, 75);
        pwBoxLabel.textComponent.fontSize = 16;
        pwBoxLabel.transform.localScale = Vector3.one;
        ((RectTransform)pwBoxLabel.transform).sizeDelta /= 3;
        pwBox.inputType = InputField.InputType.Password;

        var (loginBtn, loginLabel) = UIUtils.MakeTextButton("Log In", "Log In", gameObject,
            new Vector2(-100, -85),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(420, 80));
        loginLabel.textComponent.fontSize = 20;

        var (signupBtn, signupLabel) = UIUtils.MakeTextButton("Sign Up", "Sign Up", gameObject,
            new Vector2(100, -85),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(420, 80));
        signupLabel.textComponent.fontSize = 20;
        
        var result = UIUtils.MakeLabel("Username Title", gameObject,
            new Vector2(0, -100), 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        result.fontSize = 16;
        result.alignment = TextAnchor.MiddleCenter;

        loginBtn.onClick.AddListener(() => StartCoroutine(Login(false)));
        signupBtn.onClick.AddListener(() => StartCoroutine(Login(true)));

        return;

        IEnumerator Login(bool signup)
        {
            loginBtn.interactable = false;
            signupBtn.interactable = false;
            
            yield return RequestManager.Login(signup, userBox.text, pwBox.text, result);

            if (RequestManager.SharerKey == null)
            {
                loginBtn.interactable = true;
                signupBtn.interactable = true;
            }
            else
            {
                yield return new WaitForSeconds(1);
                SharerManager.TransitionToState(SharerManager.HomeState);
            }
        }
    }
}