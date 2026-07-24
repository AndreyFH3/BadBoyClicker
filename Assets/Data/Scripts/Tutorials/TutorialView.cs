using System;
using System.Collections.Generic;
using UIAnimations;
using UnityEngine;
using UnityEngine.UI;

namespace Tutorials
{
    public class TutorialView : MonoBehaviour, ITutorialView
    {
        [SerializeField] private Transform _parent;
        [SerializeField] private TutorialViewComponent _firstPrefab;
        [SerializeField] private TutorialViewComponent _secondPrefab;
        [SerializeField] private Button _closeButton;

        private readonly List<TutorialViewComponent> _activeComponents = new();
        private CanvasGroupFade _fade;
        private int _nextPrefabIndex;
        private bool _isClosing;

        public event Action Closed;

        private void Awake()
        {
            _fade = gameObject.TryGetComponent(out CanvasGroupFade fade)
                ? fade
                : gameObject.AddComponent<CanvasGroupFade>();

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Close);
            }

            _fade.HideImmediately();
        }

        public void Show(TutorialConfig.TutorialData tutorial)
        {
            if (tutorial == null)
            {
                return;
            }

            TutorialViewComponent prefab = GetNextPrefab();
            if (prefab == null)
            {
                Debug.LogWarning("TutorialView has no tutorial component prefabs assigned.", this);
                Closed?.Invoke();
                return;
            }

            Transform parent = _parent != null ? _parent : transform;
            TutorialViewComponent component = Instantiate(prefab, parent, false);
            _activeComponents.Add(component);
            component.Show(tutorial);

            if (_activeComponents.Count == 1)
            {
                _isClosing = false;
                _fade.Show();
            }
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(Close);
            }

            DestroyActiveComponents();
        }

        public void Close()
        {
            if (_isClosing || _activeComponents.Count == 0)
            {
                return;
            }

            _isClosing = true;
            _fade.Hide(() =>
            {
                DestroyActiveComponents();
                _isClosing = false;
                Closed?.Invoke();
            });
        }

        private TutorialViewComponent GetNextPrefab()
        {
            TutorialViewComponent preferred = _nextPrefabIndex == 0 ? _firstPrefab : _secondPrefab;
            TutorialViewComponent fallback = _nextPrefabIndex == 0 ? _secondPrefab : _firstPrefab;
            _nextPrefabIndex = (_nextPrefabIndex + 1) % 2;
            return preferred != null ? preferred : fallback;
        }

        private void DestroyActiveComponents()
        {
            for (int i = 0; i < _activeComponents.Count; i++)
            {
                TutorialViewComponent component = _activeComponents[i];
                if (component == null)
                {
                    continue;
                }

                component.gameObject.SetActive(false);
                Destroy(component.gameObject);
            }

            _activeComponents.Clear();
        }
    }
}
