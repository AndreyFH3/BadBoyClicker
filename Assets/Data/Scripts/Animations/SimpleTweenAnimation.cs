using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SimpleTweenAnimation : MonoBehaviour
{
    public enum PlayMode
    {
        OnStart,
        OnEnable,
        Manual
    }

    public enum StepInsertMode
    {
        Append,
        Join
    }

    public enum TweenTargetType
    {
        Auto,
        Transform,
        RectTransform,
        CanvasGroup,
        Graphic,
        Image,
        SpriteRenderer,
        TMP_Text
    }

    public enum TweenAnimationType
    {
        Move,
        LocalMove,
        AnchoredPosition,
        Scale,
        Rotate,
        LocalRotate,
        Fade,
        Color,
        FillAmount
    }

    [Serializable]
    public class TweenStep
    {
        [Header("General")]
        public string name;
        public bool enabled = true;
        public StepInsertMode insertMode = StepInsertMode.Append;

        [Header("Tween")]
        public TweenAnimationType animationType = TweenAnimationType.Scale;
        public TweenTargetType targetType = TweenTargetType.Auto;
        public Component targetOverride;

        [Min(0f)] public float duration = 0.3f;
        [Min(0f)] public float delay = 0f;
        public Ease ease = Ease.OutQuad;

        [Header("Loop")]
        public int loops = 0; // 0 = play once
        public LoopType loopType = LoopType.Restart;

        [Header("Options")]
        public bool isRelative = false;
        public bool setFrom = false;
        public bool useCustomFromValue = false;
        public bool ignoreTimeScale = false;

        [Header("Vector Values")]
        public Vector3 toVector3;
        public Vector3 fromVector3;

        [Header("Float Values")]
        public float toFloat = 1f;
        public float fromFloat = 0f;

        [Header("Color Values")]
        public Color toColor = Color.white;
        public Color fromColor = Color.white;
    }

    [Header("Playback")]
    [SerializeField] private PlayMode _playMode = PlayMode.OnStart;
    [SerializeField] private bool _autoKill = false;
    [SerializeField] private bool _rebuildOnPlay = true;
    [SerializeField] private bool _playBackwards = false;

    [Header("Steps")]
    [SerializeField] private List<TweenStep> _steps = new();

    private Sequence _sequence;

    public bool IsPlaying => _sequence != null && _sequence.IsActive() && _sequence.IsPlaying();
    public bool IsInitialized => _sequence != null && _sequence.IsActive();

    private void Start()
    {
        if (_playMode == PlayMode.OnStart)
            Play();
    }

    private void OnEnable()
    {
        if (_playMode == PlayMode.OnEnable)
            Play();
    }

    private void OnDisable()
    {
        if (_sequence != null && _sequence.IsActive())
            _sequence.Pause();
    }

    private void OnDestroy()
    {
        Kill();
    }

    [ContextMenu("Play")]
    public void Play()
    {
        if (_rebuildOnPlay || !IsInitialized)
            BuildSequence();

        if (_sequence == null)
            return;

        if (_playBackwards)
            _sequence.PlayBackwards();
        else
            _sequence.PlayForward();
    }

    [ContextMenu("Restart")]
    public void Restart()
    {
        BuildSequence();
        if (_sequence == null)
            return;

        if (_playBackwards)
            _sequence.PlayBackwards();
        else
            _sequence.Restart();
    }

    [ContextMenu("Rewind")]
    public void Rewind()
    {
        if (!IsInitialized)
            return;

        _sequence.Rewind();
        _sequence.Pause();
    }

    [ContextMenu("Complete")]
    public void Complete()
    {
        if (!IsInitialized)
            return;

        _sequence.Complete();
    }

    [ContextMenu("Kill")]
    public void Kill()
    {
        if (_sequence != null)
        {
            _sequence.Kill();
            _sequence = null;
        }
    }

    public void BuildSequence()
    {
        Kill();

        _sequence = DOTween.Sequence();
        _sequence.SetAutoKill(_autoKill);
        _sequence.Pause();

        foreach (TweenStep step in _steps)
        {
            if (!step.enabled)
                continue;

            Tween tween = CreateTween(step);
            if (tween == null)
                continue;

            if (step.insertMode == StepInsertMode.Append)
                _sequence.Append(tween);
            else
                _sequence.Join(tween);
        }
    }

    private Tween CreateTween(TweenStep step)
    {
        Component target = ResolveTarget(step);

        if (target == null)
        {
            Debug.LogWarning($"[{nameof(SimpleTweenAnimation)}] Target not found for step '{step.name}' on {name}", this);
            return null;
        }

        Tween tween = step.animationType switch
        {
            TweenAnimationType.Move => CreateMoveTween(target, step),
            TweenAnimationType.LocalMove => CreateLocalMoveTween(target, step),
            TweenAnimationType.AnchoredPosition => CreateAnchoredPositionTween(target, step),
            TweenAnimationType.Scale => CreateScaleTween(target, step),
            TweenAnimationType.Rotate => CreateRotateTween(target, step),
            TweenAnimationType.LocalRotate => CreateLocalRotateTween(target, step),
            TweenAnimationType.Fade => CreateFadeTween(target, step),
            TweenAnimationType.Color => CreateColorTween(target, step),
            TweenAnimationType.FillAmount => CreateFillAmountTween(target, step),
            _ => null
        };

        if (tween == null)
        {
            Debug.LogWarning(
                $"[{nameof(SimpleTweenAnimation)}] Could not create tween '{step.animationType}' for target '{target.GetType().Name}' on {name}",
                this);
            return null;
        }

        ApplyCommonSettings(tween, step, target);
        return tween;
    }

    private void ApplyCommonSettings(Tween tween, TweenStep step, Component target)
    {
        tween.SetEase(step.ease)
             .SetDelay(step.delay)
             .SetLoops(step.loops, step.loopType)
             .SetUpdate(step.ignoreTimeScale)
             .SetTarget(target);

        if (step.isRelative)
            tween.SetRelative();
    }

    private Component ResolveTarget(TweenStep step)
    {
        if (step.targetOverride != null)
            return step.targetOverride;

        return step.targetType switch
        {
            TweenTargetType.Transform => transform,
            TweenTargetType.RectTransform => GetComponent<RectTransform>(),
            TweenTargetType.CanvasGroup => GetComponent<CanvasGroup>(),
            TweenTargetType.Graphic => GetComponent<Graphic>(),
            TweenTargetType.Image => GetComponent<Image>(),
            TweenTargetType.SpriteRenderer => GetComponent<SpriteRenderer>(),
            TweenTargetType.TMP_Text => GetComponent<TMP_Text>(),
            TweenTargetType.Auto => ResolveAutoTarget(step.animationType),
            _ => null
        };
    }

    private Component ResolveAutoTarget(TweenAnimationType animationType)
    {
        switch (animationType)
        {
            case TweenAnimationType.AnchoredPosition:
                return GetComponent<RectTransform>();

            case TweenAnimationType.Fade:
                if (TryGetComponent<CanvasGroup>(out var canvasGroup))
                    return canvasGroup;
                if (TryGetComponent<TMP_Text>(out var tmpText))
                    return tmpText;
                if (TryGetComponent<Graphic>(out var graphic))
                    return graphic;
                if (TryGetComponent<SpriteRenderer>(out var spriteRenderer))
                    return spriteRenderer;
                break;

            case TweenAnimationType.Color:
                if (TryGetComponent<TMP_Text>(out var tmpColorText))
                    return tmpColorText;
                if (TryGetComponent<Graphic>(out var graphicColor))
                    return graphicColor;
                if (TryGetComponent<SpriteRenderer>(out var spriteColor))
                    return spriteColor;
                break;

            case TweenAnimationType.FillAmount:
                return GetComponent<Image>();

            default:
                return transform;
        }

        return null;
    }

    private Tween CreateMoveTween(Component target, TweenStep step)
    {
        if (target is not Transform tr)
            return null;

        if (step.setFrom && step.useCustomFromValue)
            tr.position = step.fromVector3;

        var tweener = tr.DOMove(step.toVector3, step.duration);

        if (step.setFrom && !step.useCustomFromValue)
            tweener.From();

        return tweener;
    }

    private Tween CreateLocalMoveTween(Component target, TweenStep step)
    {
        if (target is not Transform tr)
            return null;

        if (step.setFrom && step.useCustomFromValue)
            tr.localPosition = step.fromVector3;

        var tweener = tr.DOLocalMove(step.toVector3, step.duration);

        if (step.setFrom && !step.useCustomFromValue)
            tweener.From();

        return tweener;
    }

    private Tween CreateAnchoredPositionTween(Component target, TweenStep step)
    {
        if (target is not RectTransform rectTransform)
            return null;

        Vector2 to = new Vector2(step.toVector3.x, step.toVector3.y);
        Vector2 from = new Vector2(step.fromVector3.x, step.fromVector3.y);

        if (step.setFrom && step.useCustomFromValue)
            rectTransform.anchoredPosition = from;

        var tweener = rectTransform.DOAnchorPos(to, step.duration);

        if (step.setFrom && !step.useCustomFromValue)
            tweener.From();

        return tweener;
    }

    private Tween CreateScaleTween(Component target, TweenStep step)
    {
        if (target is not Transform tr)
            return null;

        if (step.setFrom && step.useCustomFromValue)
            tr.localScale = step.fromVector3;

        var tweener = tr.DOScale(step.toVector3, step.duration);

        if (step.setFrom && !step.useCustomFromValue)
            tweener.From();

        return tweener;
    }

    private Tween CreateRotateTween(Component target, TweenStep step)
    {
        if (target is not Transform tr)
            return null;

        if (step.setFrom && step.useCustomFromValue)
            tr.eulerAngles = step.fromVector3;

        var tweener = tr.DORotate(step.toVector3, step.duration);

        if (step.setFrom && !step.useCustomFromValue)
            tweener.From();

        return tweener;
    }

    private Tween CreateLocalRotateTween(Component target, TweenStep step)
    {
        if (target is not Transform tr)
            return null;

        if (step.setFrom && step.useCustomFromValue)
            tr.localEulerAngles = step.fromVector3;

        var tweener = tr.DOLocalRotate(step.toVector3, step.duration);

        if (step.setFrom && !step.useCustomFromValue)
            tweener.From();

        return tweener;
    }

    private Tween CreateFadeTween(Component target, TweenStep step)
    {
        switch (target)
        {
            case CanvasGroup cg:
                {
                    if (step.setFrom && step.useCustomFromValue)
                        cg.alpha = step.fromFloat;

                    var tweener = cg.DOFade(step.toFloat, step.duration);

                    if (step.setFrom && !step.useCustomFromValue)
                        tweener.From();

                    return tweener;
                }

            case TMP_Text tmp:
                {
                    if (step.setFrom && step.useCustomFromValue)
                    {
                        Color color = tmp.color;
                        color.a = step.fromFloat;
                        tmp.color = color;
                    }

                    var tweener = tmp.DOFade(step.toFloat, step.duration);

                    if (step.setFrom && !step.useCustomFromValue)
                        tweener.From();

                    return tweener;
                }

            case Graphic g:
                {
                    if (step.setFrom && step.useCustomFromValue)
                    {
                        Color color = g.color;
                        color.a = step.fromFloat;
                        g.color = color;
                    }

                    var tweener = g.DOFade(step.toFloat, step.duration);

                    if (step.setFrom && !step.useCustomFromValue)
                        tweener.From();

                    return tweener;
                }

            case SpriteRenderer sr:
                {
                    if (step.setFrom && step.useCustomFromValue)
                    {
                        Color color = sr.color;
                        color.a = step.fromFloat;
                        sr.color = color;
                    }

                    var tweener = sr.DOFade(step.toFloat, step.duration);

                    if (step.setFrom && !step.useCustomFromValue)
                        tweener.From();

                    return tweener;
                }

            default:
                return null;
        }
    }

    private Tween CreateColorTween(Component target, TweenStep step)
    {
        switch (target)
        {
            case TMP_Text tmp:
                {
                    if (step.setFrom && step.useCustomFromValue)
                        tmp.color = step.fromColor;

                    var tweener = tmp.DOColor(step.toColor, step.duration);

                    if (step.setFrom && !step.useCustomFromValue)
                        tweener.From();

                    return tweener;
                }

            case Graphic g:
                {
                    if (step.setFrom && step.useCustomFromValue)
                        g.color = step.fromColor;

                    var tweener = g.DOColor(step.toColor, step.duration);

                    if (step.setFrom && !step.useCustomFromValue)
                        tweener.From();

                    return tweener;
                }

            case SpriteRenderer sr:
                {
                    if (step.setFrom && step.useCustomFromValue)
                        sr.color = step.fromColor;

                    var tweener = sr.DOColor(step.toColor, step.duration);

                    if (step.setFrom && !step.useCustomFromValue)
                        tweener.From();

                    return tweener;
                }

            default:
                return null;
        }
    }

    private Tween CreateFillAmountTween(Component target, TweenStep step)
    {
        if (target is not Image image)
            return null;

        if (step.setFrom && step.useCustomFromValue)
            image.fillAmount = step.fromFloat;

        var tweener = image.DOFillAmount(step.toFloat, step.duration);

        if (step.setFrom && !step.useCustomFromValue)
            tweener.From();

        return tweener;
    }
}