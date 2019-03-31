using System.Collections.Generic;
using Game.Core;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Profiling;
using Utils;

namespace Audio
{
    public class SoundSystem : ISoundSystem
    {
        private const float SoundVolCutoff = -60;

        [ConfigVar(name = "sound.numemitters", defaultValue = "48", description = "Number of sound emitters.")]
        private static ConfigVar _soundNumEmitters;

        [ConfigVar(name = "sound.spatialize", defaultValue = "1", description = "If use spatializer")]
        private static ConfigVar _soundSpatialize;

        private static AudioMixerGroup[] _mixerGroups;
        private readonly Dictionary<string, SoundDef> _soundDefList = new Dictionary<string, SoundDef>();

        private AudioMixer _audioMixer;
        private AudioListener _currentListener;
        private SoundEmitter[] _emitters;
        private int _sequenceId;
        private GameObject _sourceHolder;

        public void Init(AudioMixer audioMixer)
        {
            _sourceHolder = new GameObject("SoundSystemSources");
            Object.DontDestroyOnLoad(_sourceHolder);
            _audioMixer = audioMixer;
            GameDebug.Log($"SoundSystem using mixer: {audioMixer.name}");
            _sequenceId = 0;

            // create pool of emitters
            _emitters = new SoundEmitter[_soundNumEmitters.IntValue];
            for (var i = 0; i < _soundNumEmitters.IntValue; i++)
            {
                var emitter = new SoundEmitter
                {
                    source = MakeAudioSource(),
                    fadeToKill = new Interpolator(1.0f, Interpolator.CurveType.Linear)
                };
                _emitters[i] = emitter;
            }

            // setup mixer groups
            _mixerGroups = new AudioMixerGroup[(int) SoundMixerGroup.GroupCount];
            _mixerGroups[(int) SoundMixerGroup.Music] = _audioMixer.FindMatchingGroups("Music")[0];
            _mixerGroups[(int) SoundMixerGroup.SFX] = _audioMixer.FindMatchingGroups("SFX")[0];
            _mixerGroups[(int) SoundMixerGroup.Menu] = _audioMixer.FindMatchingGroups("Menu")[0];
        }

        public void MountBank(SoundBank bank)
        {
            Debug.Assert(bank.soundDefGuidList.Count == bank.soundDefList.Count);
            for (var i = 0; i < bank.soundDefGuidList.Count; i++)
            {
                _soundDefList[bank.soundDefGuidList[i]] = bank.soundDefList[i];
            }

            GameDebug.Log($"Mounted sound bank: {bank.name} with {bank.soundDefGuidList.Count} sounds");
        }

        public SoundHandle Play(SoundDef soundDef)
        {
            Debug.Assert(soundDef != null);
            SoundEmitter e = AllocEmitter();
            if (e == null)
                return new SoundHandle(null);

            if (soundDef.spatialBlend > 0.0f)
            {
                Debug.LogWarning($"Play 3D {soundDef.name} sound at (0, 0, 0)");
            }

            e.source.transform.position = Vector3.zero;
            e.repeatCount = Random.Range(soundDef.repeatMin, soundDef.repeatMax);
            e.playing = true;
            e.soundDef = soundDef;
            StartEmitter(e);
            return new SoundHandle(e);
        }

        private void StartEmitter(SoundEmitter emitter)
        {
            StartSource(emitter.source, emitter.soundDef);
        }

#if UNITY_EDITOR
        public static void StartSource(AudioSource source, SoundDef soundDef)
#else
        private static void StartSource(AudioSource source, SoundDef soundDef)  
#endif
        {
            Profiler.BeginSample(".Set source clip");
            source.clip = soundDef.clips[Random.Range(0, soundDef.clips.Length)];
            Profiler.EndSample();

            Profiler.BeginSample(".Setup source");
            // Map from halftone space to linear playback multiplier
            source.pitch = Mathf.Pow(2.0f, Random.Range(soundDef.pitchMin, soundDef.pitchMax) / 12.0f);
            source.minDistance = soundDef.distMin;
            source.maxDistance = soundDef.distMax;
            source.volume = AmplitudeFromDecibel(soundDef.volume);
            source.loop = soundDef.loopCount < 1;
            source.rolloffMode = soundDef.rolloffMode;
            if (_mixerGroups != null)
                source.outputAudioMixerGroup = _mixerGroups[(int) soundDef.soundGroup];
            source.spatialBlend = soundDef.spatialBlend;
            source.panStereo = Random.Range(soundDef.panMin, soundDef.panMax);
            Profiler.EndSample();

            Profiler.BeginSample(".Setup spatializer");
            if (_soundSpatialize != null && _soundSpatialize.IntValue > 0 && soundDef.spatialBlend > 0.5f)
            {
                source.spatialize = true;
                source.SetSpatializerFloat(0, 8.0f);
                source.SetSpatializerFloat(1, 0.0f);
                source.SetSpatializerFloat(4, 0.0f);
                source.SetSpatializerFloat(5, 0.0f);
                source.spatializePostEffects = false;
            }
            else
            {
                source.spatialize = false;
            }

            Profiler.EndSample();

            if (!source.enabled)
            {
                GameDebug.Log("Fix disabled sound source");
                source.enabled = true;
            }

            Profiler.BeginSample("AudioSource.Play");
            float delay = Random.Range(soundDef.delayMin, soundDef.delayMax);
            if (delay > 0)
            {
                source.PlayDelayed(delay);
            }
            else
            {
                source.Play();
            }

            Profiler.EndSample();
        }

        private static float AmplitudeFromDecibel(float decibel)
        {
            if (decibel <= SoundVolCutoff)
            {
                return 0;
            }

            return Mathf.Pow(2.0f, decibel / 6);
        }

        private SoundEmitter AllocEmitter()
        {
            // Look for unused emitter
            for (var i = 0; i < _emitters.Length; i++)
            {
                SoundEmitter e = _emitters[i];
                if (!e.playing)
                {
                    e.seqId = _sequenceId++;
                    return e;
                }
            }

            // Hunt down farthest emitter to kill
            SoundEmitter emitter = null;
            float maxDist = float.MinValue;
            Vector3 listenerPos = _currentListener != null ? _currentListener.transform.position : Vector3.zero;
            for (var i = 0; i < _emitters.Length; i++)
            {
                SoundEmitter e = _emitters[i];
                AudioSource source = e.source;
                if (source == null)
                {
                    GameDebug.LogWarning("Sound emitter had its audio source destroyed. Making a new.");
                    e.source = MakeAudioSource();
                    e.repeatCount = 0;
                    source = e.source;
                }

                if (source.loop)
                {
                    continue;
                }

                var dist = 0.0f;
                if (source.spatialBlend > 0.0f)
                {
                    Transform t = source.transform;
                    dist = (t.position - listenerPos).magnitude;
                    if (t.parent != _sourceHolder.transform)
                    {
                        dist *= 0.5f;
                    }
                }

                if (dist > maxDist)
                {
                    maxDist = dist;
                    emitter = e;
                }
            }

            if (emitter != null)
            {
                emitter.Kill();
                emitter.seqId = _sequenceId++;
                return emitter;
            }

            GameDebug.Log("Unable to allocate sound emitter!");
            return null;
        }

        private AudioSource MakeAudioSource()
        {
            var go = new GameObject("SoundSystemSource");
            go.transform.SetParent(_sourceHolder.transform);
            return go.AddComponent<AudioSource>();
        }
    }
}