using UnityEngine;
using UnityEngine.Serialization;

namespace Utils
{
    [RequireComponent(typeof(Camera))]
    public class CameraSizeSetter : MonoBehaviour
    {
        private const float MinDimension = 1f;

        [Header("Reference View (9:16)")]
        [FormerlySerializedAs("_referenceResolution")]
        [SerializeField] private Vector2 _referenceAspect = new(9f, 16f);
        [FormerlySerializedAs("_phoneCameraSize")]
        [SerializeField] private float _referenceCameraSize = 5f;

        private Camera _camera;
        private int _lastScreenWidth;
        private int _lastScreenHeight;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            ApplyReferenceView();
        }

        private void Update()
        {
            if (_lastScreenWidth != Screen.width || _lastScreenHeight != Screen.height)
            {
                ApplyReferenceView();
            }
        }

        private void ApplyReferenceView()
        {
            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;

            if (_camera == null || Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            float referenceWidth = Mathf.Max(MinDimension, _referenceAspect.x);
            float referenceHeight = Mathf.Max(MinDimension, _referenceAspect.y);
            float referenceAspect = referenceWidth / referenceHeight;
            float screenAspect = (float)Screen.width / Screen.height;

            // Orthographic width = size * 2 * aspect. Scaling the size inversely
            // to the screen aspect keeps the same world width on every device.
            // The UI still uses the complete screen; only the world camera changes.
            _camera.rect = new Rect(0f, 0f, 1f, 1f);
            _camera.orthographicSize = _referenceCameraSize * referenceAspect / screenAspect;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _referenceAspect.x = Mathf.Max(MinDimension, _referenceAspect.x);
            _referenceAspect.y = Mathf.Max(MinDimension, _referenceAspect.y);
            _referenceCameraSize = Mathf.Max(0.01f, _referenceCameraSize);
        }
#endif
    }
}
