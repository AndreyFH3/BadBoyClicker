using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameAudio
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Configs/Audio")]
    public class AudioConfig : ScriptableObject
    {
        [SerializeField] private List<AudioCue> _cues = new();
        [SerializeField] private MusicPlaylist _music = new();
        [SerializeField, Range(0f, 1f)] private float _masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _sfxVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _musicVolume = 0.6f;

        public IReadOnlyList<AudioCue> Cues => _cues;
        public MusicPlaylist Music => _music;
        public float MasterVolume => _masterVolume;
        public float SfxVolume => _sfxVolume;
        public float MusicVolume => _musicVolume;

        public bool TryGetCue(AudioCueId id, out AudioCue cue)
        {
            for (int i = 0; i < _cues.Count; i++)
            {
                if (_cues[i].Id == id)
                {
                    cue = _cues[i];
                    return true;
                }
            }

            cue = null;
            return false;
        }
    }

    [Serializable]
    public class AudioCue
    {
        [SerializeField] private AudioCueId _id;
        [SerializeField] private List<AudioClip> _clips = new();
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [SerializeField] private Vector2 _pitchRange = Vector2.one;

        public AudioCueId Id => _id;
        public IReadOnlyList<AudioClip> Clips => _clips;
        public float Volume => _volume;
        public Vector2 PitchRange => _pitchRange;

        public AudioClip GetRandomClip()
        {
            if (_clips.Count == 0)
            {
                return null;
            }

            return _clips[UnityEngine.Random.Range(0, _clips.Count)];
        }
    }

    [Serializable]
    public class MusicPlaylist
    {
        [SerializeField] private List<AudioClip> _tracks = new();
        [SerializeField] private bool _shuffle;
        [SerializeField] private float _crossfadeDuration = 1.5f;
        [SerializeField] private float _delayBetweenTracks;

        public IReadOnlyList<AudioClip> Tracks => _tracks;
        public bool Shuffle => _shuffle;
        public float CrossfadeDuration => Mathf.Max(0f, _crossfadeDuration);
        public float DelayBetweenTracks => Mathf.Max(0f, _delayBetweenTracks);
    }
}
