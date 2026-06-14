using UnityEngine;
using Zenject;

namespace GameAudio
{
    public class AudioCuePlayer : MonoBehaviour
    {
        [SerializeField] private AudioCueId _cue = AudioCueId.ButtonClick;
        [SerializeField] private bool _playAtTransformPosition;

        private IAudioService _audioService;

        [Inject]
        private void Construct(IAudioService audioService)
        {
            _audioService = audioService;
        }

        public void Play()
        {
            if (_playAtTransformPosition)
            {
                (_audioService ?? AudioServices.Current)?.Play(_cue, transform.position);
                return;
            }

            (_audioService ?? AudioServices.Current)?.Play(_cue);
        }
    }
}
