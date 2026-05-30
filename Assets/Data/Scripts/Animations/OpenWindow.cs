using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class OpenWindow : MonoBehaviour
{
    private static readonly List<OpenWindow> Windows = new();
    private static readonly List<OpenWindow> HiddenWindows = new();
    private static OpenWindow _currentWindow;

    [SerializeField] private string _windowId;
    [SerializeField] private RectTransform _window;
    [SerializeField] private RectTransform _showPosition;
    [SerializeField] private RectTransform _hidePosition;
    [SerializeField] private float _duration = .25f;
    [SerializeField] private UnityEvent _onShown;
    [SerializeField] private UnityEvent _onHidden;

    private Tween _tween;
    private bool _isShow = false;

    private void OnEnable()
    {
        if (!Windows.Contains(this))
            Windows.Add(this);
    }

    private void OnDisable()
    {
        Windows.Remove(this);
        HiddenWindows.Remove(this);

        if (_currentWindow == this)
            _currentWindow = null;
    }

    private void Start()
    {
        if (_window != null && _hidePosition != null)
            _window.position = _hidePosition.position;
    }

    public void Show()
    {
        gameObject.SetActive(true);

        if (_currentWindow == this)
            return;

        if (_currentWindow != null)
        {
            HiddenWindows.Remove(_currentWindow);
            HiddenWindows.Add(_currentWindow);
            _currentWindow.HideInternal();
        }

        HiddenWindows.Remove(this);
        _currentWindow = this;
        ShowInternal();
    }

    public void Hide()
    {
        HiddenWindows.Remove(this);
        HideInternal();

        if (_currentWindow != this)
            return;

        _currentWindow = null;
        ShowPreviousHiddenWindow();
    }

    public static void HideAll()
    {
        HiddenWindows.Clear();

        foreach (var window in Windows)
            window.HideInternal();

        _currentWindow = null;
    }

    private void ShowInternal()
    {
        if (_isShow)
            return;
        if (_window == null || _showPosition == null)
            return;

        _window.gameObject.SetActive(true);

        if (_tween != null)
        {
            _tween.Kill();
        }
        _isShow = true;
        _tween = _window.DOMove(_showPosition.position, _duration);
        _onShown?.Invoke();
    }

    private void HideInternal()
    {
        if (!_isShow)
            return;
        if (_window == null || _hidePosition == null)
            return;

        if (_tween != null)
        {
            _tween.Kill();
        }
        _isShow = false;
        _tween = _window
            .DOMove(_hidePosition.position, _duration)
            .OnComplete(() =>
            {
                if (!_isShow && _window != null)
                    _window.gameObject.SetActive(false);

                _onHidden?.Invoke();
            });
    }

    private static void ShowPreviousHiddenWindow()
    {
        while (HiddenWindows.Count > 0)
        {
            int lastIndex = HiddenWindows.Count - 1;
            var window = HiddenWindows[lastIndex];
            HiddenWindows.RemoveAt(lastIndex);

            if (window == null || !window.isActiveAndEnabled)
                continue;

            _currentWindow = window;
            window.ShowInternal();
            return;
        }
    }

}
