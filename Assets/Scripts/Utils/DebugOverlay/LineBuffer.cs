using UnityEngine;

namespace Utils.DebugOverlay
{
    public class LineBuffer : BufferBase<LineInstanceData>
    {
        protected override int Stride => 16 + 16;

        public override void PrepareBuffer()
        {
            base.PrepareBuffer();

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

            material.SetVector(ShaderProperties.Scales,
                new Vector4(
                    1.0f / DebugOverlay.Width,
                    1.0f / DebugOverlay.Height,
                    1.0f / width,
                    1.0f / height
                ));
        }
    }

    public struct LineInstanceData
    {
        public Vector4 position; // segment from (x, y) to (z, w)
        public Vector4 color;
    }
}