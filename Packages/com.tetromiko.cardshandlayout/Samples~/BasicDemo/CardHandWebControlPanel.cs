using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Tetromiko.CardsHandLayout.Samples
{
    public class CardHandWebControlPanel : MonoBehaviour
    {
        [SerializeField] private CardHandController handController;

        private RectTransform panelRt;
        private bool isMinimized = false;
        private GameObject contentRoot;
        private GameObject minimizedRoot;

        // UI elements
        private TextMeshProUGUI headerCountText;
        private TextMeshProUGUI miniCountText;
        private TextMeshProUGUI countText;
        private Button minButton;
        private Button addCardBtn;
        private Button removeCardBtn;
        private Button resetHandBtn;
        private Button miniAddCardBtn;
        private Button miniRemoveCardBtn;
        private Button miniResetHandBtn;

        // Slider & Input pairs
        private Slider cardWidthSlider;
        private TMP_InputField cardWidthInput;
        private Slider handWidthSlider;
        private TMP_InputField handWidthInput;

        private Slider cardDistSlider;
        private TMP_InputField cardDistInput;
        private Slider minCardDistSlider;
        private TMP_InputField minCardDistInput;

        private Slider hoverDistSlider;
        private TMP_InputField hoverDistInput;
        private Slider hoverLiftSlider;
        private TMP_InputField hoverLiftInput;

        // Layout Details toggle button
        private Button layoutDetailsBtn;
        private TextMeshProUGUI layoutDetailsBtnText;

        // Telemetry
        private TextMeshProUGUI telemetryStateText;
        private TextMeshProUGUI telemetryStepText;
        private TextMeshProUGUI telemetryHStepText;

        private bool isUpdatingUI = false;

        public void Initialize(CardHandController controller)
        {
            this.handController = controller;
            BuildWebStylePanel();
            WireEvents();
            RefreshAllValues();
        }

        private void Awake()
        {
            if (handController == null) handController = FindObjectOfType<CardHandController>();
        }

        private void Start()
        {
            if (contentRoot == null)
            {
                BuildWebStylePanel();
                WireEvents();
                RefreshAllValues();
            }
        }

        private void Update()
        {
            if (handController == null) return;
            UpdateTelemetry();
        }

        public void BuildWebStylePanel()
        {
            panelRt = GetComponent<RectTransform>();
            if (panelRt == null) panelRt = gameObject.AddComponent<RectTransform>();

            panelRt.anchorMin = new Vector2(0f, 1f);
            panelRt.anchorMax = new Vector2(0f, 1f);
            panelRt.pivot = new Vector2(0f, 1f);
            panelRt.anchoredPosition = new Vector2(16f, -16f);
            panelRt.sizeDelta = new Vector2(330f, 540f);

            var bgImg = gameObject.GetComponent<Image>();
            if (bgImg == null) bgImg = gameObject.AddComponent<Image>();
            bgImg.color = new Color(0.015f, 0.025f, 0.06f, 0.95f);

            var outline = gameObject.GetComponent<Outline>();
            if (outline == null) outline = gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.12f, 0.16f, 0.25f, 0.9f);
            outline.effectDistance = new Vector2(1f, -1f);

            // Add blueprint corner ticks
            CreateCornerTicks(transform);

            // Create Header Bar
            var headerObj = new GameObject("HeaderBar", typeof(RectTransform));
            headerObj.transform.SetParent(transform, false);
            var headerRt = headerObj.GetComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0f, 1f);
            headerRt.anchorMax = new Vector2(1f, 1f);
            headerRt.pivot = new Vector2(0.5f, 1f);
            headerRt.anchoredPosition = new Vector2(0f, -8f);
            headerRt.sizeDelta = new Vector2(-16f, 26f);

            var titleText = CreateText(headerObj.transform, "Title", "ПАНЕЛЬ КЕРУВАННЯ", 11, FontStyles.Bold, new Color(0.8f, 0.85f, 0.92f, 1f), TextAlignmentOptions.Left);
            titleText.rectTransform.anchorMin = new Vector2(0f, 0f);
            titleText.rectTransform.anchorMax = new Vector2(0.6f, 1f);
            titleText.rectTransform.anchoredPosition = Vector2.zero;

            headerCountText = CreateText(headerObj.transform, "CountText", "5 шт.", 10, FontStyles.Normal, new Color(0.45f, 0.5f, 0.6f, 1f), TextAlignmentOptions.Right);
            headerCountText.rectTransform.anchorMin = new Vector2(0.6f, 0f);
            headerCountText.rectTransform.anchorMax = new Vector2(0.88f, 1f);
            headerCountText.rectTransform.anchoredPosition = Vector2.zero;

            minButton = CreateSimpleButton(headerObj.transform, "MinBtn", "_", new Vector2(22f, 20f), new Color(0.08f, 0.1f, 0.16f, 1f), Color.white, 10);
            var minBtnRt = minButton.GetComponent<RectTransform>();
            minBtnRt.anchorMin = new Vector2(1f, 0.5f);
            minBtnRt.anchorMax = new Vector2(1f, 0.5f);
            minBtnRt.pivot = new Vector2(1f, 0.5f);
            minBtnRt.anchoredPosition = Vector2.zero;

            // Minimized View Root
            minimizedRoot = new GameObject("MinimizedRoot", typeof(RectTransform));
            minimizedRoot.transform.SetParent(transform, false);
            var minRt = minimizedRoot.GetComponent<RectTransform>();
            minRt.anchorMin = Vector2.zero;
            minRt.anchorMax = Vector2.one;
            minRt.sizeDelta = new Vector2(-16f, -44f);
            minRt.anchoredPosition = new Vector2(0f, -18f);

            var minRow = new GameObject("MinRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            minRow.transform.SetParent(minimizedRoot.transform, false);
            var minRowRt = minRow.GetComponent<RectTransform>();
            minRowRt.anchorMin = new Vector2(0f, 0.5f);
            minRowRt.anchorMax = new Vector2(1f, 0.5f);
            minRowRt.sizeDelta = new Vector2(0f, 28f);

            miniRemoveCardBtn = CreateSimpleButton(minRow.transform, "MiniRemove", "−", new Vector2(26f, 26f), new Color(0.08f, 0.1f, 0.16f, 1f), Color.white, 12);
            miniCountText = CreateText(minRow.transform, "MiniCount", "5", 12, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
            miniCountText.rectTransform.sizeDelta = new Vector2(34f, 26f);
            miniAddCardBtn = CreateSimpleButton(minRow.transform, "MiniAdd", "+", new Vector2(26f, 26f), new Color(0.08f, 0.1f, 0.16f, 1f), Color.white, 12);
            miniResetHandBtn = CreateSimpleButton(minRow.transform, "MiniReset", "Скинути", new Vector2(80f, 26f), new Color(0.08f, 0.1f, 0.16f, 1f), new Color(0.7f, 0.75f, 0.85f, 1f), 10);
            minimizedRoot.SetActive(false);

            // Expanded Content Root
            contentRoot = new GameObject("ContentRoot", typeof(RectTransform), typeof(VerticalLayoutGroup));
            contentRoot.transform.SetParent(transform, false);
            var contentRt = contentRoot.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = new Vector2(0f, -38f);
            contentRt.sizeDelta = new Vector2(-16f, 0f);

            var vGroup = contentRoot.GetComponent<VerticalLayoutGroup>();
            vGroup.spacing = 8f;
            vGroup.childControlWidth = true;
            vGroup.childControlHeight = false;

            // Top Buttons Bar: [ - 5 + ] [ Скинути руку ]
            var topBar = new GameObject("TopBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            topBar.transform.SetParent(contentRoot.transform, false);
            var topBarRt = topBar.GetComponent<RectTransform>();
            topBarRt.sizeDelta = new Vector2(0f, 26f);
            var topBarLayout = topBar.GetComponent<HorizontalLayoutGroup>();
            topBarLayout.spacing = 6f;
            topBarLayout.childControlWidth = false;
            topBarLayout.childControlHeight = true;

            var counterBox = new GameObject("CounterBox", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(Image));
            counterBox.transform.SetParent(topBar.transform, false);
            counterBox.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.14f, 1f);
            counterBox.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 26f);
            var cLayout = counterBox.GetComponent<HorizontalLayoutGroup>();
            cLayout.childControlWidth = false;
            cLayout.childControlHeight = true;

            removeCardBtn = CreateSimpleButton(counterBox.transform, "RemoveBtn", "−", new Vector2(30f, 26f), Color.clear, new Color(0.9f, 0.4f, 0.4f, 1f), 13);
            countText = CreateText(counterBox.transform, "Count", "5", 12, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
            countText.rectTransform.sizeDelta = new Vector2(40f, 26f);
            addCardBtn = CreateSimpleButton(counterBox.transform, "AddBtn", "+", new Vector2(30f, 26f), Color.clear, new Color(0.3f, 0.85f, 0.5f, 1f), 13);

            resetHandBtn = CreateSimpleButton(topBar.transform, "ResetBtn", "СКИНУТИ РУКУ", new Vector2(195f, 26f), new Color(0.05f, 0.07f, 0.12f, 1f), new Color(0.6f, 0.65f, 0.75f, 1f), 10);

            // CATEGORY 1: CYAN BOX (Ширина карти & Ширина руки)
            var cat1 = CreateCategoryBox("Cat1_Cyan", new Color(0.02f, 0.12f, 0.18f, 0.35f), new Color(0.08f, 0.35f, 0.45f, 0.6f));
            CreateSliderRow(cat1.transform, "Ширина карти (W)", new Color(0.2f, 0.85f, 0.95f, 1f), 60f, 200f, 112f, out cardWidthSlider, out cardWidthInput);
            CreateDivider(cat1.transform, new Color(0.08f, 0.35f, 0.45f, 0.4f));
            CreateSliderRow(cat1.transform, "Ширина руки", new Color(0.2f, 0.85f, 0.95f, 1f), 300f, 1400f, 680f, out handWidthSlider, out handWidthInput);

            // CATEGORY 2: SLATE BOX (Відстань step & Мін. відстань)
            var cat2 = CreateCategoryBox("Cat2_Slate", new Color(0.05f, 0.07f, 0.12f, 0.45f), new Color(0.16f, 0.2f, 0.3f, 0.7f));
            CreateSliderRow(cat2.transform, "Відстань (step)", new Color(0.75f, 0.8f, 0.9f, 1f), 24f, 220f, 56f, out cardDistSlider, out cardDistInput);
            CreateDivider(cat2.transform, new Color(0.16f, 0.2f, 0.3f, 0.5f));
            CreateSliderRow(cat2.transform, "Мін. відстань", new Color(0.75f, 0.8f, 0.9f, 1f), 4f, 100f, 24f, out minCardDistSlider, out minCardDistInput);

            // CATEGORY 3: PINK BOX (Відстань ховеру H & Підйом Lift)
            var cat3 = CreateCategoryBox("Cat3_Pink", new Color(0.16f, 0.02f, 0.1f, 0.35f), new Color(0.45f, 0.1f, 0.3f, 0.6f));
            CreateSliderRow(cat3.transform, "Відстань ховеру (H)", new Color(0.95f, 0.4f, 0.7f, 1f), 112f, 280f, 112f, out hoverDistSlider, out hoverDistInput);
            CreateDivider(cat3.transform, new Color(0.45f, 0.1f, 0.3f, 0.4f));
            CreateSliderRow(cat3.transform, "Підйом (Lift)", new Color(0.95f, 0.4f, 0.7f, 1f), 0f, 80f, 28f, out hoverLiftSlider, out hoverLiftInput);

            // CATEGORY 4: EXTENSIONS & DETAILS TOGGLE
            var cat4 = CreateCategoryBox("Cat4_Extensions", new Color(0.04f, 0.05f, 0.09f, 0.6f), new Color(0.12f, 0.16f, 0.24f, 0.8f));
            var toggleRow = new GameObject("ToggleRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            toggleRow.transform.SetParent(cat4.transform, false);
            toggleRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 26f);
            var trLayout = toggleRow.GetComponent<HorizontalLayoutGroup>();
            trLayout.childControlWidth = false;
            trLayout.childControlHeight = true;

            var toggleLabel = CreateText(toggleRow.transform, "TLabel", "Деталі розмітки (Layout)", 11, FontStyles.Bold, new Color(0.8f, 0.85f, 0.9f, 1f), TextAlignmentOptions.Left);
            toggleLabel.rectTransform.sizeDelta = new Vector2(170f, 26f);

            layoutDetailsBtn = CreateSimpleButton(toggleRow.transform, "DetailsBtn", "[ УВІМКНЕНО ]", new Vector2(120f, 24f), new Color(0.04f, 0.16f, 0.22f, 1f), new Color(0.3f, 0.9f, 1f, 1f), 10);
            layoutDetailsBtnText = layoutDetailsBtn.GetComponentInChildren<TextMeshProUGUI>();

            // TELEMETRY BAR
            var telemetryBox = CreateCategoryBox("Telemetry", new Color(0.02f, 0.03f, 0.06f, 0.9f), new Color(0.1f, 0.14f, 0.22f, 0.9f));
            var telRow = new GameObject("TelRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            telRow.transform.SetParent(telemetryBox.transform, false);
            telRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 20f);
            var telLayout = telRow.GetComponent<HorizontalLayoutGroup>();
            telLayout.childControlWidth = false;
            telLayout.childControlHeight = true;

            telemetryStateText = CreateText(telRow.transform, "TelState", "Стан: Normal", 10, FontStyles.Bold, new Color(0.2f, 0.9f, 0.5f, 1f), TextAlignmentOptions.Left);
            telemetryStateText.rectTransform.sizeDelta = new Vector2(110f, 20f);

            telemetryStepText = CreateText(telRow.transform, "TelStep", "step: 56px", 10, FontStyles.Normal, new Color(0.7f, 0.75f, 0.85f, 1f), TextAlignmentOptions.Center);
            telemetryStepText.rectTransform.sizeDelta = new Vector2(85f, 20f);

            telemetryHStepText = CreateText(telRow.transform, "TelHStep", "h-step: 112px", 10, FontStyles.Bold, new Color(0.95f, 0.4f, 0.7f, 1f), TextAlignmentOptions.Right);
            telemetryHStepText.rectTransform.sizeDelta = new Vector2(95f, 20f);
        }

        private void CreateCornerTicks(Transform parent)
        {
            Vector2[] anchors = { new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(1f, 0f) };
            Vector2[] pivots = { new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(1f, 0f) };

            for (int i = 0; i < 4; i++)
            {
                var tick = new GameObject($"CornerTick_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                tick.transform.SetParent(parent, false);
                var rt = tick.GetComponent<RectTransform>();
                rt.anchorMin = anchors[i];
                rt.anchorMax = anchors[i];
                rt.pivot = pivots[i];
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(8f, 8f);
                tick.GetComponent<Image>().color = new Color(0.4f, 0.5f, 0.65f, 0.8f);
            }
        }

        private GameObject CreateCategoryBox(string name, Color bg, Color border)
        {
            var box = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(Image));
            box.transform.SetParent(contentRoot.transform, false);
            box.GetComponent<Image>().color = bg;
            var outline = box.AddComponent<Outline>();
            outline.effectColor = border;
            outline.effectDistance = new Vector2(1f, -1f);

            var vGroup = box.GetComponent<VerticalLayoutGroup>();
            vGroup.padding = new RectOffset(8, 8, 8, 8);
            vGroup.spacing = 6f;
            vGroup.childControlWidth = true;
            vGroup.childControlHeight = false;

            return box;
        }

        private void CreateDivider(Transform parent, Color color)
        {
            var div = new GameObject("Divider", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            div.transform.SetParent(parent, false);
            div.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 1f);
            div.GetComponent<Image>().color = color;
        }

        private void CreateSliderRow(Transform parent, string label, Color accentColor, float min, float max, float defaultVal, out Slider slider, out TMP_InputField inputField)
        {
            var row = new GameObject("SliderRow_" + label, typeof(RectTransform), typeof(VerticalLayoutGroup));
            row.transform.SetParent(parent, false);
            var vGroup = row.GetComponent<VerticalLayoutGroup>();
            vGroup.spacing = 3f;
            vGroup.childControlWidth = true;
            vGroup.childControlHeight = false;

            // Top Header: Label + Number Input + "px"
            var header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            header.transform.SetParent(row.transform, false);
            header.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 20f);
            var hLayout = header.GetComponent<HorizontalLayoutGroup>();
            hLayout.childControlWidth = false;
            hLayout.childControlHeight = true;

            var lbl = CreateText(header.transform, "Label", label, 11, FontStyles.Bold, accentColor, TextAlignmentOptions.Left);
            lbl.rectTransform.sizeDelta = new Vector2(210f, 20f);

            var inputObj = new GameObject("Input", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TMP_InputField));
            inputObj.transform.SetParent(header.transform, false);
            var inRt = inputObj.GetComponent<RectTransform>();
            inRt.sizeDelta = new Vector2(52f, 18f);
            inputObj.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.1f, 1f);

            var textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(inputObj.transform, false);
            var textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;
            var inTmp = textObj.GetComponent<TextMeshProUGUI>();
            inTmp.fontSize = 11;
            inTmp.alignment = TextAlignmentOptions.Right;
            inTmp.color = accentColor;

            inputField = inputObj.GetComponent<TMP_InputField>();
            inputField.textComponent = inTmp;
            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            inputField.text = Mathf.RoundToInt(defaultVal).ToString();

            var pxLbl = CreateText(header.transform, "Px", "px", 10, FontStyles.Normal, new Color(0.4f, 0.5f, 0.6f, 1f), TextAlignmentOptions.Left);
            pxLbl.rectTransform.sizeDelta = new Vector2(18f, 20f);

            // Slider Graphic
            var sliderObj = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            sliderObj.transform.SetParent(row.transform, false);
            sliderObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 12f);

            var bgTrack = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgTrack.transform.SetParent(sliderObj.transform, false);
            var bgRt = bgTrack.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0.4f);
            bgRt.anchorMax = new Vector2(1f, 0.6f);
            bgRt.sizeDelta = Vector2.zero;
            bgTrack.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.16f, 1f);

            var fillArea = new GameObject("FillArea", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObj.transform, false);
            var faRt = fillArea.GetComponent<RectTransform>();
            faRt.anchorMin = new Vector2(0f, 0.4f);
            faRt.anchorMax = new Vector2(1f, 0.6f);
            faRt.sizeDelta = Vector2.zero;

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.sizeDelta = Vector2.zero;
            fill.GetComponent<Image>().color = accentColor;

            var handleArea = new GameObject("HandleArea", typeof(RectTransform));
            handleArea.transform.SetParent(sliderObj.transform, false);
            var haRt = handleArea.GetComponent<RectTransform>();
            haRt.anchorMin = Vector2.zero;
            haRt.anchorMax = Vector2.one;
            haRt.sizeDelta = Vector2.zero;

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            var hRt = handle.GetComponent<RectTransform>();
            hRt.sizeDelta = new Vector2(12f, 12f);
            handle.GetComponent<Image>().color = Color.white;

            slider = sliderObj.GetComponent<Slider>();
            slider.fillRect = fillRt;
            slider.handleRect = hRt;
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = defaultVal;
        }

        private TextMeshProUGUI CreateText(Transform parent, string name, string text, float size, FontStyles style, Color color, TextAlignmentOptions align)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);
            var tmp = obj.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = align;
            return tmp;
        }

        private Button CreateSimpleButton(Transform parent, string name, string label, Vector2 size, Color bg, Color textCol, float fontSize)
        {
            var btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            var rt = btnObj.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            btnObj.GetComponent<Image>().color = bg;

            var outline = btnObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.18f, 0.22f, 0.35f, 0.8f);
            outline.effectDistance = new Vector2(1f, -1f);

            var txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(btnObj.transform, false);
            var txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            var tmp = txtObj.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = textCol;
            tmp.alignment = TextAlignmentOptions.Center;

            return btnObj.GetComponent<Button>();
        }

        private void WireEvents()
        {
            if (minButton != null)
            {
                minButton.onClick.AddListener(ToggleMinimize);
            }

            if (addCardBtn != null) addCardBtn.onClick.AddListener(() => AddCard());
            if (miniAddCardBtn != null) miniAddCardBtn.onClick.AddListener(() => AddCard());
            if (removeCardBtn != null) removeCardBtn.onClick.AddListener(() => RemoveCard());
            if (miniRemoveCardBtn != null) miniRemoveCardBtn.onClick.AddListener(() => RemoveCard());
            if (resetHandBtn != null) resetHandBtn.onClick.AddListener(() => ResetHand());
            if (miniResetHandBtn != null) miniResetHandBtn.onClick.AddListener(() => ResetHand());

            // Wire Card Width
            if (cardWidthSlider != null && cardWidthInput != null)
            {
                cardWidthSlider.onValueChanged.AddListener((v) =>
                {
                    if (isUpdatingUI) return;
                    isUpdatingUI = true;
                    if (handController != null) handController.Settings.cardWidth = v;
                    cardWidthInput.text = Mathf.RoundToInt(v).ToString();
                    RefreshCardVisuals();
                    isUpdatingUI = false;
                });
                cardWidthInput.onEndEdit.AddListener((s) =>
                {
                    if (float.TryParse(s, out float val))
                    {
                        cardWidthSlider.value = Mathf.Clamp(val, cardWidthSlider.minValue, cardWidthSlider.maxValue);
                    }
                });
            }

            // Wire Hand Width
            if (handWidthSlider != null && handWidthInput != null)
            {
                handWidthSlider.onValueChanged.AddListener((v) =>
                {
                    if (isUpdatingUI) return;
                    isUpdatingUI = true;
                    if (handController != null) handController.Settings.handWidth = v;
                    handWidthInput.text = Mathf.RoundToInt(v).ToString();
                    isUpdatingUI = false;
                });
                handWidthInput.onEndEdit.AddListener((s) =>
                {
                    if (float.TryParse(s, out float val))
                    {
                        handWidthSlider.value = Mathf.Clamp(val, handWidthSlider.minValue, handWidthSlider.maxValue);
                    }
                });
            }

            // Wire Card Distance
            if (cardDistSlider != null && cardDistInput != null)
            {
                cardDistSlider.onValueChanged.AddListener((v) =>
                {
                    if (isUpdatingUI) return;
                    isUpdatingUI = true;
                    if (handController != null) handController.Settings.cardDistance = v;
                    cardDistInput.text = Mathf.RoundToInt(v).ToString();
                    isUpdatingUI = false;
                });
                cardDistInput.onEndEdit.AddListener((s) =>
                {
                    if (float.TryParse(s, out float val))
                    {
                        cardDistSlider.value = Mathf.Clamp(val, cardDistSlider.minValue, cardDistSlider.maxValue);
                    }
                });
            }

            // Wire Min Card Distance
            if (minCardDistSlider != null && minCardDistInput != null)
            {
                minCardDistSlider.onValueChanged.AddListener((v) =>
                {
                    if (isUpdatingUI) return;
                    isUpdatingUI = true;
                    if (handController != null) handController.Settings.minCardDistance = v;
                    minCardDistInput.text = Mathf.RoundToInt(v).ToString();
                    isUpdatingUI = false;
                });
                minCardDistInput.onEndEdit.AddListener((s) =>
                {
                    if (float.TryParse(s, out float val))
                    {
                        minCardDistSlider.value = Mathf.Clamp(val, minCardDistSlider.minValue, minCardDistSlider.maxValue);
                    }
                });
            }

            // Wire Hover Distance
            if (hoverDistSlider != null && hoverDistInput != null)
            {
                hoverDistSlider.onValueChanged.AddListener((v) =>
                {
                    if (isUpdatingUI) return;
                    isUpdatingUI = true;
                    if (handController != null) handController.Settings.hoverDistance = v;
                    hoverDistInput.text = Mathf.RoundToInt(v).ToString();
                    isUpdatingUI = false;
                });
                hoverDistInput.onEndEdit.AddListener((s) =>
                {
                    if (float.TryParse(s, out float val))
                    {
                        hoverDistSlider.value = Mathf.Clamp(val, hoverDistSlider.minValue, hoverDistSlider.maxValue);
                    }
                });
            }

            // Wire Hover Lift
            if (hoverLiftSlider != null && hoverLiftInput != null)
            {
                hoverLiftSlider.onValueChanged.AddListener((v) =>
                {
                    if (isUpdatingUI) return;
                    isUpdatingUI = true;
                    if (handController != null) handController.Settings.hoverLift = v;
                    hoverLiftInput.text = Mathf.RoundToInt(v).ToString();
                    isUpdatingUI = false;
                });
                hoverLiftInput.onEndEdit.AddListener((s) =>
                {
                    if (float.TryParse(s, out float val))
                    {
                        hoverLiftSlider.value = Mathf.Clamp(val, hoverLiftSlider.minValue, hoverLiftSlider.maxValue);
                    }
                });
            }

            // Wire Details Toggle
            if (layoutDetailsBtn != null)
            {
                layoutDetailsBtn.onClick.AddListener(ToggleLayoutDetails);
            }

            if (handController != null)
            {
                handController.onHandUpdated.AddListener(RefreshAllValues);
            }
        }

        private void ToggleMinimize()
        {
            isMinimized = !isMinimized;
            if (contentRoot != null) contentRoot.SetActive(!isMinimized);
            if (minimizedRoot != null) minimizedRoot.SetActive(isMinimized);

            if (panelRt != null)
            {
                panelRt.sizeDelta = isMinimized ? new Vector2(230f, 70f) : new Vector2(330f, 540f);
            }
            if (minButton != null)
            {
                var txt = minButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = isMinimized ? "□" : "_";
            }
        }

        private void ToggleLayoutDetails()
        {
            if (handController == null) return;
            handController.Settings.showLayoutDetails = !handController.Settings.showLayoutDetails;
            RefreshDetailsButton();
            RefreshCardVisuals();
        }

        private void RefreshDetailsButton()
        {
            if (handController == null || layoutDetailsBtnText == null) return;
            bool on = handController.Settings.showLayoutDetails;
            layoutDetailsBtnText.text = on ? "[ УВІМКНЕНО ]" : "[ ВИМКНЕНО ]";
            layoutDetailsBtnText.color = on ? new Color(0.3f, 0.9f, 1f, 1f) : new Color(0.5f, 0.55f, 0.65f, 1f);
        }

        private void RefreshCardVisuals()
        {
            if (handController == null) return;
            foreach (var card in handController.CardViews)
            {
                if (card != null) card.UpdateVisuals();
            }
        }

        private void AddCard()
        {
            if (handController != null) handController.AddCard();
            RefreshAllValues();
        }

        private void RemoveCard()
        {
            if (handController != null) handController.RemoveLastCard();
            RefreshAllValues();
        }

        private void ResetHand()
        {
            if (handController != null) handController.CreateDefaultCards(5);
            RefreshAllValues();
        }

        public void RefreshAllValues()
        {
            if (handController == null) return;

            int count = handController.CardsCount;
            if (headerCountText != null) headerCountText.text = $"{count} шт.";
            if (miniCountText != null) miniCountText.text = $"{count}";
            if (countText != null) countText.text = $"{count}";

            isUpdatingUI = true;
            var s = handController.Settings;
            if (cardWidthSlider != null) { cardWidthSlider.value = s.cardWidth; if (cardWidthInput != null) cardWidthInput.text = Mathf.RoundToInt(s.cardWidth).ToString(); }
            if (handWidthSlider != null) { handWidthSlider.value = s.handWidth; if (handWidthInput != null) handWidthInput.text = Mathf.RoundToInt(s.handWidth).ToString(); }
            if (cardDistSlider != null) { cardDistSlider.value = s.cardDistance; if (cardDistInput != null) cardDistInput.text = Mathf.RoundToInt(s.cardDistance).ToString(); }
            if (minCardDistSlider != null) { minCardDistSlider.value = s.minCardDistance; if (minCardDistInput != null) minCardDistInput.text = Mathf.RoundToInt(s.minCardDistance).ToString(); }
            if (hoverDistSlider != null) { hoverDistSlider.value = s.hoverDistance; if (hoverDistInput != null) hoverDistInput.text = Mathf.RoundToInt(s.hoverDistance).ToString(); }
            if (hoverLiftSlider != null) { hoverLiftSlider.value = s.hoverLift; if (hoverLiftInput != null) hoverLiftInput.text = Mathf.RoundToInt(s.hoverLift).ToString(); }

            RefreshDetailsButton();
            isUpdatingUI = false;
        }

        private void UpdateTelemetry()
        {
            if (handController == null) return;

            var metrics = CardHandLayoutEngine.ComputeHandMetrics(handController.CardsCount, handController.Settings, null);

            if (telemetryStateText != null)
            {
                telemetryStateText.text = metrics.IsOverflowing ? "Стан: Overflow" : "Стан: Normal";
                telemetryStateText.color = metrics.IsOverflowing ? new Color(0.95f, 0.75f, 0.25f, 1f) : new Color(0.2f, 0.9f, 0.5f, 1f);
            }

            if (telemetryStepText != null)
            {
                telemetryStepText.text = $"step: {Mathf.RoundToInt(metrics.EffectiveDistance)}px";
            }

            if (telemetryHStepText != null)
            {
                telemetryHStepText.text = $"h-step: {Mathf.RoundToInt(metrics.CompressedHoverDistance)}px";
            }
        }
    }
}
