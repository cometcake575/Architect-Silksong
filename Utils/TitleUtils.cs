using System.Collections;
using Architect.Content.Preloads;
using HutongGames.PlayMaker;
using UnityEngine;

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
    
    private static GameObject _bigBoss;
    private static PlayMakerFSM _bigBossFsm;
    private static FsmState _bigBossFsmUp;
    private static SetTextMeshProGameText _bigBossHeader;
    private static SetTextMeshProGameText _bigBossBody;
    private static SetTextMeshProGameText _bigBossFooter;
    
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
        
        PreloadManager.RegisterPreload(new BasicPreload("Abyss_Cocoon", "Boss Control/Boss Title",
            o =>
            {
                o.SetActive(false);
                _bigBoss = Object.Instantiate(o);
                Object.DontDestroyOnLoad(_bigBoss);
                
                _bigBossFsm = _bigBoss.GetComponent<PlayMakerFSM>();
                _bigBossFsmUp = _bigBossFsm.GetState("Title Up");

                var stt = _bigBoss.transform.GetChild(1).GetChild(1);
                _bigBossHeader = stt.GetChild(1).GetComponent<SetTextMeshProGameText>();
                _bigBossBody = stt.GetChild(0).GetComponent<SetTextMeshProGameText>();
                _bigBossFooter = stt.GetChild(2).GetComponent<SetTextMeshProGameText>();

                var fe = _bigBossFsm.GetState("Flash Effect");
                fe.DisableAction(2);
                fe.DisableAction(3);
                
                _bigBossFsm.GetState("Idle").AddAction(() =>
                {
                    _bigBossFsm.SendEvent("TITLE UP");
                });
                
                _bigBossFsm.GetState("Title Down").AddAction(() =>
                {
                    _bigBossFsm.StartCoroutine(TitleDown());
                });

                return;

                IEnumerator TitleDown()
                {
                    yield return new WaitForSeconds(0.5f);
                    _bigBoss.SetActive(false);
                }
            }));
    }
    
    public static void DisplayTitle(string header, string body, string footer, int type, bool waitForCancel = false)
    {
        if (type == 3)
        {
            DisplayBigBoss(header, body, footer, waitForCancel);
            return;
        }
        _overrideAreaText = true;
        _areaType = type;
        _areaHeader = header;
        _areaBody = body;
        _areaFooter = footer;
        _waitForCancel = waitForCancel;
        AreaTitle.Instance.gameObject.SetActive(true);
    }

    private static void DisplayBigBoss(string header, string body, string footer, bool waitForCancel)
    {
        _bigBossFsmUp.actions[1].enabled = !waitForCancel;
        
        _bigBossHeader.Text = (LocalStr)header;
        _bigBossHeader.setTextOn.text = header;
        
        _bigBossBody.Text = (LocalStr)body;
        _bigBossBody.setTextOn.text = body;
        
        _bigBossFooter.Text = (LocalStr)footer;
        _bigBossFooter.setTextOn.text = footer;
        
        _bigBoss.SetActive(true);
    }

    public static void CancelTitle()
    {
        _fsm.SendEvent("FINISHED");
        _fsm.SendEvent("NPC TITLE DOWN");
        _bigBossFsm.SendEvent("FINISHED");
    }
}