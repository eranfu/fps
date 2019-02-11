#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Profiling;
using UnityEditor;

namespace ProjectLODLightmaps
{
    [ExecuteInEditMode]
    public class ProjectLODLightmapManager : MonoBehaviour
    {
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Lightmapping.completed += SetupRenderer;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            Lightmapping.completed -= SetupRenderer;
        }

        private void Start()
        {
            SetupRenderer();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                SetupRenderer();
            }
        }

        private void SetupRenderer()
        {
            Profiler.BeginSample("ProjectLODLightmapManager.SetupRender");
            ProjectLODLightmaps[] projectors = FindObjectsOfType<ProjectLODLightmaps>();
            foreach (ProjectLODLightmaps projector in projectors)
            {
                projector.SetupRenderer();
            }

            Profiler.EndSample();
        }
    }
}

#endif