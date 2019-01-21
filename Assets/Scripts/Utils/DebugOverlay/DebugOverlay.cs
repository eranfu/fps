using UnityEngine;

namespace Utils.DebugOverlay
{
    public class DebugOverlay : MonoBehaviour
    {
        private static DebugOverlay _instance;
        private static char[] _buf = new char[1024];
        public static int Width => _instance.width;
        public static int Height => _instance.height;

        [SerializeField] [Header("Overlay size")]
        private int width = 80;

        [SerializeField] private int height = 25;

        [SerializeField] [Tooltip("Number of columns of glyphs on texture")]
        private int charCols = 30;

        [SerializeField] [Tooltip("Number of rows of glyphs on texture")]
        private int charRows = 16;

        [SerializeField] [Tooltip("Width in pixels of each glyph")]
        private int cellWidth = 32;

        [SerializeField] [Tooltip("Height in pixels of each glyph")]
        private int cellHeight = 32;

        [SerializeField] private Shader lineShaderProc;

        private float _yOrigin;
        private float _xOrigin;
        private Color _currentColor = Color.white;

        private QuadInstanceData[] _quadArray = new QuadInstanceData[128];
        private LineInstanceData[] _lineArray = new LineInstanceData[128];
        private int _numQuadUsed = 0;
        private int _numLineUsed = 0;
        private Material _lineMaterial;

        private void Awake()
        {
            _lineMaterial = new Material(lineShaderProc);

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
                    color.y = (r * 16 + r) / 255f;
                    color.z = (r * 16 + r) / 255f;
                    i += 3;
                    continue;
                }

                AddQuad(_xOrigin + x + xPos, _yOrigin + y, 1, 1, text[i], color);
                ++xPos;
            }
        }

        private unsafe void AddQuad(float x, float y, float w, float h, char c, Vector4 color)
        {
            if (_numQuadUsed >= _quadArray.Length)
            {
                // resize
                var newBuf = new QuadInstanceData[_quadArray.Length + 128];
                System.Array.Copy(_quadArray, newBuf, _quadArray.Length);
                _quadArray = newBuf;
            }

            fixed (QuadInstanceData* data = &_quadArray[_numQuadUsed])
            {
                if (c != '\0')
                {
                    data->positionAndUv.z = (c - 32) % charCols;
                    data->positionAndUv.w = (c - 32) / charCols;
                    color.w = 0;
                }
                else
                {
                    data->positionAndUv.z = 0;
                    data->positionAndUv.w = 0;
                }

                data->color = color;
                data->positionAndUv.x = x;
                data->positionAndUv.y = y;
                data->size.x = w;
                data->size.y = h;
                data->size.z = 0;
                data->size.w = 0;
            }

            ++_numQuadUsed;
        }

        private struct QuadInstanceData
        {
            public Vector4 positionAndUv; // if uv are zero, don't sample
            public Vector4 size; // zw unused
            public Vector4 color;
        }

        private struct LineInstanceData
        {
            public Vector4 position; // segment from (x, y) to (z, w)
            public Vector4 color;
        }
    }
}