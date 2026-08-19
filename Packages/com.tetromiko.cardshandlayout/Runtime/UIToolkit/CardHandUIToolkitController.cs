using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Tetromiko.CardsHandLayout.UIToolkit
{
    /// <summary>
    /// UI Toolkit implementation of the Cards Hand Layout Controller.
    /// Manages the CardSlotElements and CardElements within a UIDocument.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(UIDocument))]
    public class CardHandUIToolkitController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private HandSettings settings = HandSettings.CreateDefault();

        [Header("Initial Setup")]
        [SerializeField] private bool populateDefaultCardsOnStart = true;
        [SerializeField] private int initialCardCount = 5;

        [Header("Events")]
        public UnityEvent<CardData> onCardClicked = new UnityEvent<CardData>();
        public UnityEvent<CardData> onCardHovered = new UnityEvent<CardData>();
        public UnityEvent<CardData> onCardUnhovered = new UnityEvent<CardData>();
        public UnityEvent<int, int> onCardsReordered = new UnityEvent<int, int>();
        public UnityEvent onHandUpdated = new UnityEvent();

        // Runtime Cards & Slots
        private readonly List<CardData> cardsData = new List<CardData>();
        private readonly List<CardElement> cardElements = new List<CardElement>();
        private readonly List<CardSlotElement> slotElements = new List<CardSlotElement>();

        // UI Toolkit Containers
        private UIDocument uiDocument;
        private VisualElement handRoot;
        private VisualElement slotsLayer;
        private VisualElement cardsLayer;

        // Interaction State
        private int? hoveredIndex = null;
        private bool isDragging = false;
        private int? draggedIndex = null;
        private int? dragTargetIndex = null;
        private float initialDragX = 0f;
        private Vector2 dragOffset = Vector2.zero;
        private Vector2 dragStartPointerPos = Vector2.zero;
        private int activePointerId = -1;

        public HandSettings Settings => settings;
        public IReadOnlyList<CardData> CardsData => cardsData;
        public IReadOnlyList<CardElement> CardElements => cardElements;
        public int CardsCount => cardElements.Count;
        public int? HoveredIndex => hoveredIndex;
        public VisualElement HandRoot => handRoot;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            InitializeVisualElements();
        }

        private void Start()
        {
            InitializeVisualElements();
            if (Application.isPlaying && populateDefaultCardsOnStart && cardElements.Count == 0)
            {
                CreateDefaultCards(initialCardCount);
            }
        }

        public void InitializeVisualElements()
        {
            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null) return;

            var root = uiDocument.rootVisualElement;
            if (root == null) return;

            // Find or create HandRoot container
            handRoot = root.Q<VisualElement>("HandRoot");
            if (handRoot == null)
            {
                handRoot = new VisualElement
                {
                    name = "HandRoot",
                    style =
                    {
                        position = Position.Absolute,
                        left = 0,
                        right = 0,
                        bottom = 80f,
                        height = settings.cardHeight * 1.5f,
                        justifyContent = Justify.Center,
                        alignItems = Align.Center
                    }
                };
                root.Add(handRoot);
            }

            // Slots Layer
            slotsLayer = handRoot.Q<VisualElement>("SlotsLayer");
            if (slotsLayer == null)
            {
                slotsLayer = new VisualElement
                {
                    name = "SlotsLayer",
                    style =
                    {
                        position = Position.Absolute,
                        left = 0,
                        right = 0,
                        top = 0,
                        bottom = 0
                    }
                };
                handRoot.Add(slotsLayer);

                // Register global pointer up/move for drag handling
                slotsLayer.RegisterCallback<PointerMoveEvent>(OnGlobalPointerMove);
                slotsLayer.RegisterCallback<PointerUpEvent>(OnGlobalPointerUp);
                slotsLayer.RegisterCallback<PointerCaptureOutEvent>(OnGlobalPointerCaptureOut);
            }

            // Cards Layer
            cardsLayer = handRoot.Q<VisualElement>("CardsLayer");
            if (cardsLayer == null)
            {
                cardsLayer = new VisualElement
                {
                    name = "CardsLayer",
                    style =
                    {
                        position = Position.Absolute,
                        left = 0,
                        right = 0,
                        top = 0,
                        bottom = 0
                    }
                };
                handRoot.Add(cardsLayer);
            }
        }

        private void Update()
        {
            if (handRoot == null || slotsLayer == null || cardsLayer == null)
            {
                InitializeVisualElements();
                if (handRoot == null) return;
            }

            if (cardElements.Count == 0)
            {
                SyncSlotElementsCount(0);
                return;
            }

            // Enforce minimum hand width constraint
            float minHandWidth = CardHandLayoutEngine.CalculateMinHandWidth(
                cardElements.Count,
                settings.minCardDistance,
                settings.hoverDistance,
                settings.cardWidth
            );
            if (settings.handWidth < minHandWidth)
            {
                settings.handWidth = minHandWidth;
            }

            int? activeIndex = isDragging ? dragTargetIndex : hoveredIndex;
            var metrics = CardHandLayoutEngine.ComputeHandMetrics(cardElements.Count, settings, activeIndex);
            var zones = CardHandLayoutEngine.GetSlotHoverZones(metrics, settings);

            float containerCenter = handRoot.resolvedStyle.width > 0f ? handRoot.resolvedStyle.width * 0.5f : Screen.width * 0.5f;
            float containerY = handRoot.resolvedStyle.height > 0f ? handRoot.resolvedStyle.height * 0.5f : settings.cardHeight * 0.75f;

            // 1. Synchronize and update slot elements
            SyncSlotElementsCount(cardElements.Count);
            for (int s = 0; s < slotElements.Count && s < zones.Count; s++)
            {
                if (slotElements[s] != null)
                {
                    slotElements[s].UpdateBounds(zones[s], containerCenter, settings.cardHeight);
                }
            }

            // 2. Update card positions, scales and animations
            float deltaTime = Time.unscaledDeltaTime;
            for (int i = 0; i < cardElements.Count; i++)
            {
                var card = cardElements[i];
                if (card == null) continue;

                var transformData = CardHandLayoutEngine.ComputeCardTransform(
                    i,
                    card.Data?.id ?? i.ToString(),
                    metrics,
                    settings,
                    hoveredIndex,
                    isDragging,
                    isDragging && draggedIndex.HasValue && draggedIndex.Value < cardsData.Count ? cardsData[draggedIndex.Value].id : null,
                    draggedIndex,
                    dragTargetIndex,
                    initialDragX,
                    dragOffset
                );

                Vector2 worldCenterPos = new Vector2(
                    containerCenter + transformData.Position.x,
                    containerY - transformData.Position.y
                );

                card.style.width = settings.cardWidth;
                card.style.height = settings.cardHeight;
                card.SetTargetTransform(worldCenterPos, transformData.Scale);
                card.SetVisualState(transformData.State);
                card.UpdateMotion(settings.smoothTime, settings.maxSpeed, deltaTime, isDragging && draggedIndex == i);

                // Visual depth ordering
                if (transformData.State == CardInteractionState.Dragged)
                {
                    card.BringToFront();
                }
                else if (transformData.State == CardInteractionState.Hovered)
                {
                    card.BringToFront();
                }
            }
        }

        private void SyncSlotElementsCount(int targetCount)
        {
            if (slotsLayer == null) return;

            while (slotElements.Count > targetCount)
            {
                int lastIdx = slotElements.Count - 1;
                var slot = slotElements[lastIdx];
                slotElements.RemoveAt(lastIdx);
                slotsLayer.Remove(slot);
            }

            while (slotElements.Count < targetCount)
            {
                int newIdx = slotElements.Count;
                var slot = new CardSlotElement(newIdx, this);
                slotsLayer.Add(slot);
                slotElements.Add(slot);
            }
        }

        public void AddCard(CardData data = null)
        {
            if (data == null) data = CreateRandomCardData();
            cardsData.Add(data);

            if (cardsLayer != null)
            {
                var card = new CardElement(data, cardElements.Count);
                card.style.width = settings.cardWidth;
                card.style.height = settings.cardHeight;
                cardsLayer.Add(card);
                cardElements.Add(card);
                card.UpdateCardVisuals(settings.showLayoutDetails);
            }

            onHandUpdated?.Invoke();
        }

        public void RemoveCard(int index)
        {
            if (index < 0 || index >= cardElements.Count) return;

            var card = cardElements[index];
            cardElements.RemoveAt(index);
            if (cardsLayer != null) cardsLayer.Remove(card);

            if (index < cardsData.Count)
            {
                cardsData.RemoveAt(index);
            }

            for (int i = 0; i < cardElements.Count; i++)
            {
                if (cardElements[i] != null) cardElements[i].SetIndex(i);
            }

            if (hoveredIndex.HasValue && hoveredIndex.Value >= cardElements.Count)
            {
                hoveredIndex = null;
            }

            onHandUpdated?.Invoke();
        }

        public void RemoveLastCard()
        {
            if (cardElements.Count > 0)
            {
                RemoveCard(cardElements.Count - 1);
            }
        }

        public void ClearHand()
        {
            if (cardsLayer != null) cardsLayer.Clear();
            cardElements.Clear();
            cardsData.Clear();
            SyncSlotElementsCount(0);

            hoveredIndex = null;
            isDragging = false;
            draggedIndex = null;
            dragTargetIndex = null;

            onHandUpdated?.Invoke();
        }

        public void CreateDefaultCards(int count)
        {
            ClearHand();
            CardSuit[] suits = { CardSuit.Spades, CardSuit.Hearts, CardSuit.Diamonds, CardSuit.Clubs };
            string[] ranks = { "A", "K", "Q", "J", "10", "9", "8", "7", "6", "5", "4", "3", "2" };

            for (int i = 0; i < count; i++)
            {
                var suit = suits[i % suits.Length];
                var rank = ranks[i % ranks.Length];
                AddCard(new CardData($"card-{i + 1}", rank, suit));
            }
        }

        public void RefreshCardVisuals()
        {
            for (int i = 0; i < cardElements.Count; i++)
            {
                if (cardElements[i] != null)
                {
                    cardElements[i].UpdateCardVisuals(settings.showLayoutDetails);
                }
            }
        }

        private CardData CreateRandomCardData()
        {
            CardSuit[] suits = { CardSuit.Spades, CardSuit.Hearts, CardSuit.Diamonds, CardSuit.Clubs };
            string[] ranks = { "A", "K", "Q", "J", "10", "9", "8", "7", "6", "5", "4", "3", "2" };

            var suit = suits[UnityEngine.Random.Range(0, suits.Length)];
            var rank = ranks[UnityEngine.Random.Range(0, ranks.Length)];
            return new CardData(null, rank, suit);
        }

        // --- Pointer Event Handlers ---

        internal void OnSlotPointerEnter(int slotIndex)
        {
            if (isDragging) return;
            if (slotIndex >= 0 && slotIndex < cardElements.Count)
            {
                hoveredIndex = slotIndex;
                onCardHovered?.Invoke(cardsData[slotIndex]);
            }
        }

        internal void OnSlotPointerMove(int slotIndex, PointerMoveEvent evt)
        {
            if (isDragging) return;
            if (slotIndex >= 0 && slotIndex < cardElements.Count && hoveredIndex != slotIndex)
            {
                hoveredIndex = slotIndex;
                onCardHovered?.Invoke(cardsData[slotIndex]);
            }
        }

        internal void OnSlotPointerLeave(int slotIndex)
        {
            if (isDragging) return;
            if (hoveredIndex.HasValue && hoveredIndex.Value == slotIndex)
            {
                var data = (slotIndex >= 0 && slotIndex < cardsData.Count) ? cardsData[slotIndex] : null;
                hoveredIndex = null;
                if (data != null) onCardUnhovered?.Invoke(data);
            }
        }

        internal void OnSlotPointerDown(int slotIndex, PointerDownEvent evt)
        {
            if (slotIndex < 0 || slotIndex >= cardElements.Count) return;

            isDragging = true;
            draggedIndex = slotIndex;
            dragTargetIndex = slotIndex;
            activePointerId = evt.pointerId;

            var metrics = CardHandLayoutEngine.ComputeHandMetrics(cardElements.Count, settings, slotIndex);
            initialDragX = CardHandLayoutEngine.GetSlotXPos(slotIndex, metrics, settings);
            dragOffset = Vector2.zero;
            dragStartPointerPos = evt.position;

            hoveredIndex = null;

            // Capture pointer on slotsLayer for smooth drag tracking
            slotsLayer?.CapturePointer(evt.pointerId);
            onCardClicked?.Invoke(cardsData[slotIndex]);
        }

        private void OnGlobalPointerMove(PointerMoveEvent evt)
        {
            if (!isDragging || evt.pointerId != activePointerId) return;

            Vector2 delta = (Vector2)evt.position - dragStartPointerPos;
            dragOffset = new Vector2(delta.x, -delta.y); // Invert Y for UI Toolkit top-down coords

            float containerCenter = handRoot.resolvedStyle.width > 0f ? handRoot.resolvedStyle.width * 0.5f : Screen.width * 0.5f;
            float currentPointerX = evt.position.x - containerCenter;

            var metrics = CardHandLayoutEngine.ComputeHandMetrics(cardElements.Count, settings, dragTargetIndex);
            int closestSlot = CardHandLayoutEngine.FindClosestSlotIndex(currentPointerX, metrics, settings);

            if (dragTargetIndex != closestSlot)
            {
                dragTargetIndex = closestSlot;
            }
        }

        private void OnGlobalPointerUp(PointerUpEvent evt)
        {
            if (!isDragging || evt.pointerId != activePointerId) return;
            FinishDrag();
        }

        private void OnGlobalPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (isDragging) FinishDrag();
        }

        private void FinishDrag()
        {
            if (slotsLayer != null && activePointerId != -1 && slotsLayer.HasPointerCapture(activePointerId))
            {
                slotsLayer.ReleasePointer(activePointerId);
            }

            int src = draggedIndex ?? -1;
            int tgt = dragTargetIndex ?? -1;

            if (src >= 0 && tgt >= 0 && src != tgt && src < cardElements.Count && tgt < cardElements.Count)
            {
                // Reorder Card Elements
                var movedElement = cardElements[src];
                cardElements.RemoveAt(src);
                cardElements.Insert(tgt, movedElement);

                // Reorder Data
                var movedData = cardsData[src];
                cardsData.RemoveAt(src);
                cardsData.Insert(tgt, movedData);

                // Update indices
                for (int i = 0; i < cardElements.Count; i++)
                {
                    cardElements[i]?.SetIndex(i);
                }

                onCardsReordered?.Invoke(src, tgt);
            }

            hoveredIndex = tgt >= 0 ? tgt : (int?)null;
            isDragging = false;
            draggedIndex = null;
            dragTargetIndex = null;
            activePointerId = -1;
            dragOffset = Vector2.zero;

            onHandUpdated?.Invoke();
        }
    }
}
