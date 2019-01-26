using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Utils.DebugOverlay
{
    public class Line3DBuffer
    {
        private static readonly int InstanceBuffer = Shader.PropertyToID("instanceBuffer");

        private readonly Material _material;
        private ComputeBuffer _buffer;
        private Line3DInstanceData[] _lineList = new Line3DInstanceData[128];
        private int _numLinesUsed = 0;
        private int _numLinesToDraw = 0;

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

            _lineList = null;
        }

        public void PrepareBuffer()
        {
            if (_buffer == null || _buffer.count != _lineList.Length)
            {
                _buffer?.Release();
                _buffer = new ComputeBuffer(_lineList.Length, 16 + 16 + 16);
                _material.SetBuffer(InstanceBuffer, _buffer);
            }

            _buffer.SetData(_lineList, 0, 0, _numLinesUsed);
            _numLinesToDraw = _numLinesUsed;
            _numLinesUsed = 0;
        }

        public void Draw()
        {
            _material.SetPass(0);
            Graphics.DrawProcedural(MeshTopology.Triangles, _numLinesToDraw * 6, 1);
        }

        public void HDDraw(CommandBuffer cmd)
        {
            cmd.DrawProcedural(Matrix4x4.identity, _material, 0, MeshTopology.Triangles, _numLinesToDraw * 6, 1);
        }

        public void AddLine(Vector3 start, Vector3 end, Vector4 color)
        {
            if (_numLinesUsed >= _lineList.Length)
            {
                var newList = new Line3DInstanceData[_lineList.Length + 128];
                Array.Copy(_lineList, newList, _lineList.Length);
                _lineList = newList;
            }

            unsafe
            {
                fixed (Line3DInstanceData* d = &_lineList[_numLinesUsed])
                {
                    d->start = start;
                    d->start.w = 1;
                    d->end = end;
                    d->end.w = 1;
                    d->color = color;
                }
            }

            ++_numLinesUsed;
        }

        private struct Line3DInstanceData
        {
            public Vector4 start;
            public Vector4 end;
            public Vector4 color;
        }
    }
}