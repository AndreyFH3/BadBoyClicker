using DG.Tweening;
using GameAudio;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Core
{    
    public class ClickableObject : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Transform _catIcon;
        [SerializeField] private AudioCueId _clickCue = AudioCueId.MainObjectClick;
        private Tween tween;
        private IAudioService _audioService;
        public System.Action OnClick;

        [Inject]
        private void Construct(IAudioService audioService)
        {
            _audioService = audioService;
        }

        public void OnPointerClick(PointerEventData pointerEventData)
        {
            (_audioService ?? AudioServices.Current)?.Play(_clickCue);
            OnClick?.Invoke();
            if (tween != null)
            {
                tween.Complete();
                tween.Kill();
            }
            tween = _catIcon.DOPunchScale(Vector3.one * .1f, 0.1f);
            Debug.Log("Click!");
        }       
    }
}
