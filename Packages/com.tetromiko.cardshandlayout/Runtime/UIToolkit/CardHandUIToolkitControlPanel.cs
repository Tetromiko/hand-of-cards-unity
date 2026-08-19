using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tetromiko.CardsHandLayout.UIToolkit
{
    /// <summary>
    /// UI Toolkit implementation of the interactive Control Panel for the Card Hand Layout Engine.
    /// Provides controls for card counts, layout metrics, sliders, toggles, and live telemetry.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class CardHandUIToolkitControlPanel : MonoBehaviour
    {
        [SerializeField] private CardHandUIToolkitController handController;

        private UIDocument uiDocument;
        private VisualElement panelRoot;
        private VisualElement contentContainer;
        private Label countBadge;
        private Label telemetryLabel;
        private bool isCollapsed = false;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            if (handController == null)
            {
                handController = GetComponent<CardHandUIToolkitController>();
                if (handController == null)
                {
#if UNITY_2023_1_OR_NEWER
                    handController = FindFirstObjectByType<CardHandUIToolkitController>();
#else
                    handController = FindObjectOfType<CardHandUIToolkitController>();
#endif
                }
            }
        }

        private void OnEnable()
        {
            BuildPanel();
        }

        private void Start()
        {
            BuildPanel();
            if (handController != null)
            {
                handController.onHandUpdated.AddListener(UpdateTelemetry);
            }
        }

        private void OnDisable()
        {
            if (handController != null)
            {
                handController.onHandUpdated.RemoveListener(UpdateTelemetry);
            }
        }

        public void BuildPanel()
        {
            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null) return;

            var root = uiDocument.rootVisualElement;
            if (root == null) return;

            // Remove existing panel if rebuilt
            panelRoot = root.Q<VisualElement>("UIToolkitControlPanel");
            if (panelRoot != null)
            {
                root.Remove(panelRoot);
            }

            // Outer Panel Container
            panelRoot = new VisualElement
            {
                name = "UIToolkitControlPanel",
                style =
                {
                    position = Position.Absolute,
                    left = 16f,
                    top = 16f,
                    width = 330f,
                    backgroundColor = new Color(0.18f, 0.18f, 0.20f, 0.95f),
                    borderTopLeftRadius = 8f,
                    borderTopRightRadius = 8f,
                    borderBottomLeftRadius = 8f,
                    borderBottomRightRadius = 8f,
                    borderTopWidth = 1f,
                    borderBottomWidth = 1f,
                    borderLeftWidth = 1f,
                    borderRightWidth = 1f,
                    borderTopColor = new Color(0.35f, 0.35f, 0.38f, 1f),
                    borderBottomColor = new Color(0.35f, 0.35f, 0.38f, 1f),
                    borderLeftColor = new Color(0.35f, 0.35f, 0.38f, 1f),
                    borderRightColor = new Color(0.35f, 0.35f, 0.38f, 1f),
                    paddingTop = 10f,
                    paddingBottom = 10f,
                    paddingLeft = 12f,
                    paddingRight = 12f
                }
            };
            root.Add(panelRoot);

            // 1. Header Row
            var headerRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    justifyContent = Justify.SpaceBetween,
                    marginBottom = 8f
                }
            };

            var titleContainer = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center }
            };

            var titleLabel = new Label("Card Hand Engine")
            {
                style =
                {
                    color = Color.white,
                    fontSize = 15f,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginRight = 8f
                }
            };
            titleContainer.Add(titleLabel);

            countBadge = new Label("5 Cards")
            {
                style =
                {
                    backgroundColor = new Color(0.28f, 0.35f, 0.48f, 1f),
                    color = new Color(0.9f, 0.95f, 1f, 1f),
                    fontSize = 11f,
                    paddingTop = 2f,
                    paddingBottom = 2f,
                    paddingLeft = 6f,
                    paddingRight = 6f,
                    borderTopLeftRadius = 4f,
                    borderTopRightRadius = 4f,
                    borderBottomLeftRadius = 4f,
                    borderBottomRightRadius = 4f
                }
            };
            titleContainer.Add(countBadge);
            headerRow.Add(titleContainer);

            var collapseButton = new Button(() => ToggleCollapse())
            {
                text = "-",
                style =
                {
                    width = 24f,
                    height = 24f,
                    backgroundColor = new Color(0.25f, 0.25f, 0.28f, 1f),
                    color = Color.white,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 0,
                    paddingRight = 0
                }
            };
            headerRow.Add(collapseButton);
            panelRoot.Add(headerRow);

            // 2. Content Container
            contentContainer = new VisualElement { name = "ContentContainer" };
            panelRoot.Add(contentContainer);

            // 3. Card Management Row
            var cardActionsRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    justifyContent = Justify.SpaceBetween,
                    marginBottom = 10f
                }
            };

            var removeBtn = new Button(() => { handController?.RemoveLastCard(); UpdateCardCount(); })
            {
                text = "-",
                style =
                {
                    width = 36f,
                    height = 26f,
                    backgroundColor = new Color(0.28f, 0.28f, 0.32f, 1f),
                    color = Color.white,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };

            var addBtn = new Button(() => { handController?.AddCard(); UpdateCardCount(); })
            {
                text = "+",
                style =
                {
                    width = 36f,
                    height = 26f,
                    backgroundColor = new Color(0.28f, 0.28f, 0.32f, 1f),
                    color = Color.white,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };

            var resetBtn = new Button(() => { handController?.CreateDefaultCards(5); UpdateCardCount(); })
            {
                text = "Скинути руку",
                style =
                {
                    height = 26f,
                    paddingLeft = 12f,
                    paddingRight = 12f,
                    backgroundColor = new Color(0.28f, 0.28f, 0.32f, 1f),
                    color = Color.white
                }
            };

            cardActionsRow.Add(removeBtn);
            cardActionsRow.Add(addBtn);
            cardActionsRow.Add(resetBtn);
            contentContainer.Add(cardActionsRow);

            // 4. Sliders Section
            if (handController != null)
            {
                var s = handController.Settings;
                AddSliderRow("Ширина карти", s.cardWidth, 40f, 240f, val => s.cardWidth = val);
                AddSliderRow("Ширина руки", s.handWidth, 200f, 1600f, val => s.handWidth = val);
                AddSliderRow("Відстань карт", s.cardDistance, 10f, 200f, val => s.cardDistance = val);
                AddSliderRow("Мін. відстань", s.minCardDistance, 5f, 100f, val => s.minCardDistance = val);
                AddSliderRow("Hover відстань", s.hoverDistance, 40f, 240f, val => s.hoverDistance = val);
                AddSliderRow("Hover підйом", s.hoverLift, 0f, 100f, val => s.hoverLift = val);
            }

            // 5. Toggles Section
            var toggleRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    justifyContent = Justify.SpaceBetween,
                    marginTop = 6f,
                    marginBottom = 8f
                }
            };

            var toggleLabel = new Label("Деталі лейауту")
            {
                style = { color = new Color(0.85f, 0.85f, 0.88f, 1f), fontSize = 12f }
            };

            var detailsToggle = new Toggle
            {
                value = handController != null && handController.Settings.showLayoutDetails
            };
            detailsToggle.RegisterValueChangedCallback(evt =>
            {
                if (handController != null)
                {
                    handController.Settings.showLayoutDetails = evt.newValue;
                    handController.RefreshCardVisuals();
                }
            });

            toggleRow.Add(toggleLabel);
            toggleRow.Add(detailsToggle);
            contentContainer.Add(toggleRow);

            // 6. Telemetry Footer
            telemetryLabel = new Label("Стан: Готово")
            {
                style =
                {
                    fontSize = 11f,
                    color = new Color(0.65f, 0.7f, 0.78f, 1f),
                    paddingTop = 6f,
                    borderTopWidth = 1f,
                    borderTopColor = new Color(0.28f, 0.28f, 0.32f, 1f)
                }
            };
            contentContainer.Add(telemetryLabel);

            UpdateCardCount();
            UpdateTelemetry();
        }

        private void AddSliderRow(string title, float initialValue, float min, float max, Action<float> onValueChanged)
        {
            var row = new VisualElement
            {
                style = { marginBottom = 6f }
            };

            var topBar = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceBetween,
                    alignItems = Align.Center,
                    marginBottom = 2f
                }
            };

            var titleLbl = new Label(title)
            {
                style = { color = new Color(0.85f, 0.85f, 0.88f, 1f), fontSize = 12f }
            };

            var valueLbl = new Label($"{Mathf.RoundToInt(initialValue)} px")
            {
                style = { color = new Color(0.6f, 0.75f, 1f, 1f), fontSize = 11f }
            };

            topBar.Add(titleLbl);
            topBar.Add(valueLbl);
            row.Add(topBar);

            var slider = new Slider(min, max)
            {
                value = initialValue,
                style = { height = 18f, marginLeft = 0, marginRight = 0 }
            };

            slider.RegisterValueChangedCallback(evt =>
            {
                valueLbl.text = $"{Mathf.RoundToInt(evt.newValue)} px";
                onValueChanged?.Invoke(evt.newValue);
                UpdateTelemetry();
            });

            row.Add(slider);
            contentContainer.Add(row);
        }

        private void ToggleCollapse()
        {
            isCollapsed = !isCollapsed;
            if (contentContainer != null)
            {
                contentContainer.style.display = isCollapsed ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        private void UpdateCardCount()
        {
            if (countBadge != null && handController != null)
            {
                int count = handController.CardsCount;
                countBadge.text = $"{count} {(count == 1 ? "Card" : "Cards")}";
            }
        }

        private void UpdateTelemetry()
        {
            if (telemetryLabel == null || handController == null) return;

            int count = handController.CardsCount;
            var settings = handController.Settings;
            var metrics = CardHandLayoutEngine.ComputeHandMetrics(count, settings, handController.HoveredIndex);

            string state = metrics.IsHoverOverflowing ? "Стиснуто (Hover Overflow)" : (metrics.IsOverflowing ? "Каскад (Overflow)" : "Стандарт");
            telemetryLabel.text = $"Крок: {metrics.EffectiveDistance:F0}px | Стан: {state}";
        }
    }
}
