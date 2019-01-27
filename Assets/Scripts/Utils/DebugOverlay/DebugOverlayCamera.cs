using UnityEngine;

namespace Utils.DebugOverlay
{
    public class DebugOverlayCamera : MonoBehaviour
    {
        private void OnPostRender()
        {
            var line3DBuffer = DebugOverlay.GetLine3DBuffer();
            line3DBuffer?.Draw();
        }
    }
}