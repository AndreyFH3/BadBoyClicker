using System.Collections.Generic;
using UnityEngine;

namespace GameAudio
{
    public class AudioService : IAudioService
    {
        private const string AudioEnabledKey = "settings_audio_enabled";
        private readonly AudioConfig _config;
        private readonly AudioSource _source;
        private readonly Dictionary<AudioCueId, AudioCue> _cues = new();

        public bool IsEnabled { get; private set; }

        public AudioService(AudioConfig config)
        {
            _config = config;
            IsEnabled = PlayerPrefs.GetInt(AudioEnabledKey, 1) == 1;
            AudioServices.Current = this;
            AudioListener.volume = IsEnabled ? 1f : 0f;

            GameObject audioRoot = new GameObject("AudioService");
            Object.DontDestroyOnLoad(audioRoot);
            _source = audioRoot.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;

            if (_config == null)
            {
                return;
            }

            foreach (AudioCue cue in _config.Cues)
            {
                if (cue != null && cue.Id != AudioCueId.None)
                {
                    _cues[cue.Id] = cue;
                }
            }
        }

        public void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;
            PlayerPrefs.SetInt(AudioEnabledKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
            AudioListener.volume = enabled ? 1f : 0f;
        }

        public void Play(AudioCueId cueId)
        {
            if (!TryPrepareClip(cueId, out AudioCue cue, out AudioClip clip))
            {
                return;
            }

            _source.pitch = GetPitch(cue);
            _source.PlayOneShot(clip, GetVolume(cue));
        }

        public void Play(AudioCueId cueId, Vector3 position)
        {
            if (!TryPrepareClip(cueId, out AudioCue cue, out AudioClip clip))
            {
                return;
            }

            AudioSource.PlayClipAtPoint(clip, position, GetVolume(cue));
        }

        private bool TryPrepareClip(AudioCueId cueId, out AudioCue cue, out AudioClip clip)
        {
            cue = null;
            clip = null;

            if (!IsEnabled || _config == null || cueId == AudioCueId.None)
            {
                return false;
            }

            if (!_cues.TryGetValue(cueId, out cue))
            {
                return false;
            }

            clip = cue.GetRandomClip();
            return clip != null;
        }

        private float GetVolume(AudioCue cue)
        {
            return cue.Volume * _config.SfxVolume * _config.MasterVolume;
        }

        private static float GetPitch(AudioCue cue)
        {
            Vector2 range = cue.PitchRange;
            float min = Mathf.Min(range.x, range.y);
            float max = Mathf.Max(range.x, range.y);

            if (Mathf.Approximately(min, max))
            {
                return min;
            }

            return Random.Range(min, max);
        }
    }
}
