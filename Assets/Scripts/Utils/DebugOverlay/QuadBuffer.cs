using System;
using UnityEngine;

namespace Utils.DebugOverlay
{
    [Serializable]
    public class QuadBuffer : BufferBase<QuadInstanceData>
    {
        [SerializeField] [Tooltip("Number of columns of glyphs on texture")]
        private int charCols = 30;

        [SerializeField] [Tooltip("Number of rows of glyphs on texture")]
        private int charRows = 16;

        [SerializeField] [Tooltip("Width in pixels of each glyph")]
        private int cellWidth = 32;

        [SerializeField] [Tooltip("Height in pixels of each glyph")]
        private int cellHeight = 32;

        public unsafe void AddQuad(float x, float y, float w, float h, char c, Vector4 color)
        {
            fixed (QuadInstanceData* data = &DataArray[AddData()])
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
        }

        protected override int Stride => 16 + 16 + 16;

        public override void PrepareBuffer()
        {
            base.PrepareBuffer();
            Material.SetVector(ShaderProperties.Scales,
                new Vector4(
                    1.0f / DebugOverlay.Width, 1.0f / DebugOverlay.Height,
                    1 / 0f / Material.mainTexture.width, 1.0f / Material.mainTexture.height
                ));
        }
    }

    public struct QuadInstanceData
    {
        public Vector4 positionAndUv; // if uv are zero, don't sample
        public Vector4 size; // zw unused
        public Vector4 color;
    }
}