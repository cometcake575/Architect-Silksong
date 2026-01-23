using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Architect.Sharer.Info;
using Architect.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Sharer.States;

public class LevelConfig : MenuState
{
    public override MenuState ReturnState => CurrentInfo == null ? 
        SharerManager.HomeState : 
        Home.Manage;

    private InputField _nameField;
    private InputField _saveField;
    private InputField _descField;
    private InputField _iconField;

    private (GameObject, GameObject) _upload;
    private (GameObject, GameObject) _overwriteLevel;
    private (GameObject, GameObject) _overwriteSave;
    private (GameObject, GameObject) _apply;

    private Dictionary<LevelInfo.LevelDuration, Button> _durationButtons = [];
    private Dictionary<LevelInfo.LevelDifficulty, Button> _difficultyButtons = [];
    private Dictionary<LevelInfo.LevelTag, Button> _tagButtons = [];

    private LevelInfo.LevelDuration _duration;
    private LevelInfo.LevelDifficulty _difficulty;
    private List<LevelInfo.LevelTag> _tags;

    // Whether this is a new level or editing an existing one
    public static LevelInfo CurrentInfo;

    public override void OnStart()
    {
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 10);
        
        // Name (text box)
        var nameTitle = UIUtils.MakeLabel("Name Title", gameObject, new Vector2(-200, 245),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        nameTitle.fontSize = 18;
        nameTitle.text = "Level Name";
        nameTitle.alignment = TextAnchor.MiddleLeft;
        
        (_nameField, var nameLabel) = UIUtils.MakeTextbox("Name Input", gameObject, new Vector2(-95, 215),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 800, 65);
        nameLabel.textComponent.fontSize = 15;
        nameLabel.transform.localScale = Vector3.one;
        ((RectTransform)nameLabel.transform).sizeDelta /= 3;
        _nameField.characterLimit = 30;
        
        // Desc (text area)
        var descTitle = UIUtils.MakeLabel("Desc Title", gameObject, new Vector2(-200, 185),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        descTitle.fontSize = 18;
        descTitle.text = "Level Description";
        descTitle.alignment = TextAnchor.MiddleLeft;
        
        (_descField, var descLabel) = UIUtils.MakeTextbox("Desc Input", gameObject, new Vector2(-45, 132.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 1100, 205);
        descLabel.textComponent.fontSize = 15;
        descLabel.textComponent.alignment = TextAnchor.UpperLeft;
        descLabel.transform.localScale = Vector3.one;
        _descField.lineType = InputField.LineType.MultiLineNewline;
        _descField.characterLimit = 300;
        var dlt = (RectTransform)descLabel.transform;
        dlt.sizeDelta = new Vector2(dlt.sizeDelta.x / 3, 70);
        dlt.anchoredPosition = new Vector2(-45, 130);
        
        // Save (text box)
        var saveTitle = UIUtils.MakeLabel("Save Title", gameObject, new Vector2(-200, 75),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        saveTitle.fontSize = 18;
        saveTitle.text = "Attached Save (Optional)";
        saveTitle.alignment = TextAnchor.MiddleLeft;
        
        (_saveField, var saveLabel) = UIUtils.MakeTextbox("Save Input", gameObject, new Vector2(-212, 45),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 100, 65);
        saveLabel.textComponent.fontSize = 15;
        saveLabel.transform.localScale = Vector3.one;
        ((RectTransform)saveLabel.transform).sizeDelta /= 3;
        _saveField.contentType = InputField.ContentType.IntegerNumber;
        _saveField.characterLimit = 2;

        // Icon (text box, preview above)
        var iconTitle = UIUtils.MakeLabel("Icon Title", gameObject, new Vector2(225, 75),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        iconTitle.fontSize = 18;
        iconTitle.text = "Icon URL";
        iconTitle.alignment = TextAnchor.MiddleCenter;
        
        var iconImg = UIUtils.MakeImage("Icon", gameObject, new Vector2(225, 145),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(200, 200));
        iconImg.preserveAspect = true;
        iconImg.sprite = SharerManager.Placeholder;
        
        (_iconField, var iconBoxLabel) = UIUtils.MakeTextbox("Icon URL", gameObject, new Vector2(225, 45),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 300, 65);
        _iconField.onEndEdit.AddListener(s => StartCoroutine(SharerManager.GetSprite(s, iconImg)));
        iconBoxLabel.textComponent.fontSize = 15;
        iconBoxLabel.transform.localScale = Vector3.one;
        ((RectTransform)iconBoxLabel.transform).sizeDelta /= 3;
        
        // Length (can choose one)
        var lenTitle = UIUtils.MakeLabel("Length Title", gameObject, new Vector2(-200, -10),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        lenTitle.fontSize = 18;
        lenTitle.text = "Length";
        lenTitle.alignment = TextAnchor.MiddleLeft;
        MakeTagBtn(LevelInfo.LevelDuration.None.GetLabel(), LevelInfo.LevelDuration.None, new Vector2(-180, -40), ref _durationButtons, SetDuration);
        MakeTagBtn(LevelInfo.LevelDuration.Tiny.GetLabel(), LevelInfo.LevelDuration.Tiny, new Vector2(-90, -40), ref _durationButtons, SetDuration);
        MakeTagBtn(LevelInfo.LevelDuration.Short.GetLabel(), LevelInfo.LevelDuration.Short, new Vector2(0, -40), ref _durationButtons, SetDuration);
        MakeTagBtn(LevelInfo.LevelDuration.Medium.GetLabel(), LevelInfo.LevelDuration.Medium, new Vector2(90, -40), ref _durationButtons, SetDuration);
        MakeTagBtn(LevelInfo.LevelDuration.Long.GetLabel(), LevelInfo.LevelDuration.Long, new Vector2(180, -40), ref _durationButtons, SetDuration);

        // Difficulty (can choose one)
        var diffTitle = UIUtils.MakeLabel("Difficulty Title", gameObject, new Vector2(-200, -80),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        diffTitle.fontSize = 18;
        diffTitle.text = "Difficulty";
        diffTitle.alignment = TextAnchor.MiddleLeft;
        MakeTagBtn(LevelInfo.LevelDifficulty.None.GetLabel(), LevelInfo.LevelDifficulty.None, new Vector2(-180, -110), ref _difficultyButtons, SetDifficulty);
        MakeTagBtn(LevelInfo.LevelDifficulty.Easy.GetLabel(), LevelInfo.LevelDifficulty.Easy, new Vector2(-90, -110), ref _difficultyButtons, SetDifficulty);
        MakeTagBtn(LevelInfo.LevelDifficulty.Medium.GetLabel(), LevelInfo.LevelDifficulty.Medium, new Vector2(0, -110), ref _difficultyButtons, SetDifficulty);
        MakeTagBtn(LevelInfo.LevelDifficulty.Hard.GetLabel(), LevelInfo.LevelDifficulty.Hard, new Vector2(90, -110), ref _difficultyButtons, SetDifficulty);
        MakeTagBtn(LevelInfo.LevelDifficulty.ExtraHard.GetLabel(), LevelInfo.LevelDifficulty.ExtraHard, new Vector2(180, -110), ref _difficultyButtons, SetDifficulty);

        // Tags (can choose multiple)
        var tagTitle = UIUtils.MakeLabel("Tag Title", gameObject, new Vector2(-200, -150),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        tagTitle.fontSize = 18;
        tagTitle.text = "Tags";
        tagTitle.alignment = TextAnchor.MiddleLeft;
        MakeTagBtn(LevelInfo.LevelTag.Platforming.GetLabel(), LevelInfo.LevelTag.Platforming, new Vector2(-135, -180), ref _tagButtons, FlipTag);
        MakeTagBtn(LevelInfo.LevelTag.Gauntlets.GetLabel(), LevelInfo.LevelTag.Gauntlets, new Vector2(0, -180), ref _tagButtons, FlipTag);
        MakeTagBtn(LevelInfo.LevelTag.Bosses.GetLabel(), LevelInfo.LevelTag.Bosses, new Vector2(135, -180), ref _tagButtons, FlipTag);
        MakeTagBtn(LevelInfo.LevelTag.Areas.GetLabel(), LevelInfo.LevelTag.Areas, new Vector2(-135, -210), ref _tagButtons, FlipTag);
        MakeTagBtn(LevelInfo.LevelTag.Multiplayer.GetLabel(), LevelInfo.LevelTag.Multiplayer, new Vector2(0, -210), ref _tagButtons, FlipTag);
        MakeTagBtn(LevelInfo.LevelTag.Troll.GetLabel(), LevelInfo.LevelTag.Troll, new Vector2(135, -210), ref _tagButtons, FlipTag);
        
        // Status
        var status = UIUtils.MakeLabel("Status", gameObject, new Vector2(0, -290),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        status.fontSize = 15;
        status.alignment = TextAnchor.MiddleCenter;

        // Upload button
        var (uploadBtn, uploadLabel) = UIUtils.MakeTextButton("Upload", "Upload", gameObject,
            new Vector2(0, -260),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(420, 80));
        uploadLabel.textComponent.fontSize = 18;
        _upload = (uploadBtn.gameObject, uploadLabel.gameObject);
        uploadBtn.onClick.AddListener(() => Upload());
        
        // Apply and Overwrite buttons
        var (applyBtn, applyLabel) = UIUtils.MakeTextButton("Save Changes", "Save Changes", gameObject,
            new Vector2(160, -260),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(420, 80));
        applyLabel.textComponent.fontSize = 18;
        _apply = (applyBtn.gameObject, applyLabel.gameObject);
        applyBtn.onClick.AddListener(() => Upload(CurrentInfo.LevelId, 0));
        
        var (overwriteBtn, overwriteLabel) = UIUtils.MakeTextButton("Overwrite Level", "Overwrite Level", gameObject,
            new Vector2(-160, -260),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(420, 80));
        overwriteLabel.textComponent.fontSize = 18;
        _overwriteLevel = (overwriteBtn.gameObject, overwriteLabel.gameObject);
        overwriteBtn.onClick.AddListener(() => Upload(CurrentInfo.LevelId, 1));
        
        var (overwriteSaveBtn, overwriteSaveLabel) = UIUtils.MakeTextButton("Overwrite Save", "Overwrite Save", gameObject,
            new Vector2(0, -260),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(420, 80));
        overwriteSaveLabel.textComponent.fontSize = 18;
        _overwriteSave = (overwriteSaveBtn.gameObject, overwriteSaveLabel.gameObject);
        overwriteSaveBtn.onClick.AddListener(() => Upload(CurrentInfo.LevelId, 2));

        return;

        void Upload([CanBeNull] string overwriteLevelId = null, int overwriteMode = -1)
        {
            uploadBtn.interactable = false;
            
            if (!int.TryParse(_saveField.text, out var saveNumber)) saveNumber = -1;
            
            StartCoroutine(RequestManager.UploadLevel(
                _nameField.text,
                _descField.text,
                _iconField.text,
                saveNumber,
                _difficulty,
                _duration,
                _tags,
                (success, message) =>
                {
                    status.text = message;
                    if (success) StartCoroutine(Return(SharerManager.HomeState));
                    else uploadBtn.interactable = true;
                },
                overwriteLevelId, overwriteMode));
        }

        IEnumerator Return(MenuState state)
        {
            yield return new WaitForSeconds(1);
            uploadBtn.interactable = true;
            SharerManager.TransitionToState(state);
        }
    }

    private void FlipTag(LevelInfo.LevelTag t)
    {
        if (!_tags.Remove(t)) _tags.Add(t);
    }

    private void SetDifficulty(LevelInfo.LevelDifficulty difficulty)
    {
        _difficulty = difficulty;
    }

    private void SetDuration(LevelInfo.LevelDuration duration)
    {
        _duration = duration;
    }

    private void MakeTagBtn<T>(string btnName, T value, Vector2 pos, ref Dictionary<T, Button> buttons, Action<T> onClick)
    {
        var (btn, label) = UIUtils.MakeTextButton(btnName, btnName, gameObject, pos, 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(220, 60));
        label.textComponent.fontSize = 15;
        btn.onClick.AddListener(() =>
        {
            onClick(value);
            RefreshButtons();
        });
        buttons[value] = btn;
    }

    public override void OnOpen()
    {
        _saveField.text = "";
        if (CurrentInfo == null)
        {
            _nameField.text = "";
            _descField.text = "";
            _iconField.text = "";
            _difficulty = LevelInfo.LevelDifficulty.None;
            _duration = LevelInfo.LevelDuration.None;
            _tags = [];
            
            _upload.Item1.SetActive(true);
            _upload.Item2.SetActive(true);
            _apply.Item1.SetActive(false);
            _apply.Item2.SetActive(false);
            _overwriteLevel.Item1.SetActive(false);
            _overwriteLevel.Item2.SetActive(false);
            _overwriteSave.Item1.SetActive(false);
            _overwriteSave.Item2.SetActive(false);
        }
        else
        {
            _nameField.text = CurrentInfo.LevelName;
            _descField.text = CurrentInfo.LevelDesc;
            _iconField.text = CurrentInfo.IconURL;
            _difficulty = CurrentInfo.Difficulty;
            _duration = CurrentInfo.Duration;
            _tags = CurrentInfo.Tags.ToList();
            
            _upload.Item1.SetActive(false);
            _upload.Item2.SetActive(false);
            _apply.Item1.SetActive(true);
            _apply.Item2.SetActive(true);
            _overwriteLevel.Item1.SetActive(true);
            _overwriteLevel.Item2.SetActive(true);
            _overwriteSave.Item1.SetActive(true);
            _overwriteSave.Item2.SetActive(true);
        }
        _iconField.onEndEdit.Invoke(_iconField.text);
        RefreshButtons();
    }

    private void RefreshButtons()
    {
        var on = new ColorBlock
        {
            normalColor = Color.green,
            highlightedColor = Color.green,
            colorMultiplier = 1,
            selectedColor = Color.green,
            pressedColor = Color.grey,
            fadeDuration = 0.1f
        };
        var off = new ColorBlock
        {
            normalColor = Color.white,
            highlightedColor = Color.white,
            colorMultiplier = 1,
            selectedColor = Color.white,
            pressedColor = Color.grey,
            fadeDuration = 0.1f
        };
        foreach (var (t, btn) in _tagButtons)
        {
            btn.colors = _tags.Contains(t) ? on : off;
        }
        foreach (var (difficulty, btn) in _difficultyButtons)
        {
            btn.colors = _difficulty == difficulty ? on : off;
        }
        foreach (var (duration, btn) in _durationButtons)
        {
            btn.colors = _duration == duration ? on : off;
        }
    }
}