using System.Globalization;
using Architect.Editor;
using UnityEngine;

namespace Architect.Objects.Tools;

public class ResetObject() : ToolObject("reset_rocket", Storage.Settings.Reset, -5)
{
    internal static readonly ResetObject Instance = new();

    private static float _resetTime;

    private static bool _resetting;

    public static void RestartDelay()
    {
        if (_resetting)
        {
            EditorUI.ResetRocketTime.enabled = false;
            _resetting = false;
            _resetTime = 0;
            EditorUI.ResetRocketTime.text = "";
        }
    }

    public override string GetName()
    {
        return "Reset";
    }

    public override string GetDescription()
    {
        return "Hold for 3 seconds to reset the current room.\n\nWarning: This cannot be undone!";
    }

    public override void Click(Vector3 mousePosition, bool first)
    {
        if (first) _resetting = true;
        if (!_resetting) return;

        _resetTime += Time.deltaTime;

        if (_resetTime >= 3)
        {
            RestartDelay();
            ActionManager.PerformAction(new ResetRoom());
            return;
        }

        EditorUI.ResetRocketTime.enabled = true;
        EditorUI.ResetRocketTime.text = Mathf.Ceil(3 - _resetTime).ToString(CultureInfo.InvariantCulture);
    }
}