using System;
using System.Collections;
using System.Collections.Generic;
using Architect.Sharer.Info;
using Architect.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Architect.Sharer.States;

public class Browse : MenuState
{
    public static Browse Instance;
    
    public override MenuState ReturnState => SharerManager.HomeState;

    public int page;
    public int totalPages;

    private Text _pageCounter;
    
    private readonly List<Listing> _listings = [];

    private ScrollRect _scrollRect;

    private RequestManager.FilterInfo _filterInfo = new();
    
    private GameObject _browse;
    private GameObject _level;
    private GameObject _filter;
    
    private readonly List<Button> _filterBtns = [];
    private Dictionary<LevelInfo.LevelDuration, Button> _durationButtons = [];
    private Dictionary<LevelInfo.LevelDifficulty, Button> _difficultyButtons = [];
    private Dictionary<LevelInfo.LevelTag, Button> _tagButtons = [];

    private InputField _searchField;
    private InputField _usernameField;
        
    private static readonly ColorBlock Active = new()
    {
        normalColor = Color.yellow,
        highlightedColor = Color.yellow,
        colorMultiplier = 1,
        selectedColor = Color.yellow,
        pressedColor = UIUtils.LightGrey,
        fadeDuration = 0.1f
    };
    private static readonly ColorBlock ActiveGreen = new()
    {
        normalColor = Color.green,
        highlightedColor = Color.green,
        colorMultiplier = 1,
        selectedColor = Color.green,
        pressedColor = UIUtils.LightGrey,
        fadeDuration = 0.1f
    };
    private static readonly ColorBlock ActiveRed = new()
    {
        normalColor = Color.red,
        highlightedColor = Color.red,
        colorMultiplier = 1,
        selectedColor = Color.red,
        pressedColor = UIUtils.LightGrey,
        fadeDuration = 0.1f
    };
    private static readonly ColorBlock Inactive = new()
    {
        normalColor = Color.white,
        highlightedColor = Color.white,
        colorMultiplier = 1,
        selectedColor = Color.white,
        pressedColor = UIUtils.LightGrey,
        fadeDuration = 0.1f
    };

    private SortingRule _currentRule = SortingRule.Featured;
    
    private Button _featuredBtn;
    private Button _likedBtn;
    private Button _downloadedBtn;
    private Button _newBtn;
    private Button _filterBtn;

    private void Start()
    {
        Instance = this;
        
        _browse = new GameObject("Browse");
        _browse.transform.SetParent(transform, false);
        _browse.RemoveOffset();
        
        _filter = new GameObject("Filter");
        _filter.transform.SetParent(transform, false);
        _filter.RemoveOffset();
        
        _level = new GameObject("Level");
        _level.transform.SetParent(transform, false);
        _level.RemoveOffset();
        
        (_featuredBtn, var labelFeatured) = UIUtils.MakeTextButton("Most Recent Likes", "Featured", 
            _browse, new Vector2(-275, 220), 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(390, 100));
        labelFeatured.textComponent.fontSize = 18;
        _featuredBtn.onClick.AddListener(() => SetSortingRule(SortingRule.Featured));
        
        (_likedBtn, var labelLiked) = UIUtils.MakeTextButton("Liked", "Most Liked",
            _browse, new Vector2(-125, 220), 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(390, 100));
        labelLiked.textComponent.fontSize = 18;
        _likedBtn.onClick.AddListener(() => SetSortingRule(SortingRule.Liked));
        
        (_downloadedBtn, var labelDownloaded) = UIUtils.MakeTextButton("Downloaded", "Most Downloaded",
            _browse, new Vector2(25, 220), 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(390, 100));
        labelDownloaded.textComponent.fontSize = 18;
        _downloadedBtn.onClick.AddListener(() => SetSortingRule(SortingRule.Downloaded));
        
        (_newBtn, var labelNew) = UIUtils.MakeTextButton("New", "New Levels",
            _browse, new Vector2(175, 220), 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(390, 100));
        labelNew.textComponent.fontSize = 18;
        _newBtn.onClick.AddListener(() => SetSortingRule(SortingRule.New));

        (_filterBtn, var imgFilter, _) = UIUtils.MakeButtonWithImage("Filter", 
            _browse, new Vector2(278, 220), 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            100,
            80);
        _filterBtn.onClick.AddListener(() =>
        {
            _browse.SetActive(false);
            _filter.SetActive(true);
        });
        
        imgFilter.sprite = ResourceUtils.LoadSpriteResource("Sharer.filter");

        SetupLevelZone();
        SetupFilterZone();
        SetupLevelInfoZone();
        
        RefreshButtons();
        DoSearch();
    }

    public override void OnOpen()
    {
        if (!_featuredBtn) return;
        
        _currentRule = SortingRule.Featured;
        WipeFilters();
    }

    private void SetSortingRule(SortingRule rule)
    {
        page = 0;
        _currentRule = rule;
        _filterInfo.SortingRule = rule;
        RefreshButtons();
        DoSearch();
    }

    private void WipeFilters()
    {
        _filterInfo = new RequestManager.FilterInfo
        {
            KeyFilter = RequestManager.SharerKey
        };
        _browse.SetActive(true);
        _filter.SetActive(false);
        _level.SetActive(false);
        foreach (var btn in _filterBtns) btn.colors = Inactive;
        _searchField.text = "";
        _usernameField.text = "";
        RefreshButtons();
        DoSearch();
    }

    private void RefreshButtons()
    {
        _featuredBtn.colors = _currentRule == SortingRule.Featured ? Active : Inactive;
        _likedBtn.colors = _currentRule == SortingRule.Liked ? Active : Inactive;
        _downloadedBtn.colors = _currentRule == SortingRule.Downloaded ? Active : Inactive;
        _newBtn.colors = _currentRule == SortingRule.New ? Active : Inactive;
        _filterBtn.colors = _filterInfo.Active ? Active : Inactive;
    }

    private LevelInfo _info;
    
    private Text _title;
    private Text _desc;
    private Text _tags;
    private Image _img;

    private Text _userTitle;
    private Text _userDesc;
    private Image _userImg;

    private Button _downloadSBtn;

    private void SetupLevelInfoZone()
    {
        _title = UIUtils.MakeLabel("Title", _level, new Vector2(-195, 125),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        _title.fontSize = 20;
        _title.alignment = TextAnchor.UpperCenter;
        
        _desc = UIUtils.MakeLabel("Desc",
            _level, new Vector2(-195, 55),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            maxWidth: 300, maxHeight: 160).textComponent;
        _desc.fontSize = 15;
        _desc.horizontalOverflow = HorizontalWrapMode.Wrap;
        _desc.alignment = TextAnchor.UpperCenter;
        
        _tags = UIUtils.MakeLabel("Tags",
            _level, new Vector2(-85, -130),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            maxWidth: 290, maxHeight: 160)
            .textComponent;
        _tags.fontSize = 15;
        _tags.alignment = TextAnchor.UpperLeft;

        _img = UIUtils.MakeImage("Icon",
            _level, new Vector2(-195, 230),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 200));
        _img.preserveAspect = true;
        
        _userTitle = UIUtils.MakeLabel("Title", _level, new Vector2(195, 125),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        _userTitle.fontSize = 20;
        _userTitle.alignment = TextAnchor.UpperCenter;
        
        _userDesc = UIUtils.MakeLabel("Desc",
            _level, new Vector2(195, 55),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            maxWidth: 300, maxHeight: 160).textComponent;
        _userDesc.fontSize = 15;
        _userDesc.horizontalOverflow = HorizontalWrapMode.Wrap;
        _userDesc.alignment = TextAnchor.UpperCenter;

        _userImg = UIUtils.MakeImage("Icon",
            _level, new Vector2(195, 230),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 200));
        _userImg.preserveAspect = true;
        
        var (closeBtn, closeLabel) = UIUtils.MakeTextButton("Close", "Close",
            _level, new Vector2(-160, -240),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(420, 80));
        closeLabel.textComponent.fontSize = 18;
        closeBtn.onClick.AddListener(() =>
        {
            _level.SetActive(false);
            _browse.SetActive(true);
        });
        
        var (downloadBtn, downloadLabel) = UIUtils.MakeTextButton("Download Level", "Download Level", 
            _level, new Vector2(0, -240),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(420, 80));
        downloadLabel.textComponent.fontSize = 18;
        
        (_downloadSBtn, var downloadSLabel) = UIUtils.MakeTextButton("Download Save", "Download Save", 
            _level, new Vector2(160, -240),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(420, 80));
        downloadSLabel.textComponent.fontSize = 18;

        var status = UIUtils.MakeLabel("Status", _level,
            new Vector2(0, -280),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        status.fontSize = 15;
        status.alignment = TextAnchor.MiddleCenter;
        
        _downloadSBtn.onClick.AddListener(() =>
        {
            StartCoroutine(DownloadSave());
        });
        
        downloadBtn.onClick.AddListener(() =>
        {
            StartCoroutine(DownloadLevel());
        });

        _level.SetActive(false);

        return;

        IEnumerator DownloadSave()
        {
            downloadBtn.interactable = false;
            _downloadSBtn.interactable = false;
            closeBtn.interactable = false;
            SharerManager.ReturnBtn.interactable = false;
            yield return RequestManager.DownloadSave(_info.LevelId, status);
            closeBtn.interactable = true;
            downloadBtn.interactable = true;
            _downloadSBtn.interactable = true;
            SharerManager.ReturnBtn.interactable = true;
        }

        IEnumerator DownloadLevel()
        {
            downloadBtn.interactable = false;
            _downloadSBtn.interactable = false;
            closeBtn.interactable = false;
            SharerManager.ReturnBtn.interactable = false;
            yield return RequestManager.DownloadLevel(_info.LevelId, status);
            closeBtn.interactable = true;
            downloadBtn.interactable = true;
            if (_info.HasSave) _downloadSBtn.interactable = true;
            SharerManager.ReturnBtn.interactable = true;
        }
    }

    private void OpenLevel(LevelInfo info)
    {
        _info = info;
        
        _browse.SetActive(false); 
        _level.SetActive(true);
        
        _title.text = info.LevelName;
        _desc.text = info.LevelDesc;
        _downloadSBtn.interactable = info.HasSave;
        var tags = "";
        if (info.Tags.IsNullOrEmpty()) tags = "None";
        for (var i = 0; i < info.Tags.Count; i++)
        {
            tags += info.Tags[i].GetLabel();
            if (i < info.Tags.Count - 1) tags += ", ";
        }
        _tags.text = $"Length: {info.Duration.GetLabel()}\n\n" +
                     $"Difficulty: {info.Difficulty.GetLabel()}\n\n" +
                     $"Tags: {tags}";
        SharerManager.DoGetSprite(info.IconURL, _img);

        _userTitle.text = info.CreatorName;
        StartCoroutine(RequestManager.GetUserInfo(new UserInfo(info.CreatorId, false),
            (_, desc, pfp) =>
            {
                _userDesc.text = desc;
                SharerManager.DoGetSprite(pfp, _userImg);
            }));
    }
    
    private void SetupFilterZone()
    {
        _filter.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 15);
        
        // Name (text box)
        var nameTitle = UIUtils.MakeLabel("Name Title", _filter, new Vector2(-105, 245),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        nameTitle.fontSize = 18;
        nameTitle.text = "Search Query";
        nameTitle.alignment = TextAnchor.MiddleLeft;
        
        (_searchField, var nameLabel) = UIUtils.MakeTextbox("Name Input", _filter, new Vector2(0, 215),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 800, 65);
        nameLabel.textComponent.fontSize = 15;
        nameLabel.transform.localScale = Vector3.one;
        ((RectTransform)nameLabel.transform).sizeDelta /= 3;
        _searchField.characterLimit = 30;
        
        // Username (text box)
        var usernameTitle = UIUtils.MakeLabel("Username Title", _filter, new Vector2(-105, 165),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        usernameTitle.fontSize = 18;
        usernameTitle.text = "Creator";
        usernameTitle.alignment = TextAnchor.MiddleLeft;
        
        (_usernameField, var usernameLabel) = UIUtils.MakeTextbox("Username Input", _filter, new Vector2(0, 135),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 800, 65);
        usernameLabel.textComponent.fontSize = 15;
        usernameLabel.transform.localScale = Vector3.one;
        ((RectTransform)usernameLabel.transform).sizeDelta /= 3;
        _usernameField.characterLimit = 30;
        
        // Length
        var lenTitle = UIUtils.MakeLabel("I Length Title", _filter, new Vector2(-180, 85),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        lenTitle.fontSize = 18;
        lenTitle.text = "Include Length";
        lenTitle.alignment = TextAnchor.MiddleCenter;
        MakeTagBtn(LevelInfo.LevelDuration.None.GetLabel(), LevelInfo.LevelDuration.None, new Vector2(-225, 55), ref _durationButtons, FlipDuration);
        MakeTagBtn(LevelInfo.LevelDuration.Tiny.GetLabel(), LevelInfo.LevelDuration.Tiny, new Vector2(-135, 55), ref _durationButtons, FlipDuration);
        MakeTagBtn(LevelInfo.LevelDuration.Short.GetLabel(), LevelInfo.LevelDuration.Short, new Vector2(-270, 25), ref _durationButtons, FlipDuration);
        MakeTagBtn(LevelInfo.LevelDuration.Medium.GetLabel(), LevelInfo.LevelDuration.Medium, new Vector2(-180, 25), ref _durationButtons, FlipDuration);
        MakeTagBtn(LevelInfo.LevelDuration.Long.GetLabel(), LevelInfo.LevelDuration.Long, new Vector2(-90, 25), ref _durationButtons, FlipDuration);

        var lenETitle = UIUtils.MakeLabel("E Length Title", _filter, new Vector2(180, 85),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        lenETitle.fontSize = 18;
        lenETitle.text = "Exclude Length";
        lenETitle.alignment = TextAnchor.MiddleCenter;
        MakeTagBtn(LevelInfo.LevelDuration.None.GetLabel(), LevelInfo.LevelDuration.None, new Vector2(135, 55), ref _durationButtons, FlipEDuration);
        MakeTagBtn(LevelInfo.LevelDuration.Tiny.GetLabel(), LevelInfo.LevelDuration.Tiny, new Vector2(225, 55), ref _durationButtons, FlipEDuration);
        MakeTagBtn(LevelInfo.LevelDuration.Short.GetLabel(), LevelInfo.LevelDuration.Short, new Vector2(90, 25), ref _durationButtons, FlipEDuration);
        MakeTagBtn(LevelInfo.LevelDuration.Medium.GetLabel(), LevelInfo.LevelDuration.Medium, new Vector2(180, 25), ref _durationButtons, FlipEDuration);
        MakeTagBtn(LevelInfo.LevelDuration.Long.GetLabel(), LevelInfo.LevelDuration.Long, new Vector2(270, 25), ref _durationButtons, FlipEDuration);

        // Difficulty
        var diffTitle = UIUtils.MakeLabel("Difficulty Title", _filter, new Vector2(-180, -25),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        diffTitle.fontSize = 18;
        diffTitle.text = "Include Difficulty";
        diffTitle.alignment = TextAnchor.MiddleCenter;
        MakeTagBtn(LevelInfo.LevelDifficulty.None.GetLabel(), LevelInfo.LevelDifficulty.None, new Vector2(-225, -55), ref _difficultyButtons, FlipDifficulty);
        MakeTagBtn(LevelInfo.LevelDifficulty.Easy.GetLabel(), LevelInfo.LevelDifficulty.Easy, new Vector2(-135, -55), ref _difficultyButtons, FlipDifficulty);
        MakeTagBtn(LevelInfo.LevelDifficulty.Medium.GetLabel(), LevelInfo.LevelDifficulty.Medium, new Vector2(-270, -85), ref _difficultyButtons, FlipDifficulty);
        MakeTagBtn(LevelInfo.LevelDifficulty.Hard.GetLabel(), LevelInfo.LevelDifficulty.Hard, new Vector2(-180, -85), ref _difficultyButtons, FlipDifficulty);
        MakeTagBtn(LevelInfo.LevelDifficulty.ExtraHard.GetLabel(), LevelInfo.LevelDifficulty.ExtraHard, new Vector2(-90, -85), ref _difficultyButtons, FlipDifficulty);
        
        var diffETitle = UIUtils.MakeLabel("Difficulty Title", _filter, new Vector2(180, -25),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        diffETitle.fontSize = 18;
        diffETitle.text = "Exclude Difficulty";
        diffETitle.alignment = TextAnchor.MiddleCenter;
        MakeTagBtn(LevelInfo.LevelDifficulty.None.GetLabel(), LevelInfo.LevelDifficulty.None, new Vector2(135, -55), ref _difficultyButtons, FlipEDifficulty);
        MakeTagBtn(LevelInfo.LevelDifficulty.Easy.GetLabel(), LevelInfo.LevelDifficulty.Easy, new Vector2(225, -55), ref _difficultyButtons, FlipEDifficulty);
        MakeTagBtn(LevelInfo.LevelDifficulty.Medium.GetLabel(), LevelInfo.LevelDifficulty.Medium, new Vector2(90, -85), ref _difficultyButtons, FlipEDifficulty);
        MakeTagBtn(LevelInfo.LevelDifficulty.Hard.GetLabel(), LevelInfo.LevelDifficulty.Hard, new Vector2(180, -85), ref _difficultyButtons, FlipEDifficulty);
        MakeTagBtn(LevelInfo.LevelDifficulty.ExtraHard.GetLabel(), LevelInfo.LevelDifficulty.ExtraHard, new Vector2(270, -85), ref _difficultyButtons, FlipEDifficulty);

        // Tags
        var tagTitle = UIUtils.MakeLabel("Tag Title", _filter, new Vector2(-180, -125),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        tagTitle.fontSize = 18;
        tagTitle.text = "Include Tags";
        tagTitle.alignment = TextAnchor.MiddleCenter;
        MakeTagBtn(LevelInfo.LevelTag.Platforming.GetLabel(), LevelInfo.LevelTag.Platforming, new Vector2(-225, -155), ref _tagButtons, FlipTag);
        MakeTagBtn(LevelInfo.LevelTag.Gauntlets.GetLabel(), LevelInfo.LevelTag.Gauntlets, new Vector2(-135, -155), ref _tagButtons, FlipTag);
        MakeTagBtn(LevelInfo.LevelTag.Bosses.GetLabel(), LevelInfo.LevelTag.Bosses, new Vector2(-270, -185), ref _tagButtons, FlipTag);
        MakeTagBtn(LevelInfo.LevelTag.Areas.GetLabel(), LevelInfo.LevelTag.Areas, new Vector2(-180, -185), ref _tagButtons, FlipTag);
        MakeTagBtn(LevelInfo.LevelTag.Minigames.GetLabel(), LevelInfo.LevelTag.Minigames, new Vector2(-90, -185), ref _tagButtons, FlipTag);
        MakeTagBtn(LevelInfo.LevelTag.Multiplayer.GetLabel(), LevelInfo.LevelTag.Multiplayer, new Vector2(-225, -215), ref _tagButtons, FlipTag);
        MakeTagBtn(LevelInfo.LevelTag.Troll.GetLabel(), LevelInfo.LevelTag.Troll, new Vector2(-135, -215), ref _tagButtons, FlipTag);
        
        var tagETitle = UIUtils.MakeLabel("Tag Title", _filter, new Vector2(180, -125),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        tagETitle.fontSize = 18;
        tagETitle.text = "Exclude Tags";
        tagETitle.alignment = TextAnchor.MiddleCenter;
        MakeTagBtn(LevelInfo.LevelTag.Platforming.GetLabel(), LevelInfo.LevelTag.Platforming, new Vector2(135, -155), ref _tagButtons, FlipETag);
        MakeTagBtn(LevelInfo.LevelTag.Gauntlets.GetLabel(), LevelInfo.LevelTag.Gauntlets, new Vector2(225, -155), ref _tagButtons, FlipETag);
        MakeTagBtn(LevelInfo.LevelTag.Bosses.GetLabel(), LevelInfo.LevelTag.Bosses, new Vector2(90, -185), ref _tagButtons, FlipETag);
        MakeTagBtn(LevelInfo.LevelTag.Areas.GetLabel(), LevelInfo.LevelTag.Areas, new Vector2(180, -185), ref _tagButtons, FlipETag);
        MakeTagBtn(LevelInfo.LevelTag.Minigames.GetLabel(), LevelInfo.LevelTag.Minigames, new Vector2(270, -185), ref _tagButtons, FlipETag);
        MakeTagBtn(LevelInfo.LevelTag.Multiplayer.GetLabel(), LevelInfo.LevelTag.Multiplayer, new Vector2(135, -215), ref _tagButtons, FlipETag);
        MakeTagBtn(LevelInfo.LevelTag.Troll.GetLabel(), LevelInfo.LevelTag.Troll, new Vector2(225, -215), ref _tagButtons, FlipETag);
        
        var (clearBtn, clearLabel) = UIUtils.MakeTextButton("Clear", "Clear Filter", _filter,
            new Vector2(-80, -265),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(420, 80));
        clearLabel.textComponent.fontSize = 18;
        clearBtn.onClick.AddListener(WipeFilters);
        
        var (searchBtn, searchLabel) = UIUtils.MakeTextButton("Search", "Search", _filter,
            new Vector2(80, -265),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(420, 80));
        searchLabel.textComponent.fontSize = 18;
        searchBtn.onClick.AddListener(() =>
        {
            _browse.SetActive(true);
            _filter.SetActive(false);
            _filterInfo.Active = true;
            _filterInfo.SearchQuery = _searchField.text;
            _filterInfo.UsernameFilter = _usernameField.text;
            RefreshButtons();
            DoSearch();
        });
        
        _filter.SetActive(false);
    }

    private static readonly Sprite BlankLike = ResourceUtils.LoadSpriteResource("Sharer.blank_up");
    private static readonly Sprite Like = ResourceUtils.LoadSpriteResource("Sharer.up");

    private void SetupLevelZone()
    {
        var downloadIcon = ResourceUtils.LoadSpriteResource("Sharer.downloads");
        
        (_scrollRect, var scroll, _) = UIUtils.MakeScrollView("Scroll",
            _browse, 
            new Vector2(-22.5f, -30),
            new Vector2(362.5f, 0),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(635, 400), 1500);
        
        for (var i = 0; i < 15; i++)
        {
            var listing = new GameObject($"Listing {i}");
            listing.transform.SetParent(scroll.transform, false);
            listing.RemoveOffset();

            var img = UIUtils.MakeImage("Icon",
                listing, new Vector2(-259, 5),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(200, 200));
            img.preserveAspect = true;
            
            var title = UIUtils.MakeLabel("Title",
                listing, new Vector2(19, 40),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                maxWidth: 440, maxHeight: 20).textComponent;
            title.fontSize = 17;
            title.horizontalOverflow = HorizontalWrapMode.Wrap;
            title.alignment = TextAnchor.UpperLeft;
            
            var desc = UIUtils.MakeLabel("Desc",
                listing, new Vector2(15, -15),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                maxWidth: 420).textComponent;
            desc.fontSize = 15;
            desc.horizontalOverflow = HorizontalWrapMode.Wrap;
            desc.alignment = TextAnchor.UpperLeft;

            var downloads = UIUtils.MakeLabel("Download Count",
                listing, new Vector2(270, 12.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
            downloads.alignment = TextAnchor.MiddleCenter;
            
            var likes = UIUtils.MakeLabel("Like Count",
                listing, new Vector2(270, -18.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
            likes.alignment = TextAnchor.MiddleCenter;

            var downloadImg = UIUtils.MakeImage("Download Icon",
                    listing, new Vector2(299, 15),
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(40, 40));
            downloadImg.sprite = downloadIcon;

            var (likeBtn, likeImg, _) = UIUtils.MakeButtonWithImage("Like Icon",
                    listing, new Vector2(299, -15),
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    60, 60, doOutline: false);
            
            var selIcon = UIUtils.MakeImage("Selected Icon",
                listing, new Vector2(-315, 5),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(5, 140));
            selIcon.sprite = UIUtils.Square;
            selIcon.gameObject.SetActive(false);

            var instance = listing.AddComponent<ListingInstance>();
            instance.selected = selIcon.gameObject;
            
            var listObject = new Listing(instance, title, desc, img, likes, downloads,
                downloadImg.gameObject, likeImg, likeBtn.gameObject);
            _listings.Add(listObject);
            
            listObject.Setup(null);
            likeBtn.onClick.AddListener(OnLike);

            continue;

            void OnLike()
            {
                if (listObject.Info == null || listObject.Info.Liked || RequestManager.SharerKey == null) return;
                listObject.Info.Liked = true;
                likeImg.sprite = Like;
                likes.text = (listObject.Info.LikeCount + 1).ToString();

                StartCoroutine(RequestManager.LikeLevel(listObject.Info.LevelId));
            }
        }

        _pageCounter = UIUtils.MakeLabel("Page Number", _browse, new Vector2(-22.5f, -270),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
        _pageCounter.fontSize = 17;
        _pageCounter.alignment = TextAnchor.MiddleCenter;
        
        var (leftBtn, leftLab) = UIUtils.MakeTextButton("Arrow Left", "<=", _browse,
            new Vector2(-72.5f, -270),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size:new Vector2(100, 100));
        leftLab.textComponent.fontSize = 15;
        leftLab.textComponent.alignment = TextAnchor.MiddleCenter;
        leftBtn.onClick.AddListener(() =>
        {
            page--;
            if (page < 0) page = totalPages;
            DoSearch();
        });
        
        var (rightBtn, rightLab) = UIUtils.MakeTextButton("Arrow Right", "=>", _browse,
            new Vector2(27.5f, -270),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size:new Vector2(100, 100));
        rightLab.textComponent.fontSize = 15;
        rightLab.textComponent.alignment = TextAnchor.MiddleCenter;
        rightBtn.onClick.AddListener(() =>
        {
            page++;
            if (page > totalPages) page = 0;
            DoSearch();
        });
    }

    public class ListingInstance : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public LevelInfo Info;
        public GameObject selected;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Info == null) return;
            selected.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            selected.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Info == null) return;
            selected.SetActive(false);
            Instance.OpenLevel(Info);
        }
    }

    public class Listing(ListingInstance instance, 
        Text title, Text desc, Image icon, 
        Text likes, Text downloads, 
        GameObject downloadIcon, Image likeIcon, GameObject likeBtn)
    {
        public LevelInfo Info;
        
        public void Setup(LevelInfo info)
        {
            Info = info;
            instance.selected.SetActive(false);
            instance.Info = info;
            if (info == null)
            {
                title.text = "";
                desc.text = "";
                icon.sprite = ArchitectPlugin.BlankSprite;

                likes.text = "";
                downloads.text = "";
                
                downloadIcon.SetActive(false);
                likeIcon.gameObject.SetActive(false);
                likeBtn.SetActive(false);

                var sr = icon.GetComponent<SharerManager.SpriteURL>();
                if (sr) sr.StopAllCoroutines();
                return;
            }

            title.text = $"{info.LevelName} – {info.CreatorName}";
            desc.text = info.LevelDesc;
            SharerManager.DoGetSprite(info.IconURL, icon);

            likes.text = info.LikeCount.ToString();
            downloads.text = info.DownloadCount.ToString();
                
            downloadIcon.SetActive(true);
            likeIcon.gameObject.SetActive(true);
            likeBtn.SetActive(true);
            
            likeIcon.sprite = info.Liked ? Like : BlankLike;
        }
    }

    private int FlipDuration(LevelInfo.LevelDuration obj)
    {
        if (_filterInfo.IncludedDurations.Remove(obj)) return -1;
        _filterInfo.IncludedDurations.Add(obj);
        return 0;
    }

    private int FlipDifficulty(LevelInfo.LevelDifficulty obj)
    {
        if (_filterInfo.IncludedDifficulty.Remove(obj)) return -1;
        _filterInfo.IncludedDifficulty.Add(obj);
        return 0;
    }

    private int FlipTag(LevelInfo.LevelTag obj)
    {
        if (_filterInfo.IncludedTags.Remove(obj)) return -1;
        _filterInfo.IncludedTags.Add(obj);
        return 0;
    }

    private int FlipEDuration(LevelInfo.LevelDuration obj)
    {
        if (_filterInfo.ExcludedDurations.Remove(obj)) return -1;
        _filterInfo.ExcludedDurations.Add(obj);
        return 1;
    }

    private int FlipEDifficulty(LevelInfo.LevelDifficulty obj)
    {
        if (_filterInfo.ExcludedDifficulty.Remove(obj)) return -1;
        _filterInfo.ExcludedDifficulty.Add(obj);
        return 1;
    }

    private int FlipETag(LevelInfo.LevelTag obj)
    {
        if (_filterInfo.ExcludedTags.Remove(obj)) return -1;
        _filterInfo.ExcludedTags.Add(obj);
        return 1;
    }

    private void DoSearch()
    {
        StartCoroutine(Search());
    }

    private IEnumerator Search()
    {
        _scrollRect.verticalNormalizedPosition = 1;
        _filterInfo.KeyFilter = RequestManager.SharerKey;
        yield return RequestManager.SearchLevels(_filterInfo, 
            15, page, (success, levels, pages) =>
        {
            if (!success)
            {
                SharerManager.TransitionToState(SharerManager.HomeState);
                return;
            }

            var i = 0;
            foreach (var info in levels)
            {
                _listings[i].Setup(info);
                i++;
            }
            for (; i < _listings.Count; i++)
            {
                _listings[i].Setup(null);
            }

            totalPages = pages;
            _pageCounter.text = $"{page+1}/{totalPages+1}";
        });
    }

    private void MakeTagBtn<T>(string btnName, T value, Vector2 pos, ref Dictionary<T, Button> buttons, Func<T, int> onClick)
    {
        var (btn, label) = UIUtils.MakeTextButton(btnName, btnName, _filter, pos, 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(220, 60));
        label.textComponent.fontSize = 15;
        btn.onClick.AddListener(() =>
        {
            btn.colors = onClick(value) switch
            {
                0 => ActiveGreen,
                1 => ActiveRed,
                _ => Inactive
            };
        });
        _filterBtns.Add(btn);
        buttons[value] = btn;
    }
    
    public enum SortingRule
    {
        Featured,
        Liked,
        Downloaded,
        New
    }
}