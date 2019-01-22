using UnityEngine;
using Vector4 = System.Numerics.Vector4;

namespace Utils.DebugOverlay
{
    public class Line3DBuffer
    {
        private Material _material;
        private ComputeBuffer _buffer;
        private Lien3DInstanceData[] _dataList = new Lien3DInstanceData[128];

        public Line3DBuffer()
        {
            var shader = Shader.Find("Debug/Line3DShaderProc");
            if (shader == null)
            {
                Debug.LogError("Line3DBuffer can not find shader resource");
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

        }

        private struct Lien3DInstanceData
        {
            public Vector4 start;
            public Vector4 end;
            public Vector4 color;
        }
    }
}