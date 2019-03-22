using Core;
using Game.Core;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{
    public class SoundSystem : ISoundSystem
    {
        private GameObject _sourceHolder;
        private AudioMixer _audioMixer;
        private int _sequenceId;

        [ConfigVar(name = "sound.numemitters", defaultValue = "48", description = "Number of sound emitters.")]
        private static ConfigVar soundNumEmitters;

        private SoundEmitter[] _emitters;

        public void Init(AudioMixer audioMixer)
        {
            _sourceHolder = new GameObject("SoundSystemSources");
            Object.DontDestroyOnLoad(_sourceHolder);
            _audioMixer = audioMixer;
            GameDebug.Log($"SoundSystem using mixer: {audioMixer.name}");
            _sequenceId = 0;

            // create pool of emitters
            _emitters = new SoundEmitter[soundNumEmitters.IntValue];
            for (var i = 0; i < soundNumEmitters.IntValue; i++)
            {
                
            }
        }

        private class SoundEmitter
        {
        }
    }
}