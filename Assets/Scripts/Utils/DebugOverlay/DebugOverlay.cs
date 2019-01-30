#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;

namespace Utils.DebugOverlay
{
    public class DebugOverlay : ScriptableObject
    {
        private static DebugOverlay _instance;
        private static char[] _buf = new char[1024];
        public static int Width => _instance.width;
        public static int Height => _instance.height;

        [SerializeField] [Header("Overlay size")]
        private int width = 80;

        [SerializeField] private int height = 25;

        [SerializeField] private QuadBuffer quadBuffer;
        [SerializeField] private Line3DBuffer line3DBuffer;
        [SerializeField] private LineBuffer lineBuffer;

        private float _yOrigin;
        private float _xOrigin;
        private Color _currentColor = Color.white;

        private void Awake()
        {
#if UNITY_EDITOR
            Camera[] allSceneCameras = SceneView.GetAllSceneCameras();
            foreach (var sceneCamera in allSceneCameras)
            {
                sceneCamera.gameObject.AddComponent<DebugOverlayCamera>();
            }
#endif
        }

        public void Init()
        {
            _instance = this;
        }

        public void Shutdown()
        {
            quadBuffer.Shutdown();
            quadBuffer = null;
            line3DBuffer.Shutdown();
            line3DBuffer = null;
            lineBuffer.Shutdown();
            lineBuffer = null;

            _instance = null;
        }

        public void TickLateUpdate()
        {
            quadBuffer.PrepareBuffer();
            lineBuffer.PrepareBuffer();
            line3DBuffer.PrepareBuffer();
            SetOrigin(0, 0);
        }

        public static void SetOrigin(float x, float y)
        {
            if (_instance == null)
                return;
            _instance._xOrigin = x;
            _instance._yOrigin = y;
        }

        public static void SetColor(Color color)
        {
            if (_instance == null)
                return;
            _instance._currentColor = color;
        }

        public static void Write(float x, float y, string format)
        {
            if (_instance == null)
            {
                return;
            }

            int l = StringFormatter.Write(ref _buf, 0, format);
            _instance.DrawText(x, y, ref _buf, l);
        }

        private void DrawText(float x, float y, ref char[] text, int length)
        {
            const string hexes = "0123456789ABCDEF";
            Vector4 color = _currentColor;
            var xPos = 0;
            if (x < 0)
                x += Width;
            if (y < 0)
                y += Height;
            for (var i = 0; i < length; i++)
            {
                if (text[i] == '^' && i < length - 3)
                {
                    int r = hexes.IndexOf(text[i + 1]);
                    int g = hexes.IndexOf(text[i + 2]);
                    int b = hexes.IndexOf(text[i + 3]);
                    color.x = (r * 16 + r) / 255f;
                    color.y = (g * 16 + g) / 255f;
                    color.z = (b * 16 + b) / 255f;
                    i += 3;
                    continue;
                }

                quadBuffer.AddQuad(_xOrigin + x + xPos, _yOrigin + y, 1, 1, text[i], color);
                ++xPos;
            }
        }

        public static void Draw()
        {
            if (_instance == null)
            {
                return;
            }

            _instance.line3DBuffer.Draw();
            _instance.lineBuffer.Draw();
            _instance.quadBuffer.Draw();
        }

        public static void HDDraw(CommandBuffer cmd)
        {
            if (_instance == null)
            {
                return;
            }

            _instance.lineBuffer.HDDraw(cmd);
            _instance.quadBuffer.HDDraw(cmd);
        }

        public static void HDDraw3D(CommandBuffer cmd)
        {
            if (_instance == null)
            {
                return;
            }

            _instance.line3DBuffer.HDDraw(cmd);
        }
    }
}