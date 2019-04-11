using System;
using UnityEngine;

namespace Utils.DebugOverlay
{
    [Serializable]
    public class LineBuffer : BufferBase<LineInstanceData>
    {
        protected override int Stride => 16 + 16;

        public override void PrepareBuffer()
        {
            base.PrepareBuffer();

            RenderTexture activeTexture = RenderTexture.active;
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

            Material.SetVector(ShaderProperties.Scales,
                new Vector4(
                    1.0f / DebugOverlay.Width,
                    1.0f / DebugOverlay.Height,
                    1.0f / width,
                    1.0f / height
                ));
        }

        public unsafe void AddLine(float x1, float y1, float x2, float y2, Vector4 col)
        {
            fixed (LineInstanceData* d = &DataArray[AddData()])
            {
                d->color = col;
                d->position.x = x1;
                d->position.y = y1;
                d->position.z = x2;
                d->position.w = y2;
            }
        }
    }

    public struct LineInstanceData
    {
        public Vector4 position; // segment from (x, y) to (z, w)
        public Vector4 color;
    }
}