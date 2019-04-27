using UnityEditor;
using UnityEngine;

namespace Audio.Editor
{
    [CustomEditor(typeof(SoundDef))]
    public class SoundDefEditor : UnityEditor.Editor
    {
        private static AudioSource _audioSource;

        public override void OnInspectorGUI()
        {
            if (_audioSource == null)
            {
                var go = new GameObject("TestAudioSource") {hideFlags = HideFlags.HideAndDontSave};
                _audioSource = go.AddComponent<AudioSource>();
            }

            bool oldGuiEnabled = GUI.enabled;
            GUI.enabled = true;
            if (_audioSource.isPlaying)
            {
                if (GUILayout.Button("Stop []"))
                {
                    _audioSource.Stop();
                }
            }
            else
            {
                if (GUILayout.Button("Play >"))
                {
                    var soundDef = (SoundDef) target;
                    SoundSystem.StartSource(_audioSource, soundDef);
                }
            }

            GUI.enabled = oldGuiEnabled;

            base.OnInspectorGUI();
        }
    }
}