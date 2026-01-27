using System.Linq;

namespace Architect.Behaviour.Utility;

public class CustomGuidComponent : GuidComponent
{
    public bool overrideAll;

    private void Update()
    {
        if (overrideAll)
        {
            overrideAll = false;
            foreach (var m in HeroCorpseMarker._activeMarkers
                         .Where(m => m.guidComponent is not CustomGuidComponent)
                         .ToArray())
            {
                m.gameObject.SetActive(false);
            }
        }
    }
}