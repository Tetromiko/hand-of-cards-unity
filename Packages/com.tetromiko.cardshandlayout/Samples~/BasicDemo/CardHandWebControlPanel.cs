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

        [Header("Optional Icons (Leave empty to use text labels)")]
        [SerializeField] private Sprite iconAdd;
        [SerializeField] private Sprite iconRemove;
        [SerializeField] private Sprite iconReset;
        [SerializeField] private Sprite iconMinimize;
        [SerializeField] private Sprite iconExpand;

        // UI References
        private RectTransform panelRt;
        private bool isMinimized = false;
        private GameObject contentRoot;
        private GameObject minimizedRoot;

        // Header and Counter
        private TextMeshProUGUI headerCountText;
        private TextMeshProUGUI miniCountText;
        private TextMeshProUGUI countText;
        private Button minButton;
        private TextMeshProUGUI minButtonText;

        private Button addCardBtn;
        private Button removeCardBtn;
        private Button resetHandBtn;

        private Button miniAddCardBtn;
        private Button miniRemoveCardBtn;
        private Button miniResetHandBtn;

        // Sliders & Input Fields
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

        // Toggle Details
        private Button layoutDetailsBtn;
        private TextMeshProUGUI layoutDetailsBtnText;

        // Telemetry
        private TextMeshProUGUI telemetryStateText;
        private TextMeshProUGUI telemetryStepText;
        private TextMeshProUGUI telemetryHStepText;

        private bool isUpdatingUI = false;

        private const float PANEL_WIDTH = 340f;

        public void Initialize(CardHandController controller)
        {
            this.handController = controller;
            RebuildPanelLayout();
            WireEvents();
            RefreshAllValues();
        }

        private void Awake()
        {
            if (handController == null)
            {
#if UNITY_2023_1_OR_NEWER
                handController = UnityEngine.Object.FindAnyObjectByType<CardHandController>();
#else
                handController = UnityEngine.Object.FindObjectOfType<CardHandController>();
#endif
            }
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
            // Clean up existing children
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }

            // 1. Root Panel
            panelRt = GetComponent<RectTransform>();
            if (panelRt == null) panelRt = gameObject.AddComponent<RectTransform>();

            panelRt.anchorMin = new Vector2(0f, 1f);
            panelRt.anchorMax = new Vector2(0f, 1f);
            panelRt.pivot = new Vector2(0f, 1f);
            panelRt.anchoredPosition = new Vector2(16f, -16f);
            panelRt.sizeDelta = new Vector2(PANEL_WIDTH, 0f);

            var rootLayout = GetComponent<VerticalLayoutGroup>();
            if (rootLayout == null) rootLayout = gameObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(12, 12, 12, 12);
            rootLayout.spacing = 8f;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;

            // ContentSizeFitter ONLY on the root panel
            var rootFitter = GetComponent<ContentSizeFitter>();
            if (rootFitter == null) rootFitter = gameObject.AddComponent<ContentSizeFitter>();
            rootFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            rootFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var bgImg = GetComponent<Image>();
            if (bgImg == null) bgImg = gameObject.AddComponent<Image>();
            bgImg.color = new Color(0.18f, 0.18f, 0.20f, 0.96f);

            // 2. Build Header
            BuildHeader();

            // 3. Build Minimized Bar
            BuildMinimizedBar();

            // 4. Build Main Content
            BuildMainContent();
        }

        private void BuildHeader()
        {
            var header = CreateRow("HeaderRow", transform, 28f);
            var hGroup = header.AddComponent<HorizontalLayoutGroup>();
            hGroup.spacing = 8f;
            hGroup.childControlWidth = false;
            hGroup.childControlHeight = true;
            hGroup.childForceExpandWidth = false;
            hGroup.childForceExpandHeight = true;
            hGroup.childAlignment = TextAnchor.MiddleLeft;

            // Title
            var title = CreateText(header.transform, "Title", "Панель керування", 13, FontStyles.Bold, Color.white, TextAlignmentOptions.MidlineLeft);
            var titleLe = title.gameObject.AddComponent<LayoutElement>();
            titleLe.preferredWidth = 200f;

            // Count Badge
            var badge = new GameObject("CountBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            badge.transform.SetParent(header.transform, false);
            badge.GetComponent<Image>().color = new Color(0.28f, 0.28f, 0.32f, 1f);
            var badgeLe = badge.AddComponent<LayoutElement>();
            badgeLe.preferredWidth = 55f;
            badgeLe.preferredHeight = 24f;

            headerCountText = CreateText(badge.transform, "CountText", "5 шт.", 11, FontStyles.Normal, Color.white, TextAlignmentOptions.Center);
            var bRt = headerCountText.GetComponent<RectTransform>();
            bRt.anchorMin = Vector2.zero;
            bRt.anchorMax = Vector2.one;
            bRt.sizeDelta = Vector2.zero;

            // Minimize Button
            minButton = CreateButton(header.transform, "MinButton", "_", new Color(0.28f, 0.28f, 0.32f, 1f), Color.white, 12f);
            var minLe = minButton.gameObject.AddComponent<LayoutElement>();
            minLe.preferredWidth = 28f;
            minLe.preferredHeight = 24f;
            minButtonText = minButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void BuildMinimizedBar()
        {
            minimizedRoot = CreateRow("MinimizedRoot", transform, 30f);
            var hGroup = minimizedRoot.AddComponent<HorizontalLayoutGroup>();
            hGroup.spacing = 6f;
            hGroup.childControlWidth = false;
            hGroup.childControlHeight = true;
            hGroup.childForceExpandWidth = false;
            hGroup.childForceExpandHeight = true;
            hGroup.childAlignment = TextAnchor.MiddleCenter;

            miniRemoveCardBtn = CreateButton(minimizedRoot.transform, "MiniRemove", "−", new Color(0.28f, 0.28f, 0.32f, 1f), Color.white, 14f);
            miniRemoveCardBtn.gameObject.AddComponent<LayoutElement>().preferredWidth = 30f;

            miniCountText = CreateText(minimizedRoot.transform, "MiniCount", "5", 13, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
            miniCountText.gameObject.AddComponent<LayoutElement>().preferredWidth = 40f;

            miniAddCardBtn = CreateButton(minimizedRoot.transform, "MiniAdd", "+", new Color(0.28f, 0.28f, 0.32f, 1f), Color.white, 14f);
            miniAddCardBtn.gameObject.AddComponent<LayoutElement>().preferredWidth = 30f;

            miniResetHandBtn = CreateButton(minimizedRoot.transform, "MiniReset", "Скинути", new Color(0.28f, 0.28f, 0.32f, 1f), Color.white, 11f);
            miniResetHandBtn.gameObject.AddComponent<LayoutElement>().preferredWidth = 190f;

            minimizedRoot.SetActive(false);
        }

        private void BuildMainContent()
        {
            contentRoot = CreateRow("ContentRoot", transform, -1f);
            var vGroup = contentRoot.AddComponent<VerticalLayoutGroup>();
            vGroup.spacing = 8f;
            vGroup.childControlWidth = true;
            vGroup.childControlHeight = true;
            vGroup.childForceExpandWidth = true;
            vGroup.childForceExpandHeight = false;

            // 1. Actions Bar [ - ] [ 5 ] [ + ] [ Скинути руку ]
            var actionsRow = CreateRow("ActionsRow", contentRoot.transform, 30f);
            var aGroup = actionsRow.AddComponent<HorizontalLayoutGroup>();
            aGroup.spacing = 6f;
            aGroup.childControlWidth = false;
            aGroup.childControlHeight = true;
            aGroup.childForceExpandWidth = false;
            aGroup.childForceExpandHeight = true;
            aGroup.childAlignment = TextAnchor.MiddleLeft;

            removeCardBtn = CreateButton(actionsRow.transform, "RemoveBtn", "−", new Color(0.28f, 0.28f, 0.32f, 1f), Color.white, 14f);
            removeCardBtn.gameObject.AddComponent<LayoutElement>().preferredWidth = 32f;

            countText = CreateText(actionsRow.transform, "CountText", "5", 13, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
            countText.gameObject.AddComponent<LayoutElement>().preferredWidth = 38f;

            addCardBtn = CreateButton(actionsRow.transform, "AddBtn", "+", new Color(0.28f, 0.28f, 0.32f, 1f), Color.white, 14f);
            addCardBtn.gameObject.AddComponent<LayoutElement>().preferredWidth = 32f;

            resetHandBtn = CreateButton(actionsRow.transform, "ResetHandBtn", "Скинути руку", new Color(0.28f, 0.28f, 0.32f, 1f), Color.white, 11f);
            resetHandBtn.gameObject.AddComponent<LayoutElement>().preferredWidth = 190f;

            CreateDivider(contentRoot.transform);

            // 2. Sliders (Each has fixed height = 36px, zero nested fitters)
            CreateSliderEntry(contentRoot.transform, "Ширина карти", 60f, 200f, 112f, out cardWidthSlider, out cardWidthInput);
            CreateSliderEntry(contentRoot.transform, "Ширина руки", 300f, 1400f, 680f, out handWidthSlider, out handWidthInput);
            CreateSliderEntry(contentRoot.transform, "Відстань між картами", 24f, 220f, 56f, out cardDistSlider, out cardDistInput);
            CreateSliderEntry(contentRoot.transform, "Мінімальна відстань", 4f, 100f, 24f, out minCardDistSlider, out minCardDistInput);
            CreateSliderEntry(contentRoot.transform, "Відстань ховеру", 112f, 280f, 112f, out hoverDistSlider, out hoverDistInput);
            CreateSliderEntry(contentRoot.transform, "Підйом при ховері", 0f, 80f, 28f, out hoverLiftSlider, out hoverLiftInput);

            CreateDivider(contentRoot.transform);

            // 3. Layout Details Toggle Row
            var toggleRow = CreateRow("ToggleRow", contentRoot.transform, 28f);
            var tGroup = toggleRow.AddComponent<HorizontalLayoutGroup>();
            tGroup.spacing = 8f;
            tGroup.childControlWidth = false;
            tGroup.childControlHeight = true;
            tGroup.childForceExpandWidth = false;
            tGroup.childForceExpandHeight = true;
            tGroup.childAlignment = TextAnchor.MiddleLeft;

            var tLbl = CreateText(toggleRow.transform, "ToggleLbl", "Деталі розмітки", 12, FontStyles.Normal, Color.white, TextAlignmentOptions.MidlineLeft);
            tLbl.gameObject.AddComponent<LayoutElement>().preferredWidth = 190f;

            layoutDetailsBtn = CreateButton(toggleRow.transform, "DetailsBtn", "Вимкнено", new Color(0.28f, 0.28f, 0.32f, 1f), Color.white, 11f);
            layoutDetailsBtn.gameObject.AddComponent<LayoutElement>().preferredWidth = 110f;
            layoutDetailsBtnText = layoutDetailsBtn.GetComponentInChildren<TextMeshProUGUI>();

            // 4. Telemetry Bar
            var telBar = CreateRow("TelemetryBar", contentRoot.transform, 22f);
            var telBg = telBar.AddComponent<Image>();
            telBg.color = new Color(0.13f, 0.13f, 0.15f, 1f);
            var telH = telBar.AddComponent<HorizontalLayoutGroup>();
            telH.padding = new RectOffset(6, 6, 2, 2);
            telH.spacing = 6f;
            telH.childControlWidth = false;
            telH.childControlHeight = true;
            telH.childForceExpandWidth = false;
            telH.childForceExpandHeight = true;
            telH.childAlignment = TextAnchor.MiddleLeft;

            telemetryStateText = CreateText(telBar.transform, "StateText", "Normal", 10, FontStyles.Bold, Color.white, TextAlignmentOptions.MidlineLeft);
            telemetryStateText.gameObject.AddComponent<LayoutElement>().preferredWidth = 90f;

            telemetryStepText = CreateText(telBar.transform, "StepText", "step: 56px", 10, FontStyles.Normal, Color.white * 0.8f, TextAlignmentOptions.Center);
            telemetryStepText.gameObject.AddComponent<LayoutElement>().preferredWidth = 100f;

            telemetryHStepText = CreateText(telBar.transform, "HStepText", "h-step: 112px", 10, FontStyles.Normal, Color.white * 0.8f, TextAlignmentOptions.MidlineRight);
            telemetryHStepText.gameObject.AddComponent<LayoutElement>().preferredWidth = 100f;
        }

        private GameObject CreateRow(string name, Transform parent, float height)
        {
            var row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            if (height > 0f)
            {
                var le = row.AddComponent<LayoutElement>();
                le.preferredHeight = height;
                le.minHeight = height;
            }
            return row;
        }

        private void CreateDivider(Transform parent)
        {
            var div = new GameObject("Divider", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            div.transform.SetParent(parent, false);
            div.GetComponent<Image>().color = new Color(0.28f, 0.28f, 0.32f, 0.6f);
            var le = div.AddComponent<LayoutElement>();
            le.preferredHeight = 1f;
            le.minHeight = 1f;
        }

        private void CreateSliderEntry(Transform parent, string label, float min, float max, float defaultVal, out Slider slider, out TMP_InputField inputField)
        {
            // Single container per slider entry with fixed height = 36px
            var entry = CreateRow("SliderEntry_" + label, parent, 36f);

            // Label (anchored top-left)
            var lblObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblObj.transform.SetParent(entry.transform, false);
            var lblRt = lblObj.GetComponent<RectTransform>();
            lblRt.anchorMin = new Vector2(0f, 0.5f);
            lblRt.anchorMax = new Vector2(0f, 1f);
            lblRt.pivot = new Vector2(0f, 0.5f);
            lblRt.anchoredPosition = new Vector2(0f, 0f);
            lblRt.sizeDelta = new Vector2(200f, 18f);

            var lbl = lblObj.GetComponent<TextMeshProUGUI>();
            lbl.text = label;
            lbl.fontSize = 11;
            lbl.color = Color.white;
            lbl.alignment = TextAlignmentOptions.MidlineLeft;
            lbl.enableWordWrapping = false;

            // Unit "px" (anchored top-right)
            var pxObj = new GameObject("Unit", typeof(RectTransform), typeof(TextMeshProUGUI));
            pxObj.transform.SetParent(entry.transform, false);
            var pxRt = pxObj.GetComponent<RectTransform>();
            pxRt.anchorMin = new Vector2(1f, 0.5f);
            pxRt.anchorMax = new Vector2(1f, 1f);
            pxRt.pivot = new Vector2(1f, 0.5f);
            pxRt.anchoredPosition = new Vector2(0f, 0f);
            pxRt.sizeDelta = new Vector2(18f, 18f);

            var px = pxObj.GetComponent<TextMeshProUGUI>();
            px.text = "px";
            px.fontSize = 10;
            px.color = Color.white * 0.7f;
            px.alignment = TextAlignmentOptions.MidlineRight;

            // Input Field (anchored to the left of "px")
            var inObj = new GameObject("Input", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            inObj.transform.SetParent(entry.transform, false);
            var inRt = inObj.GetComponent<RectTransform>();
            inRt.anchorMin = new Vector2(1f, 0.5f);
            inRt.anchorMax = new Vector2(1f, 1f);
            inRt.pivot = new Vector2(1f, 0.5f);
            inRt.anchoredPosition = new Vector2(-22f, 0f);
            inRt.sizeDelta = new Vector2(50f, 18f);
            inObj.GetComponent<Image>().color = new Color(0.12f, 0.12f, 0.14f, 1f);

            var inTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            inTxtObj.transform.SetParent(inObj.transform, false);
            var inTxtRt = inTxtObj.GetComponent<RectTransform>();
            inTxtRt.anchorMin = Vector2.zero;
            inTxtRt.anchorMax = Vector2.one;
            inTxtRt.sizeDelta = Vector2.zero;

            var inTmp = inTxtObj.GetComponent<TextMeshProUGUI>();
            inTmp.fontSize = 11;
            inTmp.alignment = TextAlignmentOptions.MidlineRight;
            inTmp.color = Color.white;

            inputField = inObj.AddComponent<TMP_InputField>();
            inputField.textComponent = inTmp;
            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            inputField.text = Mathf.RoundToInt(defaultVal).ToString();

            // Slider Bar (anchored bottom, full width)
            var sliderObj = new GameObject("Slider", typeof(RectTransform));
            sliderObj.transform.SetParent(entry.transform, false);
            var sRt = sliderObj.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0f, 0f);
            sRt.anchorMax = new Vector2(1f, 0.45f);
            sRt.pivot = new Vector2(0.5f, 0.5f);
            sRt.anchoredPosition = Vector2.zero;
            sRt.sizeDelta = Vector2.zero;

            // Background Track
            var bgTrack = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgTrack.transform.SetParent(sliderObj.transform, false);
            var bgRt = bgTrack.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0.35f);
            bgRt.anchorMax = new Vector2(1f, 0.65f);
            bgRt.sizeDelta = Vector2.zero;
            bgTrack.GetComponent<Image>().color = new Color(0.12f, 0.12f, 0.14f, 1f);

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
            fill.GetComponent<Image>().color = new Color(0.35f, 0.60f, 0.90f, 1f);

            // Handle Area & Handle
            var handleArea = new GameObject("HandleArea", typeof(RectTransform));
            handleArea.transform.SetParent(sliderObj.transform, false);
            var haRt = handleArea.GetComponent<RectTransform>();
            haRt.anchorMin = Vector2.zero;
            haRt.anchorMax = Vector2.one;
            haRt.sizeDelta = Vector2.zero;

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            var hRt = handle.GetComponent<RectTransform>();
            hRt.sizeDelta = new Vector2(12f, 14f);
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
            tmp.enableWordWrapping = false;
            return tmp;
        }

        private Button CreateButton(Transform parent, string name, string label, Color bg, Color textCol, float fontSize)
        {
            var btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            btnObj.GetComponent<Image>().color = bg;

            var txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(btnObj.transform, false);
            var txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            var tmp = txtObj.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Normal;
            tmp.color = textCol;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;

            return btnObj.GetComponent<Button>();
        }

        private void WireEvents()
        {
            if (minButton != null) minButton.onClick.AddListener(ToggleMinimize);
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

            if (minButtonText != null)
            {
                minButtonText.text = isMinimized ? "□" : "_";
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
            layoutDetailsBtnText.text = on ? "Увімкнено" : "Вимкнено";
            if (layoutDetailsBtn != null)
            {
                var img = layoutDetailsBtn.GetComponent<Image>();
                if (img != null)
                {
                    img.color = on ? new Color(0.24f, 0.45f, 0.65f, 1f) : new Color(0.28f, 0.28f, 0.32f, 1f);
                }
            }
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
                telemetryStateText.text = metrics.IsOverflowing ? "Overflow" : "Normal";
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
