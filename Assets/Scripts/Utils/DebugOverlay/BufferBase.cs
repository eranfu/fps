using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Utils.DebugOverlay
{
    [Serializable]
    public abstract class BufferBase<T> where T : struct
    {
        private ComputeBuffer _buffer;
        private int _numDataToDraw = 0;
        private int _numDataUsed = 0;
        [SerializeField] private Material material;

        protected Material Material => material;
        protected abstract int Stride { get; }
        protected T[] DataArray { get; private set; } = new T[128];

        public virtual void PrepareBuffer()
        {
            if (_buffer == null || _buffer.count != DataArray.Length)
            {
                _buffer?.Release();
                _buffer = new ComputeBuffer(DataArray.Length, Stride);
                Material.SetBuffer(ShaderProperties.InstanceBuffer, _buffer);
            }

            _buffer.SetData(DataArray, 0, 0, _numDataUsed);
            _numDataToDraw = _numDataUsed;
            _numDataUsed = 0;
        }

        public void Draw()
        {
            Material.SetPass(0);
            Graphics.DrawProcedural(MeshTopology.Triangles, _numDataToDraw * 6, 1);
        }

        public void HDDraw(CommandBuffer cmd)
        {
            cmd.DrawProcedural(Matrix4x4.identity, Material, 0, MeshTopology.Triangles, _numDataToDraw * 6, 1);
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