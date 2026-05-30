using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core
{    
    public class ClickableObject : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Transform _catIcon;
        private Tween tween;
        public System.Action OnClick;

        public void OnPointerClick(PointerEventData pointerEventData)
        {
            OnClick?.Invoke();
            if (tween != null)
            {
                tween.Complete();
                tween.Kill();
            }
            tween = _catIcon.DOPunchScale(Vector3.one * .1f, 0.1f);
            Debug.Log("Click!");
        }       
    }
}