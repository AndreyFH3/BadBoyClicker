using UnityEngine;

namespace GameAudio
{
    public interface IAudioService
    {
        bool IsEnabled { get; }
        void SetEnabled(bool enabled);
        void Play(AudioCueId cueId);
        void Play(AudioCueId cueId, Vector3 position);
    }
}
