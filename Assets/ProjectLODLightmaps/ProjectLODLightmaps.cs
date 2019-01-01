using UnityEngine;


namespace ProjectLODLightmaps
{
    public class ProjectLODLightmaps : MonoBehaviour
    {
        public new Renderer renderer;

        public void SetupRenderer()
        {
            var r = GetComponent<Renderer>();
            if (r && this.renderer)
            {
                r.lightmapScaleOffset = this.renderer.lightmapScaleOffset;
                r.lightmapIndex = this.renderer.lightmapIndex;
            }
        }
    }
}