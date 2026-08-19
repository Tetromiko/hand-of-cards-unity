using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Tetromiko.CardsHandLayout.Samples
{
    public class CardHandDemoController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CardHandController handController;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Slider handWidthSlider;
        [SerializeField] private Slider cardDistanceSlider;
        [SerializeField] private Slider hoverLiftSlider;
        [SerializeField] private Button addCardButton;
        [SerializeField] private Button removeCardButton;
        [SerializeField] private Button resetButton;

        private void Awake()
        {
            if (handController == null)
            {
                handController = FindObjectOfType<CardHandController>();
            }

            if (handController == null && Application.isPlaying)
            {
                BuildCompleteDemoUI();
            }
        }

        private void Start()
        {
            EnsureEventSystem();

            if (handController == null)
            {
                handController = FindObjectOfType<CardHandController>();
            }

            if (handController != null)
            {
                // Wire Events
                handController.onCardClicked.AddListener(OnCardClicked);
                handController.onCardHovered.AddListener(OnCardHovered);
                handController.onCardUnhovered.AddListener(OnCardUnhovered);
                handController.onCardsReordered.AddListener(OnCardsReordered);
                handController.onHandUpdated.AddListener(UpdateUI);

                // Wire UI Sliders if assigned
                if (handWidthSlider != null)
                {
                    handWidthSlider.minValue = 300f;
                    handWidthSlider.maxValue = 1400f;
                    handWidthSlider.value = handController.Settings.handWidth;
                    handWidthSlider.onValueChanged.AddListener((v) =>
                    {
                        handController.Settings.handWidth = v;
                    });
                }

                if (cardDistanceSlider != null)
                {
                    cardDistanceSlider.minValue = handController.Settings.minCardDistance;
                    cardDistanceSlider.maxValue = 220f;
                    cardDistanceSlider.value = handController.Settings.cardDistance;
                    cardDistanceSlider.onValueChanged.AddListener((v) =>
                    {
                        handController.Settings.cardDistance = v;
                    });
                }

                if (hoverLiftSlider != null)
                {
                    hoverLiftSlider.minValue = 0f;
                    hoverLiftSlider.maxValue = 120f;
                    hoverLiftSlider.value = handController.Settings.hoverLift;
                    hoverLiftSlider.onValueChanged.AddListener((v) =>
                    {
                        handController.Settings.hoverLift = v;
                    });
                }

                if (addCardButton != null) addCardButton.onClick.AddListener(() => handController.AddCard());
                if (removeCardButton != null) removeCardButton.onClick.AddListener(() => handController.RemoveLastCard());
                if (resetButton != null) resetButton.onClick.AddListener(() => handController.CreateDefaultCards(5));
            }

            UpdateUI();
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
        }

        public void BuildCompleteDemoUI()
        {
            // Find or create Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasObj = new GameObject("DemoCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObj.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasObj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
            }

            // Create Hand Controller Container
            var handObj = new GameObject("CardHand", typeof(RectTransform), typeof(CardHandController));
            handObj.transform.SetParent(canvas.transform, false);

            var handRt = handObj.GetComponent<RectTransform>();
            handRt.anchorMin = new Vector2(0.5f, 0f);
            handRt.anchorMax = new Vector2(0.5f, 0f);
            handRt.pivot = new Vector2(0.5f, 0f);
            handRt.anchoredPosition = new Vector2(0f, 120f);
            handRt.sizeDelta = new Vector2(900f, 260f);

            handController = handObj.GetComponent<CardHandController>();

            // Top Header Panel
            var headerObj = new GameObject("HeaderPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            headerObj.transform.SetParent(canvas.transform, false);
            var headerRt = headerObj.GetComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0f, 1f);
            headerRt.anchorMax = new Vector2(1f, 1f);
            headerRt.pivot = new Vector2(0.5f, 1f);
            headerRt.anchoredPosition = new Vector2(0f, 0f);
            headerRt.sizeDelta = new Vector2(0f, 90f);
            headerObj.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.2f, 0.95f);

            // Title Text
            var titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(headerObj.transform, false);
            var titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 0.5f);
            titleRt.anchorMax = new Vector2(0.5f, 0.5f);
            titleRt.anchoredPosition = new Vector2(30f, 0f);
            titleRt.sizeDelta = new Vector2(500f, 50f);
            var titleTmp = titleObj.GetComponent<TextMeshProUGUI>();
            titleTmp.text = "🃏 Cards Hand Layout Engine Demo";
            titleTmp.fontSize = 24;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = Color.white;
            titleTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Status Text
            var statusObj = new GameObject("StatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
            statusObj.transform.SetParent(headerObj.transform, false);
            var statusRt = statusObj.GetComponent<RectTransform>();
            statusRt.anchorMin = new Vector2(0.5f, 0.5f);
            statusRt.anchorMax = new Vector2(1f, 0.5f);
            statusRt.anchoredPosition = new Vector2(-30f, 0f);
            statusRt.sizeDelta = new Vector2(500f, 50f);
            statusText = statusObj.GetComponent<TextMeshProUGUI>();
            statusText.text = "Hover a card or drag to reorder";
            statusText.fontSize = 18;
            statusText.color = new Color(0.8f, 0.85f, 1f, 1f);
            statusText.alignment = TextAlignmentOptions.MidlineRight;

            // Controls Bar at Bottom
            var barObj = new GameObject("ControlsBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            barObj.transform.SetParent(canvas.transform, false);
            var barRt = barObj.GetComponent<RectTransform>();
            barRt.anchorMin = new Vector2(0.5f, 0f);
            barRt.anchorMax = new Vector2(0.5f, 0f);
            barRt.pivot = new Vector2(0.5f, 0f);
            barRt.anchoredPosition = new Vector2(0f, 20f);
            barRt.sizeDelta = new Vector2(600f, 50f);

            var layout = barObj.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 15f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            addCardButton = CreateButton(barObj.transform, "+ Add Card", new Color(0.2f, 0.65f, 0.35f, 1f));
            removeCardButton = CreateButton(barObj.transform, "- Remove Card", new Color(0.8f, 0.3f, 0.3f, 1f));
            resetButton = CreateButton(barObj.transform, "Reset Hand (5)", new Color(0.3f, 0.45f, 0.8f, 1f));
        }

        private Button CreateButton(Transform parent, string label, Color bg)
        {
            var btnObj = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            var rt = btnObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160f, 44f);
            btnObj.GetComponent<Image>().color = bg;

            var txtObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(btnObj.transform, false);
            var txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            var tmp = txtObj.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 16;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return btnObj.GetComponent<Button>();
        }

        private void OnCardClicked(CardView card)
        {
            if (statusText != null)
            {
                statusText.text = $"Clicked: {card.CardData?.rank} of {card.CardData?.suit} (#{card.CurrentIndex})";
            }
        }

        private void OnCardHovered(CardView card)
        {
            if (statusText != null)
            {
                statusText.text = $"Hovered: {card.CardData?.rank} of {card.CardData?.suit} (Compensation active)";
            }
        }

        private void OnCardUnhovered(CardView card)
        {
            if (statusText != null)
            {
                statusText.text = "Hand Idle (Cards in hand: " + (handController != null ? handController.CardsCount : 0) + ")";
            }
        }

        private void OnCardsReordered(int fromIndex, int toIndex)
        {
            if (statusText != null)
            {
                statusText.text = $"Reordered card from slot {fromIndex} -> {toIndex}";
            }
        }

        private void UpdateUI()
        {
            if (handController != null && statusText != null && handController.CardViews.Count > 0)
            {
                statusText.text = $"Cards in hand: {handController.CardsCount}";
            }
        }
    }
}
