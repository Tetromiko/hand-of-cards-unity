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
        [SerializeField] private TextMeshProUGUI centerIndexText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Blueprint Corner Ticks")]
        [SerializeField] private Image[] cornerTicks;

        [Header("State")]
        private CardData cardData;
        private RectTransform rectTransform;
        private CardHandController controller;
        private int currentIndex;
        private CardInteractionState visualState = CardInteractionState.Idle;

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
        public CardInteractionState VisualState => visualState;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        }

        public void SetupProceduralHierarchy()
        {
            // Background Image - Dark Slate #020617
            backgroundImage = GetComponent<Image>();
            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(0.02f, 0.04f, 0.09f, 0.96f);
            }

            // Outline / Border
            var outline = GetComponent<Outline>();
            if (outline == null) outline = gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.12f, 0.16f, 0.25f, 0.8f);
            outline.effectDistance = new Vector2(1f, -1f);

            // Center Index Text (for Layout Details mode)
            var indexObj = new GameObject("CenterIndexText", typeof(RectTransform), typeof(TextMeshProUGUI));
            indexObj.transform.SetParent(transform, false);
            var indexRt = indexObj.GetComponent<RectTransform>();
            indexRt.anchorMin = Vector2.zero;
            indexRt.anchorMax = Vector2.one;
            indexRt.sizeDelta = Vector2.zero;

            centerIndexText = indexObj.GetComponent<TextMeshProUGUI>();
            centerIndexText.fontSize = 28;
            centerIndexText.fontStyle = FontStyles.Bold;
            centerIndexText.alignment = TextAlignmentOptions.Center;
            centerIndexText.color = new Color(0.75f, 0.8f, 0.9f, 1f);

            // Top-left rank & suit
            var topTextObj = new GameObject("TopCornerText", typeof(RectTransform), typeof(TextMeshProUGUI));
            topTextObj.transform.SetParent(transform, false);
            var topRt = topTextObj.GetComponent<RectTransform>();
            topRt.anchorMin = new Vector2(0f, 1f);
            topRt.anchorMax = new Vector2(0f, 1f);
            topRt.pivot = new Vector2(0f, 1f);
            topRt.anchoredPosition = new Vector2(8f, -6f);
            topRt.sizeDelta = new Vector2(50f, 30f);

            rankTextTop = topTextObj.GetComponent<TextMeshProUGUI>();
            rankTextTop.fontSize = 16;
            rankTextTop.fontStyle = FontStyles.Bold;
            rankTextTop.alignment = TextAlignmentOptions.TopLeft;

            // Center large suit
            var centerObj = new GameObject("CenterSuitText", typeof(RectTransform), typeof(TextMeshProUGUI));
            centerObj.transform.SetParent(transform, false);
            var centerRt = centerObj.GetComponent<RectTransform>();
            centerRt.anchorMin = new Vector2(0.5f, 0.5f);
            centerRt.anchorMax = new Vector2(0.5f, 0.5f);
            centerRt.pivot = new Vector2(0.5f, 0.5f);
            centerRt.anchoredPosition = Vector2.zero;
            centerRt.sizeDelta = new Vector2(70f, 70f);

            centerSuitText = centerObj.GetComponent<TextMeshProUGUI>();
            centerSuitText.fontSize = 38;
            centerSuitText.alignment = TextAlignmentOptions.Center;

            // Bottom rotated rank
            var bottomObj = new GameObject("BottomCornerText", typeof(RectTransform), typeof(TextMeshProUGUI));
            bottomObj.transform.SetParent(transform, false);
            var bottomRt = bottomObj.GetComponent<RectTransform>();
            bottomRt.anchorMin = new Vector2(1f, 0f);
            bottomRt.anchorMax = new Vector2(1f, 0f);
            bottomRt.pivot = new Vector2(1f, 0f);
            bottomRt.anchoredPosition = new Vector2(-8f, 6f);
            bottomRt.sizeDelta = new Vector2(50f, 30f);
            bottomRt.localEulerAngles = new Vector3(0f, 0f, 180f);

            rankTextBottom = bottomObj.GetComponent<TextMeshProUGUI>();
            rankTextBottom.fontSize = 16;
            rankTextBottom.fontStyle = FontStyles.Bold;
            rankTextBottom.alignment = TextAlignmentOptions.TopLeft;

            // Create Corner Ticks
            CreateCornerTicks();
        }

        private void CreateCornerTicks()
        {
            cornerTicks = new Image[4];
            Vector2[] anchors = {
                new Vector2(0f, 1f), // Top-Left
                new Vector2(1f, 1f), // Top-Right
                new Vector2(0f, 0f), // Bottom-Left
                new Vector2(1f, 0f)  // Bottom-Right
            };
            Vector2[] pivots = {
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, 0f),
                new Vector2(1f, 0f)
            };

            for (int i = 0; i < 4; i++)
            {
                var tickObj = new GameObject($"Tick_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                tickObj.transform.SetParent(transform, false);
                var rt = tickObj.GetComponent<RectTransform>();
                rt.anchorMin = anchors[i];
                rt.anchorMax = anchors[i];
                rt.pivot = pivots[i];
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(12f, 12f);

                var img = tickObj.GetComponent<Image>();
                img.color = new Color(0.4f, 0.5f, 0.65f, 0.8f);
                img.raycastTarget = false;
                cornerTicks[i] = img;
            }
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
            UpdateVisuals();
        }

        public void SetVisualState(CardInteractionState state)
        {
            this.visualState = state;
            UpdateStateColors();
        }

        public void UpdateStateColors()
        {
            Color tickColor = new Color(0.4f, 0.5f, 0.65f, 0.7f); // Slate Idle
            Color textColor = new Color(0.75f, 0.8f, 0.9f, 1f);

            if (visualState == CardInteractionState.Dragged)
            {
                tickColor = new Color(0.2f, 0.9f, 0.5f, 1f); // Emerald
                textColor = new Color(0.2f, 0.9f, 0.5f, 1f);
            }
            else if (visualState == CardInteractionState.Hovered)
            {
                tickColor = new Color(0.95f, 0.35f, 0.65f, 1f); // Pink
                textColor = new Color(0.95f, 0.35f, 0.65f, 1f);
            }

            if (cornerTicks != null)
            {
                foreach (var tick in cornerTicks)
                {
                    if (tick != null) tick.color = tickColor;
                }
            }

            if (centerIndexText != null)
            {
                centerIndexText.color = textColor;
            }
        }

        public void UpdateVisuals()
        {
            if (cardData == null) return;

            bool showLayoutDetails = controller != null && controller.Settings.showLayoutDetails;

            Color suitColor = cardData.GetSuitColor();
            string suitSymbol = cardData.GetSuitSymbol();

            if (centerIndexText != null)
            {
                centerIndexText.text = $"#{currentIndex}";
                centerIndexText.gameObject.SetActive(showLayoutDetails);
            }

            if (cornerTicks != null)
            {
                foreach (var tick in cornerTicks)
                {
                    if (tick != null) tick.gameObject.SetActive(showLayoutDetails);
                }
            }

            if (rankTextTop != null)
            {
                rankTextTop.text = $"{cardData.rank} {suitSymbol}";
                rankTextTop.color = suitColor;
                rankTextTop.gameObject.SetActive(!showLayoutDetails);
            }
            if (rankTextBottom != null)
            {
                rankTextBottom.text = $"{cardData.rank} {suitSymbol}";
                rankTextBottom.color = suitColor;
                rankTextBottom.gameObject.SetActive(!showLayoutDetails);
            }
            if (centerSuitText != null)
            {
                centerSuitText.text = suitSymbol;
                centerSuitText.color = new Color(suitColor.r, suitColor.g, suitColor.b, 0.7f);
                centerSuitText.gameObject.SetActive(!showLayoutDetails);
            }

            UpdateStateColors();
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
