using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Tetromiko.CardsHandLayout.Samples
{
    [RequireComponent(typeof(RectTransform))]
    public class CardHandWebControlPanel : MonoBehaviour
    {
        [Header("Controller Reference")]
        [SerializeField] private CardHandController handController;

        [Header("Optional Icons / Sprites (Leave empty for default placeholders)")]
        [Tooltip("Icon for '+ Add Card' button. If empty, Image component remains blank ready for sprite assignment.")]
        [SerializeField] private Sprite iconAdd;

        [Tooltip("Icon for '− Remove Card' button. If empty, Image component remains blank ready for sprite assignment.")]
        [SerializeField] private Sprite iconRemove;

        [Tooltip("Icon for 'Reset Hand' button. If empty, Image component remains blank ready for sprite assignment.")]
        [SerializeField] private Sprite iconReset;

        [Tooltip("Icon for 'Minimize Panel' button. If empty, Image component remains blank ready for sprite assignment.")]
        [SerializeField] private Sprite iconMinimize;

        [Tooltip("Icon for 'Expand Panel' button. If empty, Image component remains blank ready for sprite assignment.")]
        [SerializeField] private Sprite iconExpand;

        // Runtime References
        private RectTransform panelRt;
        private bool isMinimized = false;
        private GameObject contentRoot;
        private GameObject minimizedRoot;

        // Header and Counter UI
        private TextMeshProUGUI headerCountText;
        private TextMeshProUGUI miniCountText;
        private TextMeshProUGUI countText;
        private Button minButton;
        private Image minButtonIconImage;
        private Button addCardBtn;
        private Image addCardIconImage;
        private Button removeCardBtn;
        private Image removeCardIconImage;
        private Button resetHandBtn;
        private Image resetHandIconImage;

        private Button miniAddCardBtn;
        private Image miniAddCardIconImage;
        private Button miniRemoveCardBtn;
        private Image miniRemoveCardIconImage;
        private Button miniResetHandBtn;
        private Image miniResetHandIconImage;

        // Sliders & Number Input Pairs
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
            RebuildPanelLayout();
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
                RebuildPanelLayout();
                WireEvents();
                RefreshAllValues();
            }
        }

        private void Update()
        {
            if (handController == null) return;
            UpdateTelemetry();
        }

        public void RebuildPanelLayout()
        {
            // Clear any old child objects
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }

            // 1. Root Panel RectTransform & Layout
            panelRt = GetComponent<RectTransform>();
            if (panelRt == null) panelRt = gameObject.AddComponent<RectTransform>();

            panelRt.anchorMin = new Vector2(0f, 1f);
            panelRt.anchorMax = new Vector2(0f, 1f);
            panelRt.pivot = new Vector2(0f, 1f);
            panelRt.anchoredPosition = new Vector2(16f, -16f);
            panelRt.sizeDelta = new Vector2(340f, 0f);

            var rootLayout = GetComponent<VerticalLayoutGroup>();
            if (rootLayout == null) rootLayout = gameObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(12, 12, 12, 12);
            rootLayout.spacing = 8f;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;

            var rootFitter = GetComponent<ContentSizeFitter>();
            if (rootFitter == null) rootFitter = gameObject.AddComponent<ContentSizeFitter>();
            rootFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            rootFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var bgImg = GetComponent<Image>();
            if (bgImg == null) bgImg = gameObject.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.06f, 0.10f, 0.96f);

            var outline = GetComponent<Outline>();
            if (outline == null) outline = gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.15f, 0.20f, 0.30f, 0.9f);
            outline.effectDistance = new Vector2(1f, -1f);

            // 2. Header Bar
            BuildHeaderBar();

            // 3. Minimized View Container
            BuildMinimizedRoot();

            // 4. Expanded Content Container
            BuildExpandedContent();
        }

        private void BuildHeaderBar()
        {
            var headerObj = CreateLayoutObject("HeaderBar", transform, 28f);
            var hGroup = headerObj.AddComponent<HorizontalLayoutGroup>();
            hGroup.spacing = 8f;
            hGroup.childControlWidth = false;
            hGroup.childControlHeight = true;
            hGroup.childForceExpandWidth = false;
            hGroup.childForceExpandHeight = true;
            hGroup.childAlignment = TextAnchor.MiddleLeft;

            // Title Label
            var title = CreateText(headerObj.transform, "Title", "ПАНЕЛЬ КЕРУВАННЯ", 11, FontStyles.Bold, new Color(0.85f, 0.90f, 0.96f, 1f), TextAlignmentOptions.MidlineLeft);
            var titleLe = title.gameObject.AddComponent<LayoutElement>();
            titleLe.flexibleWidth = 1f;

            // Card Count Badge
            var badgeObj = CreateLayoutObject("CountBadge", headerObj.transform, 22f);
            var badgeBg = badgeObj.AddComponent<Image>();
            badgeBg.color = new Color(0.10f, 0.14f, 0.22f, 1f);
            var badgeLayout = badgeObj.AddComponent<HorizontalLayoutGroup>();
            badgeLayout.padding = new RectOffset(6, 6, 2, 2);
            badgeLayout.childControlWidth = true;
            badgeLayout.childControlHeight = true;
            badgeLayout.childForceExpandWidth = false;
            badgeLayout.childForceExpandHeight = true;
            badgeObj.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            headerCountText = CreateText(badgeObj.transform, "Text", "5 шт.", 10, FontStyles.Normal, new Color(0.55f, 0.65f, 0.80f, 1f), TextAlignmentOptions.Center);

            // Minimize / Expand Toggle Button
            minButton = CreateIconButton(headerObj.transform, "MinButton", new Vector2(24f, 24f), new Color(0.10f, 0.14f, 0.22f, 1f), iconMinimize, out minButtonIconImage);
        }

        private void BuildMinimizedRoot()
        {
            minimizedRoot = CreateLayoutObject("MinimizedRoot", transform, 32f);
            var hGroup = minimizedRoot.AddComponent<HorizontalLayoutGroup>();
            hGroup.spacing = 8f;
            hGroup.childControlWidth = false;
            hGroup.childControlHeight = true;
            hGroup.childForceExpandWidth = false;
            hGroup.childForceExpandHeight = true;
            hGroup.childAlignment = TextAnchor.MiddleCenter;

            miniRemoveCardBtn = CreateIconButton(minimizedRoot.transform, "MiniRemove", new Vector2(30f, 30f), new Color(0.12f, 0.16f, 0.25f, 1f), iconRemove, out miniRemoveCardIconImage);
            miniCountText = CreateText(minimizedRoot.transform, "MiniCount", "5", 12, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
            miniCountText.gameObject.AddComponent<LayoutElement>().preferredWidth = 32f;
            miniAddCardBtn = CreateIconButton(minimizedRoot.transform, "MiniAdd", new Vector2(30f, 30f), new Color(0.12f, 0.16f, 0.25f, 1f), iconAdd, out miniAddCardIconImage);

            miniResetHandBtn = CreateButton(minimizedRoot.transform, "MiniReset", "Скинути", new Vector2(80f, 30f), new Color(0.12f, 0.16f, 0.25f, 1f), new Color(0.75f, 0.82f, 0.92f, 1f), 10f);
            var resetLe = miniResetHandBtn.gameObject.AddComponent<LayoutElement>();
            resetLe.flexibleWidth = 1f;

            minimizedRoot.SetActive(false);
        }

        private void BuildExpandedContent()
        {
            contentRoot = CreateLayoutObject("ContentRoot", transform, -1f);
            var vGroup = contentRoot.AddComponent<VerticalLayoutGroup>();
            vGroup.spacing = 8f;
            vGroup.childControlWidth = true;
            vGroup.childControlHeight = true;
            vGroup.childForceExpandWidth = true;
            vGroup.childForceExpandHeight = false;

            var fitter = contentRoot.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 1. Top Buttons Bar [ - 5 + ] [ Скинути руку ]
            var topBar = CreateLayoutObject("TopBar", contentRoot.transform, 30f);
            var topBarH = topBar.AddComponent<HorizontalLayoutGroup>();
            topBarH.spacing = 8f;
            topBarH.childControlWidth = false;
            topBarH.childControlHeight = true;
            topBarH.childForceExpandWidth = false;
            topBarH.childForceExpandHeight = true;

            // Counter Box
            var counterBox = CreateLayoutObject("CounterBox", topBar.transform, 30f);
            counterBox.AddComponent<Image>().color = new Color(0.08f, 0.11f, 0.18f, 1f);
            var cbOutline = counterBox.AddComponent<Outline>();
            cbOutline.effectColor = new Color(0.16f, 0.22f, 0.32f, 0.8f);
            cbOutline.effectDistance = new Vector2(1f, -1f);

            var cbLayout = counterBox.AddComponent<HorizontalLayoutGroup>();
            cbLayout.padding = new RectOffset(2, 2, 2, 2);
            cbLayout.spacing = 2f;
            cbLayout.childControlWidth = false;
            cbLayout.childControlHeight = true;
            cbLayout.childForceExpandWidth = false;
            cbLayout.childForceExpandHeight = true;
            var cbLe = counterBox.AddComponent<LayoutElement>();
            cbLe.preferredWidth = 110f;

            removeCardBtn = CreateIconButton(counterBox.transform, "RemoveBtn", new Vector2(28f, 26f), Color.clear, iconRemove, out removeCardIconImage);
            countText = CreateText(counterBox.transform, "Count", "5", 12, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
            countText.gameObject.AddComponent<LayoutElement>().preferredWidth = 46f;
            addCardBtn = CreateIconButton(counterBox.transform, "AddBtn", new Vector2(28f, 26f), Color.clear, iconAdd, out addCardIconImage);

            // Reset Button
            resetHandBtn = CreateButton(topBar.transform, "ResetBtn", "СКИНУТИ РУКУ", new Vector2(0f, 30f), new Color(0.08f, 0.11f, 0.18f, 1f), new Color(0.70f, 0.76f, 0.88f, 1f), 10f);
            var resetLe = resetHandBtn.gameObject.AddComponent<LayoutElement>();
            resetLe.flexibleWidth = 1f;

            // 2. Category 1: Card & Hand Geometry (Cyan Accents)
            var cat1 = CreateSectionCard("Cat1_Geometry", contentRoot.transform, new Color(0.03f, 0.10f, 0.15f, 0.40f), new Color(0.10f, 0.35f, 0.45f, 0.6f));
            CreateSliderRow(cat1.transform, "Ширина карти (W)", new Color(0.25f, 0.85f, 0.95f, 1f), 60f, 200f, 112f, out cardWidthSlider, out cardWidthInput);
            CreateDivider(cat1.transform, new Color(0.10f, 0.35f, 0.45f, 0.3f));
            CreateSliderRow(cat1.transform, "Ширина руки", new Color(0.25f, 0.85f, 0.95f, 1f), 300f, 1400f, 680f, out handWidthSlider, out handWidthInput);

            // 3. Category 2: Distance & Step Compression (Slate Accents)
            var cat2 = CreateSectionCard("Cat2_Distance", contentRoot.transform, new Color(0.06f, 0.08f, 0.13f, 0.50f), new Color(0.18f, 0.24f, 0.35f, 0.7f));
            CreateSliderRow(cat2.transform, "Відстань (step)", new Color(0.78f, 0.84f, 0.92f, 1f), 24f, 220f, 56f, out cardDistSlider, out cardDistInput);
            CreateDivider(cat2.transform, new Color(0.18f, 0.24f, 0.35f, 0.3f));
            CreateSliderRow(cat2.transform, "Мін. відстань", new Color(0.78f, 0.84f, 0.92f, 1f), 4f, 100f, 24f, out minCardDistSlider, out minCardDistInput);

            // 4. Category 3: Hover Expansion & Lift (Pink Accents)
            var cat3 = CreateSectionCard("Cat3_Hover", contentRoot.transform, new Color(0.14f, 0.04f, 0.10f, 0.40f), new Color(0.45f, 0.12f, 0.30f, 0.6f));
            CreateSliderRow(cat3.transform, "Відстань ховеру (H)", new Color(0.95f, 0.45f, 0.72f, 1f), 112f, 280f, 112f, out hoverDistSlider, out hoverDistInput);
            CreateDivider(cat3.transform, new Color(0.45f, 0.12f, 0.30f, 0.3f));
            CreateSliderRow(cat3.transform, "Підйом (Lift)", new Color(0.95f, 0.45f, 0.72f, 1f), 0f, 80f, 28f, out hoverLiftSlider, out hoverLiftInput);

            // 5. Category 4: Extensions & Details Toggle
            var cat4 = CreateSectionCard("Cat4_Extensions", contentRoot.transform, new Color(0.05f, 0.07f, 0.11f, 0.6f), new Color(0.14f, 0.18f, 0.26f, 0.8f));
            var toggleRow = CreateLayoutObject("ToggleRow", cat4.transform, 28f);
            var trH = toggleRow.AddComponent<HorizontalLayoutGroup>();
            trH.spacing = 8f;
            trH.childControlWidth = false;
            trH.childControlHeight = true;
            trH.childForceExpandWidth = false;
            trH.childForceExpandHeight = true;
            trH.childAlignment = TextAnchor.MiddleLeft;

            var toggleLbl = CreateText(toggleRow.transform, "Label", "Деталі розмітки (Layout)", 11, FontStyles.Bold, new Color(0.82f, 0.87f, 0.94f, 1f), TextAlignmentOptions.MidlineLeft);
            toggleLbl.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            layoutDetailsBtn = CreateButton(toggleRow.transform, "DetailsBtn", "[ УВІМКНЕНО ]", new Vector2(115f, 26f), new Color(0.06f, 0.18f, 0.24f, 1f), new Color(0.30f, 0.90f, 1f, 1f), 10f);
            layoutDetailsBtnText = layoutDetailsBtn.GetComponentInChildren<TextMeshProUGUI>();

            // 6. Telemetry Status Bar
            var telBox = CreateSectionCard("Telemetry", contentRoot.transform, new Color(0.03f, 0.04f, 0.07f, 0.9f), new Color(0.12f, 0.16f, 0.24f, 0.9f));
            var telRow = CreateLayoutObject("TelRow", telBox.transform, 20f);
            var telH = telRow.AddComponent<HorizontalLayoutGroup>();
            telH.spacing = 6f;
            telH.childControlWidth = false;
            telH.childControlHeight = true;
            telH.childForceExpandWidth = false;
            telH.childForceExpandHeight = true;
            telH.childAlignment = TextAnchor.MiddleLeft;

            telemetryStateText = CreateText(telRow.transform, "TelState", "Стан: Normal", 10, FontStyles.Bold, new Color(0.25f, 0.90f, 0.55f, 1f), TextAlignmentOptions.MidlineLeft);
            telemetryStateText.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1.2f;

            telemetryStepText = CreateText(telRow.transform, "TelStep", "step: 56px", 10, FontStyles.Normal, new Color(0.70f, 0.76f, 0.86f, 1f), TextAlignmentOptions.MidlineCenter);
            telemetryStepText.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            telemetryHStepText = CreateText(telRow.transform, "TelHStep", "h-step: 112px", 10, FontStyles.Bold, new Color(0.95f, 0.45f, 0.72f, 1f), TextAlignmentOptions.MidlineRight);
            telemetryHStepText.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        }

        private GameObject CreateLayoutObject(string name, Transform parent, float preferredHeight = -1f)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            if (preferredHeight > 0f)
            {
                var le = obj.AddComponent<LayoutElement>();
                le.preferredHeight = preferredHeight;
                le.minHeight = preferredHeight;
            }
            return obj;
        }

        private GameObject CreateSectionCard(string name, Transform parent, Color bg, Color border)
        {
            var card = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            card.transform.SetParent(parent, false);
            card.GetComponent<Image>().color = bg;

            var outline = card.AddComponent<Outline>();
            outline.effectColor = border;
            outline.effectDistance = new Vector2(1f, -1f);

            var vGroup = card.AddComponent<VerticalLayoutGroup>();
            vGroup.padding = new RectOffset(10, 10, 8, 8);
            vGroup.spacing = 6f;
            vGroup.childControlWidth = true;
            vGroup.childControlHeight = true;
            vGroup.childForceExpandWidth = true;
            vGroup.childForceExpandHeight = false;

            var fitter = card.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return card;
        }

        private void CreateDivider(Transform parent, Color color)
        {
            var div = new GameObject("Divider", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            div.transform.SetParent(parent, false);
            div.GetComponent<Image>().color = color;
            var le = div.AddComponent<LayoutElement>();
            le.preferredHeight = 1f;
            le.minHeight = 1f;
        }

        private void CreateSliderRow(Transform parent, string label, Color accentColor, float min, float max, float defaultVal, out Slider slider, out TMP_InputField inputField)
        {
            var row = CreateLayoutObject("SliderRow_" + label, parent, -1f);
            var vGroup = row.AddComponent<VerticalLayoutGroup>();
            vGroup.spacing = 3f;
            vGroup.childControlWidth = true;
            vGroup.childControlHeight = true;
            vGroup.childForceExpandWidth = true;
            vGroup.childForceExpandHeight = false;

            var fitter = row.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Row 1: Header [ Label (flex) | InputField (56px) | Unit (18px) ]
            var header = CreateLayoutObject("Header", row.transform, 20f);
            var hGroup = header.AddComponent<HorizontalLayoutGroup>();
            hGroup.spacing = 4f;
            hGroup.childControlWidth = false;
            hGroup.childControlHeight = true;
            hGroup.childForceExpandWidth = false;
            hGroup.childForceExpandHeight = true;
            hGroup.childAlignment = TextAnchor.MiddleLeft;

            var lbl = CreateText(header.transform, "Label", label, 11, FontStyles.Bold, accentColor, TextAlignmentOptions.MidlineLeft);
            lbl.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            // Value Input Field
            var inputObj = CreateLayoutObject("Input", header.transform, 20f);
            inputObj.AddComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 1f);
            var inOutline = inputObj.AddComponent<Outline>();
            inOutline.effectColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.4f);
            inOutline.effectDistance = new Vector2(1f, -1f);

            var inLe = inputObj.AddComponent<LayoutElement>();
            inLe.preferredWidth = 56f;
            inLe.minWidth = 56f;

            var textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(inputObj.transform, false);
            var textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            var inTmp = textObj.GetComponent<TextMeshProUGUI>();
            inTmp.fontSize = 11;
            inTmp.alignment = TextAlignmentOptions.MidlineRight;
            inTmp.color = accentColor;

            inputField = inputObj.AddComponent<TMP_InputField>();
            inputField.textComponent = inTmp;
            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            inputField.text = Mathf.RoundToInt(defaultVal).ToString();

            // Unit Label ("px")
            var pxLbl = CreateText(header.transform, "Px", "px", 10, FontStyles.Normal, new Color(0.45f, 0.52f, 0.65f, 1f), TextAlignmentOptions.MidlineLeft);
            pxLbl.gameObject.AddComponent<LayoutElement>().preferredWidth = 18f;

            // Row 2: Standard uGUI Slider
            var sliderObj = CreateLayoutObject("Slider", row.transform, 16f);
            var sliderLe = sliderObj.AddComponent<LayoutElement>();
            sliderLe.preferredHeight = 16f;
            sliderLe.minHeight = 16f;

            // Background Track
            var bgTrack = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgTrack.transform.SetParent(sliderObj.transform, false);
            var bgRt = bgTrack.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0.35f);
            bgRt.anchorMax = new Vector2(1f, 0.65f);
            bgRt.sizeDelta = Vector2.zero;
            bgTrack.GetComponent<Image>().color = new Color(0.08f, 0.12f, 0.18f, 1f);

            // Fill Area & Fill
            var fillArea = new GameObject("FillArea", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObj.transform, false);
            var faRt = fillArea.GetComponent<RectTransform>();
            faRt.anchorMin = new Vector2(0f, 0.35f);
            faRt.anchorMax = new Vector2(1f, 0.65f);
            faRt.sizeDelta = Vector2.zero;

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.sizeDelta = Vector2.zero;
            fill.GetComponent<Image>().color = accentColor;

            // Handle Slide Area & Handle
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

            slider = sliderObj.AddComponent<Slider>();
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

        private Button CreateButton(Transform parent, string name, string label, Vector2 size, Color bg, Color textCol, float fontSize)
        {
            var btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            var rt = btnObj.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            btnObj.GetComponent<Image>().color = bg;

            var outline = btnObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.18f, 0.24f, 0.36f, 0.8f);
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

        private Button CreateIconButton(Transform parent, string name, Vector2 size, Color bg, Sprite sprite, out Image iconImage)
        {
            var btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            var rt = btnObj.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            btnObj.GetComponent<Image>().color = bg;

            var outline = btnObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.18f, 0.24f, 0.36f, 0.8f);
            outline.effectDistance = new Vector2(1f, -1f);

            var le = btnObj.AddComponent<LayoutElement>();
            le.preferredWidth = size.x;
            le.preferredHeight = size.y;
            le.minWidth = size.x;
            le.minHeight = size.y;

            // Icon Image Object
            var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObj.transform.SetParent(btnObj.transform, false);
            var iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.2f, 0.2f);
            iconRt.anchorMax = new Vector2(0.8f, 0.8f);
            iconRt.sizeDelta = Vector2.zero;

            iconImage = iconObj.GetComponent<Image>();
            iconImage.raycastTarget = false;
            iconImage.sprite = sprite;
            iconImage.color = new Color(0.85f, 0.90f, 0.98f, 1f);

            return btnObj.GetComponent<Button>();
        }

        private void WireEvents()
        {
            if (minButton != null)
            {
                minButton.onClick.AddListener(ToggleMinimize);
            }

            if (addCardBtn != null) addCardBtn.onClick.AddListener(AddCard);
            if (miniAddCardBtn != null) miniAddCardBtn.onClick.AddListener(AddCard);
            if (removeCardBtn != null) removeCardBtn.onClick.AddListener(RemoveCard);
            if (miniRemoveCardBtn != null) miniRemoveCardBtn.onClick.AddListener(RemoveCard);
            if (resetHandBtn != null) resetHandBtn.onClick.AddListener(ResetHand);
            if (miniResetHandBtn != null) miniResetHandBtn.onClick.AddListener(ResetHand);

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

            if (minButtonIconImage != null)
            {
                minButtonIconImage.sprite = isMinimized ? iconExpand : iconMinimize;
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
            layoutDetailsBtnText.color = on ? new Color(0.30f, 0.90f, 1f, 1f) : new Color(0.50f, 0.55f, 0.65f, 1f);
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
                telemetryStateText.color = metrics.IsOverflowing ? new Color(0.95f, 0.75f, 0.25f, 1f) : new Color(0.25f, 0.90f, 0.55f, 1f);
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
