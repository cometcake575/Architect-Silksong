using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class TitleDisplay : MonoBehaviour, IDisplayable
{
    public string header;
    public string body;
    public string footer;
    public int type;

    public void Display() => TitleUtils.DisplayTitle(header, body, footer, type);
}