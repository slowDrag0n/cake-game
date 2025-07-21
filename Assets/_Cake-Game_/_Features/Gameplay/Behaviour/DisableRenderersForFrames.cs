using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DisableRenderersForFrames : MonoBehaviour
{
    public List<Renderer> renderersToToggle;
    public int disableForFrames = 5;

    private IEnumerator Start()
    {
        // Disable all renderers
        foreach(var rend in renderersToToggle)
        {
            if(rend != null)
                rend.enabled = false;
        }

        // Wait for the specified number of frames
        for(int i = 0; i < disableForFrames; i++)
            yield return null;

        // Re-enable all renderers
        foreach(var rend in renderersToToggle)
        {
            if(rend != null)
                rend.enabled = true;
        }
    }
}
