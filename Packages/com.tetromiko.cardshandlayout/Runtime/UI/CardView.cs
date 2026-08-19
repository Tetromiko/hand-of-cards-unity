using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Tetromiko.CardsHandLayout
{
    [RequireComponent(typeof(RectTransform))]
    public class CardView : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private Image cardArtImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI rankTextTop;
        [SerializeField] private TextMeshProUGUI rankTextBottom;
        [SerializeField] private TextMeshProUGUI suitTextTop;
        [SerializeField] private TextMeshProUGUI suitTextBottom;
        [SerializeField] private TextMeshProUGUI centerSuitText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("State")]
        private CardData cardData;
        private RectTransform rectTransform;
        private CardHandController controller;
        private int currentIndex;

        // Motion smoothing
        private Vector2 targetPosition;
        private Vector2 currentVelocity;
        private float targetScale = 1f;
        private float currentScaleVelocity;
        private bool isBeingDragged;

        public RectTransform RectTransform => rectTransform != null ? rectTransform : (rectTransform = GetComponent<RectTransform>());
        public CardData CardData => cardData;
        public int CurrentIndex => currentIndex;
        public bool IsBeingDragged => isBeingDragged;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Initialize(CardData data, CardHandController handController, int index)
        {
            this.cardData = data;
            this.controller = handController;
            this.currentIndex = index;
            UpdateVisuals();
        }

        public void SetIndex(int index)
        {
            this.currentIndex = index;
        }

        public void UpdateVisuals()
        {
            if (cardData == null) return;

            Color suitColor = cardData.GetSuitColor();
            string suitSymbol = cardData.GetSuitSymbol();

            if (cardArtImage != null)
            {
                if (cardData.cardSprite != null)
                {
                    cardArtImage.sprite = cardData.cardSprite;
                    cardArtImage.gameObject.SetActive(true);
                }
                else
                {
                    cardArtImage.gameObject.SetActive(false);
                }
            }

            if (rankTextTop != null)
            {
                rankTextTop.text = cardData.rank;
                rankTextTop.color = suitColor;
            }
            if (rankTextBottom != null)
            {
                rankTextBottom.text = cardData.rank;
                rankTextBottom.color = suitColor;
            }
            if (suitTextTop != null)
            {
                suitTextTop.text = suitSymbol;
                suitTextTop.color = suitColor;
            }
            if (suitTextBottom != null)
            {
                suitTextBottom.text = suitSymbol;
                suitTextBottom.color = suitColor;
            }
            if (centerSuitText != null)
            {
                centerSuitText.text = suitSymbol;
                centerSuitText.color = new Color(suitColor.r, suitColor.g, suitColor.b, 0.4f);
            }
            if (titleText != null)
            {
                titleText.text = cardData.title;
            }
        }

        public void SetTargetTransform(Vector2 localPos, float scale, float smoothTime, float maxSpeed)
        {
            targetPosition = localPos;
            targetScale = scale;

            if (isBeingDragged)
            {
                RectTransform.anchoredPosition = targetPosition;
                RectTransform.localScale = Vector3.one * targetScale;
            }
            else
            {
                // Smooth spring-like motion
                RectTransform.anchoredPosition = Vector2.SmoothDamp(
                    RectTransform.anchoredPosition,
                    targetPosition,
                    ref currentVelocity,
                    smoothTime,
                    maxSpeed,
                    Time.unscaledDeltaTime
                );

                float newScale = Mathf.SmoothDamp(
                    RectTransform.localScale.x,
                    targetScale,
                    ref currentScaleVelocity,
                    smoothTime,
                    maxSpeed,
                    Time.unscaledDeltaTime
                );
                RectTransform.localScale = Vector3.one * newScale;
            }
        }

        public void SnapToTarget()
        {
            RectTransform.anchoredPosition = targetPosition;
            RectTransform.localScale = Vector3.one * targetScale;
            currentVelocity = Vector2.zero;
            currentScaleVelocity = 0f;
        }

        // Pointer & Drag Handlers
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (controller != null)
            {
                controller.OnCardPointerEnter(this);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (controller != null)
            {
                controller.OnCardPointerExit(this);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isBeingDragged && controller != null)
            {
                controller.OnCardClicked(this);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isBeingDragged = true;
            if (controller != null)
            {
                controller.OnCardBeginDrag(this, eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (controller != null)
            {
                controller.OnCardDrag(this, eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isBeingDragged = false;
            if (controller != null)
            {
                controller.OnCardEndDrag(this, eventData);
            }
        }
    }
}
