using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;

namespace Animations
{        
    public class TabMoveImage : MonoBehaviour
    {
        [SerializeField] private RectTransform _targetBackground;
        [SerializeField] private float _duration = .2f;
        [SerializeField] private int _selextedIndex = -1;
        [SerializeField] private List<Button> _buttons;

        private void Start()
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                int capturedIndex = i;
                _buttons[i].onClick.AddListener(() => SetButtonActive(capturedIndex));
            }
            Canvas.ForceUpdateCanvases();
            SetButtonActive(_selextedIndex);
        }

        private void SetButtonActive(int index)
        {
            if (index < 0 || index >= _buttons.Count)
            {
                Debug.LogError($"Index out of range: {index}, buttons count: {_buttons.Count}");
                return;
            }

            if (_buttons[index] == null || _targetBackground == null)
            {
                Debug.LogError("Button or target background is null");
                return;
            }

            var targetTransform = _buttons[index].transform as RectTransform;
            if (targetTransform == null)
            {
                Debug.LogError("Target button transform is not RectTransform");
                return;
            }
             
            _targetBackground.DOAnchorPos(targetTransform.anchoredPosition, _duration);
            _targetBackground.DOSizeDelta(targetTransform.sizeDelta, _duration);
        }
    }
}