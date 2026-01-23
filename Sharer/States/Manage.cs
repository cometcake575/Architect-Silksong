using Architect.Utils;
using UnityEngine;

namespace Architect.Sharer.States;

public class Manage : MenuState
{
    public override MenuState ReturnState => SharerManager.HomeState;

    public int page;
    
    public override void OnStart()
    {
        for (var y = -1; y <= 1; y++)
        {
            for (var x = -2; x <= 2; x++)
            {
                var icon = UIUtils.MakeImage("Icon", gameObject,
                    new Vector2(x * 125, y * 160),
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(200, 200));
                icon.sprite = ArchitectPlugin.BlankSprite;

                var levelName = UIUtils.MakeLabel("Name", gameObject,
                    new Vector2(x * 125, y * 160 - 65),
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    maxWidth: 100).textComponent;
                levelName.alignment = TextAnchor.MiddleCenter;
                levelName.truncate = true;
                levelName.fontSize = 15;
                
                icon.sprite = SharerManager.Placeholder;
                levelName.text = "Lorem Ipsum Dolor Sit";
            }
        }
    }

    public override void OnOpen()
    {
        page = 0;
        RefreshPage();
    }

    private void RefreshPage()
    {
        StartCoroutine(RequestManager.SearchLevels(new RequestManager.FilterInfo
        {
            KeyFilter = RequestManager.SharerKey
        }, 15, page, (success, levels) =>
        {
            if (!success)
            {
                SharerManager.TransitionToState(SharerManager.HomeState);
                return;
            }
        }));
    }
}