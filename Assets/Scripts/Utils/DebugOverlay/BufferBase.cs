using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Utils.DebugOverlay
{
    [Serializable]
    public abstract class BufferBase<T> where T : struct
    {
        [SerializeField] private Shader shader;

        protected readonly Material material;
        private ComputeBuffer _buffer;
        private int _numDataUsed = 0;
        private int _numDataToDraw = 0;
        protected abstract int Stride { get; }
        protected T[] DataArray { get; private set; } = new T[128];

        protected BufferBase()
        {
            if (shader == null)
            {
                Debug.LogError("Can not find shader resource");
            }

            material = new Material(shader);
        }

        public virtual void PrepareBuffer()
        {
            if (_buffer == null || _buffer.count != DataArray.Length)
            {
                _buffer?.Release();
                _buffer = new ComputeBuffer(DataArray.Length, Stride);
                material.SetBuffer(ShaderProperties.InstanceBuffer, _buffer);
            }

            _buffer.SetData(DataArray, 0, 0, _numDataUsed);
            _numDataToDraw = _numDataUsed;
            _numDataUsed = 0;
        }

        public void Draw()
        {
            material.SetPass(0);
            Graphics.DrawProcedural(MeshTopology.Triangles, _numDataToDraw * 6, 1);
        }

        public void HDDraw(CommandBuffer cmd)
        {
            cmd.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Triangles, _numDataToDraw * 6, 1);
        }

        public void Shutdown()
        {
            if (_buffer != null)
            {
                _buffer.Release();
                _buffer = null;
            }

            DataArray = null;
        }

        protected int AddData()
        {
            if (_numDataUsed >= DataArray.Length)
            {
                var newList = new T[DataArray.Length + 128];
                Array.Copy(DataArray, newList, DataArray.Length);
                DataArray = newList;
            }

            return _numDataUsed++;
        }
    }

    public static class ShaderProperties
    {
        public static readonly int InstanceBuffer = Shader.PropertyToID("instanceBuffer");
        public static readonly int Scales = Shader.PropertyToID("scales");
    }
}