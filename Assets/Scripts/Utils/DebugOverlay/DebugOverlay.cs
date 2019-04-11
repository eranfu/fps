#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using UnityEngine.Rendering;
using Utils.Pool;

namespace Utils.DebugOverlay
{
    public class DebugOverlay : MonoBehaviour
    {
        private static DebugOverlay _instance;
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

        public static void WriteString(float x, float y, string text)
        {
            if (_instance == null)
            {
                return;
            }

            const string hexes = "0123456789ABCDEF";
            Vector4 color = _instance._currentColor;
            var xPos = 0;
            if (x < 0)
                x += Width;
            if (y < 0)
                y += Height;
            int length = text.Length;
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

                _instance.quadBuffer.AddQuad(_instance._xOrigin + x + xPos, _instance._yOrigin + y, 1, 1, text[i],
                    color);
                ++xPos;
            }
        }

        public static void WriteString(Color color, float x, float y, string text)
        {
            Color old = _instance._currentColor;
            _instance._currentColor = color;
            WriteString(x, y, text);
            _instance._currentColor = old;
        }

        public static void WriteAbsolute(float x, float y, float size, char[] buf, int length)
        {
            if (_instance == null)
                return;

            float scaleX = (float) _instance.width / Screen.width;
            float scaleY = (float) _instance.height / Screen.height;
            x *= scaleX;
            y *= scaleY;
            float sizeX = size * scaleX;
            float sizeY = size * 1.5f * scaleY;
            for (var i = 0; i < length; i++)
            {
                _instance.quadBuffer.AddQuad(x + i * sizeX, y, sizeX, sizeY, buf[i], _instance._currentColor);
            }
        }

        public static void AddQuadAbsolute(float x, float y, float width, float height, char c, Vector4 color)
        {
            if (_instance == null)
                return;
            float scaleX = (float) _instance.width / Screen.width;
            float scaleY = (float) _instance.height / Screen.height;
            x *= scaleX;
            y *= scaleY;
            width *= scaleX;
            height *= scaleY;
            _instance.quadBuffer.AddQuad(x, y, width, height, c, color);
        }

        public static void DrawHistogram(int x, int y, int width, int height, float[] data, int startSample,
            Color color, float maxRange = -1)
        {
            if (_instance == null)
                return;
            float[][] dataSet = Pools.Buffer.Pop<float[]>(1);
            Color[] colors = Pools.Buffer.Pop<Color>(1);
            dataSet[0] = data;
            colors[0] = color;
            DrawHistogram(x, y, width, height, dataSet, startSample, colors, maxRange);
            dataSet[0] = null;
            Pools.Buffer.Push(colors);
            Pools.Buffer.Push(dataSet);
        }

        public static void DrawHistogram(float x, float y, float width, float height, float[][] data, int startSample,
            Color[] colors, float maxRange = -1)
        {
            if (_instance == null)
                return;

            if (data == null || data.Length == 0 || data[0] == null)
                throw new ArgumentException(
                    "Invalid data argument (data must contain at least one non null array");

            if (x < 0) x += Width;
            if (y < 0) y += Height;

            int numSamples = data[0].Length;
            for (var i = 1; i < data.Length; ++i)
            {
                if (data[i] == null || data[i].Length != numSamples)
                    throw new ArgumentException("Length of data of all arrays must be the same");
            }

            if (colors.Length != data.Length)
                throw new ArgumentException("Length of colors must match number of data sets");

            float maxData = float.MinValue;

            // Find tallest stack of values
            for (var i = 0; i < numSamples; i++)
            {
                float sum = 0;

                foreach (float[] dataSet in data)
                    sum += dataSet[i];

                if (sum > maxData)
                    maxData = sum;
            }

            if (maxData > maxRange)
                maxRange = maxData;

            float dx = width / numSamples;
            float scale = maxRange > 0 ? height / maxRange : 1.0f;

            for (var i = 0; i < numSamples; i++)
            {
                float stackOffset = 0;
                for (var j = 0; j < data.Length; j++)
                {
                    Color c = colors[j];
                    float d = data[j][(i + startSample) % numSamples];
                    float barHeight = d * scale; // now in [0, h]
                    float posX = _instance._yOrigin + x + dx * i;
                    float posY = _instance._xOrigin + y + height - barHeight - stackOffset;
                    stackOffset += barHeight;
                    _instance.quadBuffer.AddQuad(posX, posY, dx, barHeight, '\0', new Vector4(c.r, c.g, c.b, c.a));
                }
            }
        }

        public static void DrawGraph(float x, float y, float w, float h, float[] data, int startSample, Color color,
            float maxRange = -1.0f)
        {
            if (_instance == null)
                return;
            float[][] dataSet = Pools.Buffer.Pop<float[]>(1);
            Color[] colors = Pools.Buffer.Pop<Color>(1);
            dataSet[0] = data;
            colors[0] = color;
            DrawGraph(x, y, w, h, dataSet, startSample, colors, maxRange);
            dataSet[0] = null;
            Pools.Buffer.Push(dataSet);
            Pools.Buffer.Push(colors);
        }

        public static void DrawGraph(
            float x, float y, float w, float h, float[][] data, int startSample, Color[] color, float maxRange = -1.0f)
        {
            if (_instance == null)
                return;

            if (data == null || data.Length == 0 || data[0] == null)
                throw new ArgumentException(
                    "Invalid data argument (data must contain at least one non null array");

            int numSamples = data[0].Length;
            for (var i = 1; i < data.Length; ++i)
            {
                if (data[i] == null || data[i].Length != numSamples)
                    throw new ArgumentException("Length of data of all arrays must be the same");
            }

            if (color.Length != data.Length)
                throw new ArgumentException("Length of colors must match number of datasets");

            float maxData = float.MinValue;

            foreach (float[] dataSet in data)
            {
                for (var i = 0; i < numSamples; i++)
                {
                    if (dataSet[i] > maxData)
                        maxData = dataSet[i];
                }
            }

            if (maxData > maxRange)
                maxRange = maxData;

            float dx = w / numSamples;
            float scale = maxRange > 0 ? h / maxRange : 1.0f;

            for (var j = 0; j < data.Length; j++)
            {
                float oldPosX = 0;
                float oldPosY = 0;
                Vector4 col = color[j];
                for (var i = 0; i < numSamples; i++)
                {
                    float d = data[j][(i + startSample) % numSamples];
                    float posX = _instance._xOrigin + x + dx * i;
                    float posY = _instance._yOrigin + y + h - d * scale;
                    if (i > 0)
                        _instance.lineBuffer.AddLine(oldPosX, oldPosY, posX, posY, col);
                    oldPosX = posX;
                    oldPosY = posY;
                }
            }

            _instance.lineBuffer.AddLine(x, y + h, x + w, y + h, color[0]);
            _instance.lineBuffer.AddLine(x, y, x, y + h, color[0]);
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