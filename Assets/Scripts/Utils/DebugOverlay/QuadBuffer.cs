using System;
using UnityEngine;

namespace Utils.DebugOverlay
{
    [Serializable]
    public class QuadBuffer
    {
        private static readonly int InstanceBuffer = Shader.PropertyToID("instanceBuffer");
        private static readonly int Scales = Shader.PropertyToID("scales");

        [SerializeField] [Tooltip("Number of columns of glyphs on texture")]
        private int charCols = 30;

        [SerializeField] [Tooltip("Number of rows of glyphs on texture")]
        private int charRows = 16;

        [SerializeField] [Tooltip("Width in pixels of each glyph")]
        private int cellWidth = 32;

        [SerializeField] [Tooltip("Height in pixels of each glyph")]
        private int cellHeight = 32;

        private ComputeBuffer _buffer;
        private QuadInstanceData[] _quadArray = new QuadInstanceData[128];
        private int _numQuadUsed = 0;
        private int _numQuadToDraw = 0;
        private readonly Material _material;

        public QuadBuffer()
        {
            var shader = Shader.Find("Debug/GlyphShaderProc");
            if (shader == null)
            {
                Debug.LogError("QuadBuffer Cannot find shader resource");
            }

            _material = new Material(shader);
        }

        public unsafe void AddQuad(float x, float y, float w, float h, char c, Vector4 color)
        {
            if (_numQuadUsed >= _quadArray.Length)
            {
                // resize
                var newBuf = new QuadInstanceData[_quadArray.Length + 128];
                Array.Copy(_quadArray, newBuf, _quadArray.Length);
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

        public void Shutdown()
        {
            if (_buffer != null)
            {
                _buffer.Release();
                _buffer = null;
            }

            _quadArray = null;
        }

        public void PrepareBuffer()
        {
            if (_buffer == null || _buffer.count != _quadArray.Length)
            {
                _buffer?.Release();
                _buffer = new ComputeBuffer(_quadArray.Length, 16 + 16 + 16);
                _material.SetBuffer(InstanceBuffer, _buffer);
            }

            _buffer.SetData(_quadArray, 0, 0, _numQuadUsed);
            _numQuadToDraw = _numQuadUsed;
            _numQuadUsed = 0;

            _material.SetVector(Scales,
                new Vector4(
                    1.0f / DebugOverlay.Width, 1.0f / DebugOverlay.Height,
                    1 / 0f / _material.mainTexture.width, 1.0f / _material.mainTexture.height
                ));
        }

        private struct QuadInstanceData
        {
            public Vector4 positionAndUv; // if uv are zero, don't sample
            public Vector4 size; // zw unused
            public Vector4 color;
        }
    }
}