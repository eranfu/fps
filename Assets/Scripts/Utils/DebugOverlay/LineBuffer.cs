using UnityEngine;

namespace Utils.DebugOverlay
{
    public class LineBuffer
    {
        private static readonly int InstanceBuffer = Shader.PropertyToID("instanceBuffer");
        private static readonly int Scales = Shader.PropertyToID("scales");

        private ComputeBuffer _buffer;
        private LineInstanceData[] _lineArray = new LineInstanceData[128];
        private readonly Material _material;
        private int _numLineUsed = 0;
        private int _numLineToDraw = 0;

        public LineBuffer()
        {
            var shader = Shader.Find("Debug/LineShaderProc");
            if (shader == null)
            {
                Debug.LogError("Line buffer cannot find shader resource");
            }

            _material = new Material(shader);
        }

        public void Shutdown()
        {
            if (_buffer != null)
            {
                _buffer.Release();
                _buffer = null;
            }

            _lineArray = null;
        }

        public void PrepareBuffer()
        {
            if (_buffer == null || _buffer.count != _lineArray.Length)
            {
                _buffer?.Release();
                _buffer = new ComputeBuffer(_lineArray.Length, 16 + 16);
                _material.SetBuffer(InstanceBuffer, _buffer);
            }

            _buffer.SetData(_lineArray, 0, 0, _numLineUsed);
            _numLineToDraw = _numLineUsed;
            _numLineUsed = 0;

            var activeTexture = RenderTexture.active;
            float width;
            float height;
            if (activeTexture != null)
            {
                width = activeTexture.width;
                height = activeTexture.height;
            }
            else
            {
                width = Screen.width;
                height = Screen.height;
            }

            _material.SetVector(Scales,
                new Vector4(
                    1.0f / DebugOverlay.Width,
                    1.0f / DebugOverlay.Height,
                    1.0f / width,
                    1.0f / height
                ));
        }

        private struct LineInstanceData
        {
            public Vector4 position; // segment from (x, y) to (z, w)
            public Vector4 color;
        }
    }
}