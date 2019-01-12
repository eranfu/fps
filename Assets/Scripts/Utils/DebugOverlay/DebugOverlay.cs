using UnityEngine;

namespace Utils.DebugOverlay
{
    public class DebugOverlay : MonoBehaviour
    {
        private static DebugOverlay _instance;
        private static char[] _buf = new char[1024];

        public static void Write(float x, float y, string format)
        {
            if (_instance == null)
            {
                return;
            }

            int l = TextFormatter.Write(ref _buf, 0, format);
            _instance.DrawText(x, y, ref _buf, l);
        }

        private void DrawText(float x, float y, ref char[] text, int length)
        {
            throw new System.NotImplementedException();
        }
    }
}