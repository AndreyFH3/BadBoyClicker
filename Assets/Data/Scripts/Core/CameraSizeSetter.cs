using UnityEngine;

namespace Utils
{
    public class CameraSizeSetter : MonoBehaviour
    {
        private Camera _camera;

        [Header("Camera Sizes")]
        [SerializeField] private float _phoneCameraSize = 5f;
        [SerializeField] private float _tabletCameraSize = 4f;

        [Header("Aspect Threshold")]
        [SerializeField] private float _tabletAspectThreshold = 1.5f;

        private void Awake()
        {
            ApplyCameraSize();
        }

        private void ApplyCameraSize()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_camera == null)
            {
                Debug.LogError("Camera not found");
                return;
            }

            float aspect = (float)Screen.height / Screen.width;

            bool isTablet = aspect < _tabletAspectThreshold;

            _camera.orthographicSize = isTablet
                ? _tabletCameraSize
                : _phoneCameraSize;
        }
    }
}