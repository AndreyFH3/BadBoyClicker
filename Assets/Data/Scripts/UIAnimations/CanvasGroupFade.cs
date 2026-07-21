using System;
using DG.Tweening;
using UnityEngine;

namespace UIAnimations
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class CanvasGroupFade : MonoBehaviour
    {
        [Min(0f)] [SerializeField] private float _duration = 0.2f;

        private CanvasGroup _canvasGroup;
        private Tween _fadeTween;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnDestroy()
        {
            KillTween();
        }

        public void Show()
        {
            EnsureInitialized();
            KillTween();

            bool wasActive = gameObject.activeSelf;
            gameObject.SetActive(true);

            if (!wasActive)
            {
                _canvasGroup.alpha = 0f;
            }

            SetInteraction(false);

            if (_duration <= 0f)
            {
                CompleteShow();
                return;
            }

            _fadeTween = _canvasGroup
                .DOFade(1f, _duration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .OnComplete(CompleteShow);
        }

        public void Hide(Action onHidden = null)
        {
            EnsureInitialized();
            KillTween();
            SetInteraction(false);

            if (!gameObject.activeSelf || _duration <= 0f)
            {
                HideImmediately();
                onHidden?.Invoke();
                return;
            }

            _fadeTween = _canvasGroup
                .DOFade(0f, _duration)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    HideImmediately();
                    onHidden?.Invoke();
                });
        }

        public void HideImmediately()
        {
            EnsureInitialized();
            KillTween();
            _canvasGroup.alpha = 0f;
            SetInteraction(false);
            gameObject.SetActive(false);
        }

        private void CompleteShow()
        {
            _fadeTween = null;
            _canvasGroup.alpha = 1f;
            SetInteraction(true);
        }

        private void SetInteraction(bool enabled)
        {
            _canvasGroup.interactable = enabled;
            _canvasGroup.blocksRaycasts = enabled;
        }

        private void EnsureInitialized()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void KillTween()
        {
            if (_fadeTween == null)
            {
                return;
            }

            _fadeTween.Kill();
            _fadeTween = null;
        }
    }
}
