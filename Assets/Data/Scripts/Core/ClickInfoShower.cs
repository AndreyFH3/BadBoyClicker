using DG.Tweening;
using Installer.Init;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utils;
using Zenject;
namespace Core.View
{
    public class ClickInfoShower : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textReference;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private RectTransform _defaultPosition;
        private List<TextMeshProUGUI> _poolTextes = new();

        [Inject]
        public void Init(GameStartRouter router)
        {
            router.OnClickValueEvent += ShowInfo;
        }

        private void Start()
        {
            _textReference.gameObject.SetActive(false);
            _poolTextes.Add(_textReference);
        }

        private void ShowInfo(long data)
        {
            var text = _poolTextes.Find(el => !el.gameObject.activeSelf);
            if (text == null)
            {
                text = Instantiate(_textReference, _rectTransform);
                _poolTextes.Add(text);
            }
            text.gameObject.SetActive(true);
            AnimateText(text);
            text.text = $"+{data.ConvertFromLongToString()}";  
        }

        private void AnimateText(TextMeshProUGUI text)
        {
            text.transform.position = _defaultPosition.position;
            text.color = new Color(text.color.r, text.color.g, text.color.b ,1);

            text
                .DOFade(0, .25f)
                .SetEase(Ease.InQuad);

            text.transform
                .DOLocalMove(new Vector3(0, 75), .25f)
                .OnComplete(() => text.gameObject.SetActive(false))
                .SetEase(Ease.InQuad);
        }
    }
}
