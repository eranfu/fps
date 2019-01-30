using UnityEngine;

namespace Utils.DebugOverlay
{
    public class Line3DBuffer : BufferBase<Line3DInstanceData>
    {
        protected override int Stride => 16 + 16 + 16;

        public void AddLine(Vector3 start, Vector3 end, Vector4 color)
        {
            unsafe
            {
                fixed (Line3DInstanceData* data = &DataArray[AddData()])
                {
                    data->start = start;
                    data->start.w = 1;
                    data->end = end;
                    data->end.w = 1;
                    data->color = color;
                }
            }
        }
    }

    public struct Line3DInstanceData
    {
        public Vector4 start;
        public Vector4 end;
        public Vector4 color;
    }
}