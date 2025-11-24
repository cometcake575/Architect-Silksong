namespace Architect.Utils;

public static class TitleUtils
{
    private static bool _overrideAreaText;
    private static int _areaType;
    private static string _areaHeader;
    private static string _areaBody;
    private static string _areaFooter;
    
    public static void Init()
    {
        HookUtils.OnFsmAwake += fsm =>
        {
            if (fsm.FsmName == "Area Title Control")
            {
                var header = fsm.FsmVariables.FindFsmString("Title Sup");
                var footer = fsm.FsmVariables.FindFsmString("Title Sub");
                var body = fsm.FsmVariables.FindFsmString("Title Main");

                var right = fsm.FsmVariables.FindFsmBool("Display Right");

                fsm.GetState("Init all").AddAction(() =>
                {
                    if (!_overrideAreaText) return;

                    header.value = _areaHeader;
                    footer.value = _areaFooter;
                    body.value = _areaBody;
                });
                
                fsm.GetState("Visited Check").AddAction(() =>
                {
                    if (!_overrideAreaText) return;
                    _overrideAreaText = false;

                    right.value = _areaType == 2;

                    fsm.SendEvent(_areaType == 0 ? "UNVISITED" : "VISITED");
                }, 0);
            }
        };
    }
    
    public static void DisplayTitle(string header, string body, string footer, int type)
    {
        _overrideAreaText = true;
        _areaType = type;
        _areaHeader = header;
        _areaBody = body;
        _areaFooter = footer;
        AreaTitle.Instance.gameObject.SetActive(true);
    }
}