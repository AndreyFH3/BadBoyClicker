using System.Collections;
using UnityEngine;
using Zenject;

namespace GameAudio
{
    public class MusicPlaylistPlayer : MonoBehaviour
    {
        private const string AudioEnabledKey = "settings_audio_enabled";

        [SerializeField] private AudioConfig _configOverride;
        [SerializeField] private bool _playOnStart = true;

        private AudioConfig _config;
        private AudioSource _sourceA;
        private AudioSource _sourceB;
        private Coroutine _playRoutine;
        private int _trackIndex = -1;

        [Inject]
        private void Construct(AudioConfig config)
        {
            _config = config;
        }

        private void Awake()
        {
            if (_configOverride != null)
            {
                _config = _configOverride;
            }

            _sourceA = CreateSource("MusicSourceA");
            _sourceB = CreateSource("MusicSourceB");
        }

        private void Start()
        {
            if (_playOnStart)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            Stop();
        }

        public void Play()
        {
            if (_playRoutine != null)
            {
                return;
            }

            if (_config == null || _config.Music.Tracks.Count == 0)
            {
                return;
            }

            _playRoutine = StartCoroutine(PlayPlaylist());
        }

        public void Stop()
        {
            if (_playRoutine != null)
            {
                StopCoroutine(_playRoutine);
                _playRoutine = null;
            }

            _sourceA.Stop();
            _sourceB.Stop();
        }

        private IEnumerator PlayPlaylist()
        {
            AudioSource current = _sourceA;
            AudioSource next = _sourceB;
            AudioClip currentClip = GetNextTrack();

            if (currentClip == null)
            {
                yield break;
            }

            current.clip = currentClip;
            current.volume = GetMusicVolume();
            current.Play();

            while (isActiveAndEnabled)
            {
                float waitBeforeFade = Mathf.Max(0f, currentClip.length - _config.Music.CrossfadeDuration);
                yield return new WaitForSeconds(waitBeforeFade);

                AudioClip nextClip = GetNextTrack();
                if (nextClip == null)
                {
                    yield break;
                }

                next.clip = nextClip;
                next.volume = 0f;
                if (_config.Music.DelayBetweenTracks > 0f)
                {
                    yield return new WaitForSeconds(_config.Music.DelayBetweenTracks);
                }

                next.Play();

                yield return Crossfade(current, next, _config.Music.CrossfadeDuration);

                AudioSource previous = current;
                current = next;
                next = previous;
                next.Stop();
                currentClip = nextClip;
            }
        }

        private IEnumerator Crossfade(AudioSource from, AudioSource to, float duration)
        {
            if (duration <= 0f)
            {
                from.Stop();
                to.volume = GetMusicVolume();
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float targetVolume = GetMusicVolume();
                from.volume = Mathf.Lerp(targetVolume, 0f, t);
                to.volume = Mathf.Lerp(0f, targetVolume, t);
                yield return null;
            }

            from.volume = 0f;
            to.volume = GetMusicVolume();
        }

        private AudioClip GetNextTrack()
        {
            if (_config == null || _config.Music.Tracks.Count == 0)
            {
                return null;
            }

            if (_config.Music.Shuffle)
            {
                return _config.Music.Tracks[Random.Range(0, _config.Music.Tracks.Count)];
            }

            _trackIndex = (_trackIndex + 1) % _config.Music.Tracks.Count;
            return _config.Music.Tracks[_trackIndex];
        }

        private float GetMusicVolume()
        {
            bool enabled = PlayerPrefs.GetInt(AudioEnabledKey, 1) == 1;
            return enabled ? _config.MusicVolume * _config.MasterVolume : 0f;
        }

        private AudioSource CreateSource(string sourceName)
        {
            GameObject sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(transform);

            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            return source;
        }
    }
}
