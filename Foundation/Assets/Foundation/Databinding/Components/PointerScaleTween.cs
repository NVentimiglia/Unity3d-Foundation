// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Foundation.Databinding
{
    /// <summary>
    ///     Scale an element via IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, or IPointerUpHandler
    /// </summary>
    [AddComponentMenu("Foundation/Databinding/PointerScaleTween")]
    public class PointerScaleTween : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
        IPointerUpHandler
    {
        private Button Button;
        public float DownScale = 1.05f;
        public float HoverScale = 1.1f;
        protected bool IsDown;
        protected bool IsOver;
        public float NormalScale = 1f;
        protected RectTransform Transform;
        public float TweenTime = .1f;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Button && !Button.IsInteractable())
                return;

            IsDown = true;

            if (!gameObject.activeInHierarchy)
                return;

            StopAllCoroutines();
            StartCoroutine(TweenTo(HoverScale, DownScale));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Button && !Button.IsInteractable())
                return;

            IsOver = true;

            if (!gameObject.activeInHierarchy)
                return;

            StopAllCoroutines();
            StartCoroutine(TweenTo(NormalScale, HoverScale));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Button && !Button.IsInteractable())
                return;

            IsOver = false;

            if (!gameObject.activeInHierarchy)
                return;

            StopAllCoroutines();
            StartCoroutine(TweenTo(HoverScale, NormalScale));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Button && !Button.IsInteractable())
                return;

            IsDown = false;

            if (!gameObject.activeInHierarchy)
                return;

            StopAllCoroutines();
            if (IsOver)
                StartCoroutine(TweenTo(DownScale, HoverScale));
        }

        private void Awake()
        {
            Button = GetComponent<Button>();
            Transform = GetComponent<RectTransform>();
        }

        private void OnDisable()
        {
            Transform.localScale = Vector3.one*NormalScale;
            IsOver = false;
            IsDown = false;
            StopAllCoroutines();
        }

        private IEnumerator TweenTo(float from, float to)
        {
            float delta = 0;
            do
            {
                yield return 1;
                delta += Time.deltaTime;

                Transform.localScale = Vector3.one*Mathf.Lerp(from, to, delta/TweenTime);
            } while (delta < TweenTime);

            yield return 1;

            Transform.localScale = Vector3.one*to;
        }
    }
}