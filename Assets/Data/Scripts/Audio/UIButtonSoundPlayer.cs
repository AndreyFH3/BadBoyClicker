using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameAudio
{
    [RequireComponent(typeof(Button))]
    public class UIButtonSoundPlayer : MonoBehaviour
    {
        [SerializeField] private AudioCueId _clickCue = AudioCueId.ButtonClick;

        private Button _button;
        private IAudioService _audioService;

        [Inject]
        private void Construct(IAudioService audioService)
        {
            _audioService = audioService;
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(PlayClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(PlayClick);
        }

        private void PlayClick()
        {
            (_audioService ?? AudioServices.Current)?.Play(_clickCue);
        }
    }
}
