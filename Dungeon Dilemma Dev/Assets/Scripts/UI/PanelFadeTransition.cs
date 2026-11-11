using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PanelFadeTransition : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private Vector2 _offset = Vector2.zero;
    [SerializeField] private float _fadeDuration = 1.0f;
    [SerializeField] private bool _fadeOnEnable = true;

    private Vector3 _originalPos;
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTrans;

    private Tween _canvasTween;
    private Tween _rectTween;

    private void Awake()
    {
        _rectTrans = GetComponent<RectTransform>();
        if (_rectTrans == null)
        {
            Debug.LogWarning($"Fade transition missing rect transform on {name}");
        }
        _canvasGroup = GetComponent<CanvasGroup>();
        _originalPos = _rectTrans.anchoredPosition;
    }

    private void OnEnable()
    {
        if (_fadeOnEnable) FadeIn();
    }

    public void FadeIn()
    {
        if (_canvasGroup == null || _rectTrans == null) return;

        _rectTween?.Kill();
        _canvasTween?.Kill();

        _canvasGroup.alpha = 0;
        _rectTrans.anchoredPosition = new Vector3(_offset.x, _offset.y, 0) + _originalPos;
        _rectTween = _rectTrans.DOAnchorPos((Vector2)_originalPos, _fadeDuration).SetEase(Ease.OutCubic);
        _canvasTween = _canvasGroup.DOFade(1, _fadeDuration);
    }

    public void FadeOut()
    {
        if (_canvasGroup == null || _rectTrans == null)
        {
            gameObject.SetActive(false);
            return;
        }

        _rectTween?.Kill();
        _canvasTween?.Kill();

        _canvasGroup.alpha = 1;
        _rectTrans.anchoredPosition = _originalPos;
        _rectTween = _rectTrans.DOAnchorPos(_offset + (Vector2)_originalPos, _fadeDuration);
        _canvasTween = _canvasGroup.DOFade(0, _fadeDuration).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
