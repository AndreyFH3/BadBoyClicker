using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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
        FillAmount,
        PunchScale,
        PunchPosition,
        PunchRotation,
        ShakePosition,
        ShakeScale,
        ShakeRotation
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
        public int loops = 0; // 0 or 1 = play once
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

        [Header("Punch / Shake")]
        public int vibrato = 10;
        public float elasticity = 1f;
        public float randomness = 90f;
        public bool snapping = false;

        [Header("Events")]
        public UnityEvent onStepComplete;

        public bool IsPunchOrShake =>
            animationType == TweenAnimationType.PunchScale ||
            animationType == TweenAnimationType.PunchPosition ||
            animationType == TweenAnimationType.PunchRotation ||
            animationType == TweenAnimationType.ShakePosition ||
            animationType == TweenAnimationType.ShakeScale ||
            animationType == TweenAnimationType.ShakeRotation;
    }

    [Header("Playback")]
    [SerializeField] private PlayMode _playMode = PlayMode.OnStart;
    [SerializeField] private bool _autoKill = false;
    [SerializeField] private bool _rebuildOnPlay = true;
    [SerializeField] private bool _playBackwards = false;

    [Tooltip("Restore original values captured before the sequence started when it is rewound.")]
    [SerializeField] private bool _restoreOnRewind = true;
    [Tooltip("Restore original values when the object is disabled while playing.")]
    [SerializeField] private bool _restoreOnDisable = false;

    [Header("Events")]
    [SerializeField] private UnityEvent _onPlay;
    [SerializeField] private UnityEvent _onStepComplete;
    [SerializeField] private UnityEvent _onComplete;

    [Header("Steps")]
    [SerializeField] private List<TweenStep> _steps = new();

    private Sequence _sequence;
    private Action _runtimeOnComplete;

    // Snapshot of original values captured before the sequence modifies targets.
    private readonly List<Action> _restoreActions = new();
    private readonly HashSet<Component> _snapshotTargets = new();

    public bool IsPlaying => _sequence != null && _sequence.IsActive() && _sequence.IsPlaying();
    public bool IsInitialized => _sequence != null && _sequence.IsActive();
    public Sequence Sequence => _sequence;

    public UnityEvent OnPlayEvent => _onPlay;
    public UnityEvent OnStepCompleteEvent => _onStepComplete;
    public UnityEvent OnCompleteEvent => _onComplete;

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

        if (_restoreOnDisable)
            RestoreOriginals();
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

    /// <summary>
    /// Plays the sequence and invokes <paramref name="onComplete"/> when it finishes.
    /// Returns the underlying Sequence for further chaining if needed.
    /// </summary>
    public Sequence Play(Action onComplete)
    {
        _runtimeOnComplete = onComplete;
        Play();
        return _sequence;
    }

    [ContextMenu("Restart")]
    public void Restart()
    {
        BuildSequence();
        if (_sequence == null)
            return;

        if (_playBackwards)
        {
            _sequence.Complete();
            _sequence.PlayBackwards();
        }
        else
        {
            _sequence.Restart();
        }
    }

    [ContextMenu("Rewind")]
    public void Rewind()
    {
        if (!IsInitialized)
            return;

        _sequence.Rewind();
        _sequence.Pause();

        if (_restoreOnRewind)
            RestoreOriginals();
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

        _restoreActions.Clear();
        _snapshotTargets.Clear();

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

        _sequence.OnStart(HandlePlay);
        _sequence.OnStepComplete(HandleStepComplete);
        _sequence.OnComplete(HandleComplete);
    }

    private void HandlePlay() => _onPlay?.Invoke();

    private void HandleStepComplete() => _onStepComplete?.Invoke();

    private void HandleComplete()
    {
        _onComplete?.Invoke();

        Action cb = _runtimeOnComplete;
        _runtimeOnComplete = null;
        cb?.Invoke();
    }

    private void RestoreOriginals()
    {
        for (int i = _restoreActions.Count - 1; i >= 0; i--)
            _restoreActions[i]?.Invoke();
    }

    private Tween CreateTween(TweenStep step)
    {
        Component target = ResolveTarget(step);

        if (target == null)
        {
            Debug.LogWarning($"[{nameof(SimpleTweenAnimation)}] Target not found for step '{step.name}' on {name}", this);
            return null;
        }

        CaptureOriginal(target);

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
            TweenAnimationType.PunchScale => CreatePunchScaleTween(target, step),
            TweenAnimationType.PunchPosition => CreatePunchPositionTween(target, step),
            TweenAnimationType.PunchRotation => CreatePunchRotationTween(target, step),
            TweenAnimationType.ShakePosition => CreateShakePositionTween(target, step),
            TweenAnimationType.ShakeScale => CreateShakeScaleTween(target, step),
            TweenAnimationType.ShakeRotation => CreateShakeRotationTween(target, step),
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

        if (step.isRelative && !step.IsPunchOrShake)
            tween.SetRelative();

        if (step.onStepComplete != null)
            tween.OnComplete(() => step.onStepComplete.Invoke());
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

    private void CaptureOriginal(Component target)
    {
        if (target == null || !_snapshotTargets.Add(target))
            return;

        switch (target)
        {
            case RectTransform rt:
            {
                Vector2 anchored = rt.anchoredPosition;
                Vector3 localPos = rt.localPosition;
                Vector3 pos = rt.position;
                Vector3 localScale = rt.localScale;
                Quaternion localRot = rt.localRotation;
                Quaternion rot = rt.rotation;
                _restoreActions.Add(() =>
                {
                    if (rt == null) return;
                    rt.anchoredPosition = anchored;
                    rt.localPosition = localPos;
                    rt.position = pos;
                    rt.localScale = localScale;
                    rt.localRotation = localRot;
                    rt.rotation = rot;
                });
                break;
            }

            case Transform tr:
            {
                Vector3 localPos = tr.localPosition;
                Vector3 pos = tr.position;
                Vector3 localScale = tr.localScale;
                Quaternion localRot = tr.localRotation;
                Quaternion rot = tr.rotation;
                _restoreActions.Add(() =>
                {
                    if (tr == null) return;
                    tr.localPosition = localPos;
                    tr.position = pos;
                    tr.localScale = localScale;
                    tr.localRotation = localRot;
                    tr.rotation = rot;
                });
                break;
            }

            case CanvasGroup cg:
            {
                float alpha = cg.alpha;
                _restoreActions.Add(() => { if (cg != null) cg.alpha = alpha; });
                break;
            }

            case Image img:
            {
                Color color = img.color;
                float fill = img.fillAmount;
                _restoreActions.Add(() =>
                {
                    if (img == null) return;
                    img.color = color;
                    img.fillAmount = fill;
                });
                break;
            }

            case TMP_Text tmp:
            {
                Color color = tmp.color;
                _restoreActions.Add(() => { if (tmp != null) tmp.color = color; });
                break;
            }

            case Graphic g:
            {
                Color color = g.color;
                _restoreActions.Add(() => { if (g != null) g.color = color; });
                break;
            }

            case SpriteRenderer sr:
            {
                Color color = sr.color;
                _restoreActions.Add(() => { if (sr != null) sr.color = color; });
                break;
            }
        }
    }

    private Tween CreateMoveTween(Component target, TweenStep step)
    {
        if (target is not Transform tr)
            return null;

        if (step.setFrom && step.useCustomFromValue)
            tr.position = step.fromVector3;

        var tweener = tr.DOMove(step.toVector3, step.duration).SetOptions(step.snapping);

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

        var tweener = tr.DOLocalMove(step.toVector3, step.duration).SetOptions(step.snapping);

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

        var tweener = rectTransform.DOAnchorPos(to, step.duration).SetOptions(step.snapping);

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

    private Tween CreatePunchScaleTween(Component target, TweenStep step)
    {
        if (target is not Transform tr)
            return null;

        return tr.DOPunchScale(step.toVector3, step.duration, step.vibrato, step.elasticity);
    }

    private Tween CreatePunchPositionTween(Component target, TweenStep step)
    {
        if (target is not Transform tr)
            return null;

        return tr.DOPunchPosition(step.toVector3, step.duration, step.vibrato, step.elasticity, step.snapping);
    }

    private Tween CreatePunchRotationTween(Component target, TweenStep step)
    {
        if (target is not Transform tr)
            return null;

        return tr.DOPunchRotation(step.toVector3, step.duration, step.vibrato, step.elasticity);
    }

    private Tween CreateShakePositionTween(Component target, TweenStep step)
    {
        if (target is not Transform tr)
            return null;

        return tr.DOShakePosition(step.duration, step.toVector3, step.vibrato, step.randomness, step.snapping);
    }

    private Tween CreateShakeScaleTween(Component target, TweenStep step)
    {
        if (target is not Transform tr)
            return null;

        return tr.DOShakeScale(step.duration, step.toVector3, step.vibrato, step.randomness);
    }

    private Tween CreateShakeRotationTween(Component target, TweenStep step)
    {
        if (target is not Transform tr)
            return null;

        return tr.DOShakeRotation(step.duration, step.toVector3, step.vibrato, step.randomness);
    }
}
