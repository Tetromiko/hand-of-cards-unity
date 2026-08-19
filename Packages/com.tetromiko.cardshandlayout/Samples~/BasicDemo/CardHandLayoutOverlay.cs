using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Tetromiko.CardsHandLayout.Samples
{
    [ExecuteAlways]
    public class CardHandLayoutOverlay : MonoBehaviour
    {
        [SerializeField] private CardHandController handController;
        [SerializeField] private RectTransform bracketsContainer;

        private readonly List<GameObject> slotBrackets = new List<GameObject>();
        private GameObject spanBracketObj;
        private TextMeshProUGUI spanBracketText;
        private GameObject handWidthBracketObj;
        private TextMeshProUGUI handWidthBracketText;
        private GameObject minHandWidthBracketObj;
        private TextMeshProUGUI minHandWidthBracketText;

        public void Initialize(CardHandController controller)
        {
            this.handController = controller;
            EnsureStructure();
        }

        private void Awake()
        {
            if (handController == null) handController = GetComponentInParent<CardHandController>();
            if (bracketsContainer == null) bracketsContainer = GetComponent<RectTransform>();
            EnsureStructure();
        }

        private void EnsureStructure()
        {
            if (bracketsContainer == null) bracketsContainer = GetComponent<RectTransform>();

            if (spanBracketObj == null)
            {
                spanBracketObj = CreateBracketElement("SpanBracket", new Color(0.5f, 0.55f, 0.95f, 0.9f), out spanBracketText);
            }
            if (handWidthBracketObj == null)
            {
                handWidthBracketObj = CreateBracketElement("HandWidthBracket", new Color(0.2f, 0.85f, 0.75f, 0.85f), out handWidthBracketText);
            }
            if (minHandWidthBracketObj == null)
            {
                minHandWidthBracketObj = CreateBracketElement("MinHandWidthBracket", new Color(0.95f, 0.75f, 0.25f, 0.75f), out minHandWidthBracketText);
            }
        }

        private GameObject CreateBracketElement(string name, Color color, out TextMeshProUGUI labelText)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(transform, false);

            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            // Bracket graphic U-shape (Bottom line + Left line + Right line)
            var uGraphic = new GameObject("UGraphic", typeof(RectTransform));
            uGraphic.transform.SetParent(obj.transform, false);
            var uRt = uGraphic.GetComponent<RectTransform>();
            uRt.anchorMin = new Vector2(0f, 1f);
            uRt.anchorMax = new Vector2(1f, 1f);
            uRt.pivot = new Vector2(0.5f, 1f);
            uRt.sizeDelta = new Vector2(0f, 10f);

            // Bottom bar
            CreateLine(uGraphic.transform, "Bottom", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 2f), color);
            // Left tick
            CreateLine(uGraphic.transform, "Left", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(2f, 0f), color);
            // Right tick
            CreateLine(uGraphic.transform, "Right", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(2f, 0f), color);

            // Label text below bracket
            var textObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(obj.transform, false);
            var textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0f, 0f);
            textRt.anchorMax = new Vector2(1f, 0f);
            textRt.pivot = new Vector2(0.5f, 1f);
            textRt.anchoredPosition = new Vector2(0f, -14f);
            textRt.sizeDelta = new Vector2(0f, 20f);

            labelText = textObj.GetComponent<TextMeshProUGUI>();
            labelText.fontSize = 11;
            labelText.fontStyle = FontStyles.Bold;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = color;

            return obj;
        }

        private void CreateLine(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Color color)
        {
            var line = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            line.transform.SetParent(parent, false);
            var rt = line.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = sizeDelta;
            line.GetComponent<Image>().color = color;
        }

        private void Update()
        {
            if (handController == null) return;

            bool show = handController.Settings.showLayoutDetails && handController.CardsCount > 0;
            gameObject.SetActive(show);
            if (!show) return;

            var settings = handController.Settings;
            int count = handController.CardsCount;
            var metrics = CardHandLayoutEngine.ComputeHandMetrics(count, settings, null);
            var zones = CardHandLayoutEngine.GetSlotHoverZones(metrics, settings);

            float cardHeight = settings.cardHeight;
            float baseY = -cardHeight * 0.5f;

            // Row 1: Slot brackets
            while (slotBrackets.Count < zones.Count)
            {
                int index = slotBrackets.Count;
                var slotObj = new GameObject($"SlotBracket_{index}", typeof(RectTransform));
                slotObj.transform.SetParent(transform, false);
                var rt = slotObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(100f, 8f);

                CreateLine(slotObj.transform, "Bottom", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 2f), new Color(0.2f, 0.85f, 0.95f, 0.85f));
                CreateLine(slotObj.transform, "Left", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(2f, 0f), new Color(0.2f, 0.85f, 0.95f, 0.85f));
                CreateLine(slotObj.transform, "Right", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(2f, 0f), new Color(0.2f, 0.85f, 0.95f, 0.85f));

                slotBrackets.Add(slotObj);
            }

            for (int i = 0; i < slotBrackets.Count; i++)
            {
                if (i < zones.Count)
                {
                    slotBrackets[i].SetActive(true);
                    var zone = zones[i];
                    var rt = slotBrackets[i].GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(zone.CenterX, baseY - 16f);
                    rt.sizeDelta = new Vector2(Mathf.Max(4f, zone.Width), 8f);

                    bool is1x = zone.Width >= settings.cardWidth - 1f;
                    Color col = is1x ? new Color(0.95f, 0.35f, 0.65f, 0.9f) : new Color(0.2f, 0.85f, 0.95f, 0.85f);
                    foreach (var img in slotBrackets[i].GetComponentsInChildren<Image>())
                    {
                        img.color = col;
                    }
                }
                else
                {
                    slotBrackets[i].SetActive(false);
                }
            }

            // Row 2: Actual Span
            if (zones.Count > 0 && spanBracketObj != null)
            {
                spanBracketObj.SetActive(true);
                float minX = zones[0].MinX;
                float maxX = zones[zones.Count - 1].MaxX;
                float totalWidth = maxX - minX;
                float centerX = (minX + maxX) * 0.5f;

                var rt = spanBracketObj.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(centerX, baseY - 38f);
                rt.sizeDelta = new Vector2(totalWidth, 10f);

                if (spanBracketText != null)
                {
                    spanBracketText.text = $"{Mathf.RoundToInt(totalWidth)}px";
                }
            }

            // Row 3: Configured Hand Width
            float minHandWidth = CardHandLayoutEngine.CalculateMinHandWidth(count, settings.minCardDistance, settings.hoverDistance, settings.cardWidth);
            float effectiveHandWidth = Mathf.Max(minHandWidth, settings.handWidth);

            if (handWidthBracketObj != null)
            {
                handWidthBracketObj.SetActive(true);
                var rt = handWidthBracketObj.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0f, baseY - 72f);
                rt.sizeDelta = new Vector2(effectiveHandWidth, 10f);

                if (handWidthBracketText != null)
                {
                    handWidthBracketText.text = $"hand width: {Mathf.RoundToInt(effectiveHandWidth)}px";
                }
            }

            // Row 4: Min Hand Width
            if (minHandWidthBracketObj != null)
            {
                minHandWidthBracketObj.SetActive(true);
                var rt = minHandWidthBracketObj.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0f, baseY - 106f);
                rt.sizeDelta = new Vector2(minHandWidth, 10f);

                if (minHandWidthBracketText != null)
                {
                    minHandWidthBracketText.text = $"min: {Mathf.RoundToInt(minHandWidth)}px";
                }
            }
        }
    }
}
