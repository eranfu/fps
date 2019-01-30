using UnityEngine;

namespace Utils.DebugOverlay
{
    public class DebugOverlayCamera : MonoBehaviour
    {
        private void OnPostRender()
        {
            DebugOverlay.Draw();
        }
    }
}