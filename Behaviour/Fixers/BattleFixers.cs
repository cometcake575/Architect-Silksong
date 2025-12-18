using UnityEngine;

namespace Architect.Behaviour.Fixers;

public static class BattleFixers
{
    public interface IBattleFixer
    {
        public void Setup(GameObject obj);
    }

    public class AnimBattleFixer(string id) : IBattleFixer
    {
        public void Setup(GameObject obj)
        {
            var anim = obj.GetComponent<tk2dSpriteAnimator>();
            anim.defaultClipId = anim.GetClipIdByName(id);
        }
    }

    public class FsmBoolBattleFixer(string varName) : IBattleFixer
    {
        public void Setup(GameObject obj)
        {
            obj.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool(varName).Value = true;
        }
    }

    public class FallbackBattleFixer : IBattleFixer
    {
        public void Setup(GameObject obj)
        {
            // TODO Make fallback logic
        }
    }
}