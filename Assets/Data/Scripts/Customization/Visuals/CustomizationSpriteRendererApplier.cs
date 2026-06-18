using UnityEngine;
using Zenject;

namespace Customization
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CustomizationSpriteRendererApplier : MonoBehaviour
    {
        [SerializeField] private CustomizationItemType _type;
        [SerializeField] private SpriteRenderer _target;

        private ICustomizationService _service;
        private bool _isSubscribed;

        [Inject]
        private void Construct(ICustomizationService service)
        {
            _service = service;
            TrySubscribeAndApply();
        }

        private void Awake()
        {
            if (_target == null)
            {
                _target = GetComponent<SpriteRenderer>();
            }
        }

        private void OnEnable()
        {
            TrySubscribeAndApply();
        }

        private void OnDisable()
        {
            if (_service != null && _isSubscribed)
            {
                _service.ActiveItemChanged -= OnActiveItemChanged;
                _isSubscribed = false;
            }
        }

        public void Apply()
        {
            if (_target == null || _service == null)
            {
                return;
            }

            Sprite sprite = _type == CustomizationItemType.Background
                ? _service.ActiveBackgroundSprite
                : _service.ActiveCatSprite;

            if (sprite != null)
            {
                _target.sprite = sprite;
            }
        }

        private void OnActiveItemChanged(CustomizationItemType type, string id)
        {
            if (type == _type)
            {
                Apply();
            }
        }

        private void TrySubscribeAndApply()
        {
            if (_service == null || !isActiveAndEnabled)
            {
                return;
            }

            if (!_isSubscribed)
            {
                _service.ActiveItemChanged += OnActiveItemChanged;
                _isSubscribed = true;
            }

            Apply();
        }
    }
}
