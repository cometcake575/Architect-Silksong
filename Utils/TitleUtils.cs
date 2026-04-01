namespace Architect.Utils;

public static class TitleUtils
{
    private static bool _overrideAreaText;
    private static bool _waitForCancel;
    private static int _areaType;
    private static string _areaHeader;
    private static string _areaBody;
    private static string _areaFooter;

    private static PlayMakerFSM _fsm;
    
    public static void Init()
    {
        HookUtils.OnFsmAwake += fsm =>
        {
            if (fsm.FsmName == "Area Title Control")
            {
                _fsm = fsm;
                var header = fsm.FsmVariables.FindFsmString("Title Sup");
                var footer = fsm.FsmVariables.FindFsmString("Title Sub");
                var body = fsm.FsmVariables.FindFsmString("Title Main");

                var right = fsm.FsmVariables.FindFsmBool("Display Right");
                
                var npc = fsm.FsmVariables.FindFsmBool("NPC Title");
                var wait = fsm.FsmVariables.FindFsmFloat("Unvisited Wait");
                
                fsm.GetState("Init all").AddAction(() =>
                {
                    if (!_overrideAreaText) return;

                    header.value = _areaHeader;
                    footer.value = _areaFooter;
                    body.value = _areaBody;
                    npc.value = _waitForCancel;
                    wait.value = _waitForCancel ? 9999999 : 4.75f;
                });
                
                fsm.GetState("Visited Check").AddAction(() =>
                {
                    if (!_overrideAreaText) return;
                    _overrideAreaText = false;

                    right.value = _areaType == 2;

                    fsm.SendEvent(_areaType == 0 ? "UNVISITED" : "VISITED");
                }, 0);
                
                fsm.GetState("Titles Down 2").AddAction(() =>
                {
                    wait.value = 4.75f;
                }, 0);
            }
        };
    }
    
    public static void DisplayTitle(string header, string body, string footer, int type, bool waitForCancel = false)
    {
        _overrideAreaText = true;
        _areaType = type;
        _areaHeader = header;
        _areaBody = body;
        _areaFooter = footer;
        _waitForCancel = waitForCancel;
        AreaTitle.Instance.gameObject.SetActive(true);
    }

    public static void CancelTitle()
    {
        _fsm.SendEvent("FINISHED");
        _fsm.SendEvent("NPC TITLE DOWN");
    }
}