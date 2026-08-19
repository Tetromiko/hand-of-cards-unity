using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Tetromiko.CardsHandLayout
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class CardHandController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private HandSettings settings = HandSettings.CreateDefault();
        [SerializeField] private CardView cardPrefab;
        [SerializeField] private RectTransform cardsContainer;
        [SerializeField] private RectTransform slotsContainer;

        [Header("Initial Setup")]
        [SerializeField] private bool populateDefaultCardsOnStart = true;
        [SerializeField] private int initialCardCount = 5;

        [Header("Events")]
        public UnityEvent<CardView> onCardClicked = new UnityEvent<CardView>();
        public UnityEvent<CardView> onCardHovered = new UnityEvent<CardView>();
        public UnityEvent<CardView> onCardUnhovered = new UnityEvent<CardView>();
        public UnityEvent<int, int> onCardsReordered = new UnityEvent<int, int>();
        public UnityEvent onHandUpdated = new UnityEvent();

        // Runtime Cards & Slots State
        private readonly List<CardData> cardsData = new List<CardData>();
        private readonly List<CardView> cardViews = new List<CardView>();
        private readonly List<CardSlotView> slotViews = new List<CardSlotView>();

        // Interaction State
        private int? hoveredIndex = null;
        private bool isDragging = false;
        private CardView draggedCard = null;
        private int? draggedIndex = null;
        private int? dragTargetIndex = null;
        private float initialDragX = 0f;
        private Vector2 dragOffset = Vector2.zero;
        private Vector2 dragStartPointerLocalPos = Vector2.zero;

        private RectTransform rectTransform;
        public RectTransform RectTransform => rectTransform != null ? rectTransform : (rectTransform = GetComponent<RectTransform>());
        public RectTransform Container => cardsContainer != null ? cardsContainer : RectTransform;
        public RectTransform SlotsContainer => slotsContainer != null ? slotsContainer : RectTransform;
        public HandSettings Settings => settings;
        public IReadOnlyList<CardView> CardViews => cardViews;
        public IReadOnlyList<CardSlotView> SlotViews => slotViews;
        public IReadOnlyList<CardData> CardsData => cardsData;
        public int CardsCount => cardViews.Count;
        public int? HoveredIndex => hoveredIndex;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (cardsContainer == null) cardsContainer = rectTransform;
            EnsureSlotsContainer();
        }

        private void Start()
        {
            EventSystemAdapter.EnsureAdaptiveEventSystem();
            if (Application.isPlaying && populateDefaultCardsOnStart && cardViews.Count == 0)
            {
                CreateDefaultCards(initialCardCount);
            }
        }

        private void EnsureSlotsContainer()
        {
            if (slotsContainer == null)
            {
                var existing = transform.Find("SlotsLayer");
                if (existing != null)
                {
                    slotsContainer = existing.GetComponent<RectTransform>();
                }
                else
                {
                    var slotsObj = new GameObject("SlotsLayer", typeof(RectTransform));
                    slotsObj.transform.SetParent(transform, false);
                    slotsObj.transform.SetAsFirstSibling(); // Render behind cards for clean raycasting
                    slotsContainer = slotsObj.GetComponent<RectTransform>();
                    slotsContainer.anchorMin = Vector2.zero;
                    slotsContainer.anchorMax = Vector2.one;
                    slotsContainer.sizeDelta = Vector2.zero;
                }
            }
        }

        private void Update()
        {
            EnsureSlotsContainer();

            if (cardViews.Count == 0)
            {
                SyncSlotViewsCount(0);
                return;
            }

            // Enforce minimum hand width constraint
            float minHandWidth = CardHandLayoutEngine.CalculateMinHandWidth(
                cardViews.Count,
                settings.minCardDistance,
                settings.hoverDistance,
                settings.cardWidth
            );
            if (settings.handWidth < minHandWidth)
            {
                settings.handWidth = minHandWidth;
            }

            int? activeIndex = isDragging ? dragTargetIndex : hoveredIndex;
            var metrics = CardHandLayoutEngine.ComputeHandMetrics(cardViews.Count, settings, activeIndex);
            var zones = CardHandLayoutEngine.GetSlotHoverZones(metrics, settings);

            // 1. Synchronize and update interaction slots
            SyncSlotViewsCount(cardViews.Count);
            for (int s = 0; s < slotViews.Count && s < zones.Count; s++)
            {
                if (slotViews[s] != null)
                {
                    slotViews[s].UpdateZone(zones[s], settings.cardHeight);
                }
            }

            // 2. Update card positions, scales and animations
            for (int i = 0; i < cardViews.Count; i++)
            {
                var cardView = cardViews[i];
                if (cardView == null) continue;

                var transformData = CardHandLayoutEngine.ComputeCardTransform(
                    i,
                    cardView.CardData?.id ?? i.ToString(),
                    metrics,
                    settings,
                    hoveredIndex,
                    isDragging,
                    draggedCard?.CardData?.id,
                    draggedIndex,
                    dragTargetIndex,
                    initialDragX,
                    dragOffset
                );

                cardView.SetTargetTransform(
                    transformData.Position,
                    transformData.Scale,
                    settings.smoothTime,
                    settings.maxSpeed
                );

                cardView.SetVisualState(transformData.State);

                // Hierarchy depth ordering
                if (transformData.State == CardInteractionState.Dragged)
                {
                    cardView.transform.SetAsLastSibling();
                }
                else if (transformData.State == CardInteractionState.Hovered)
                {
                    cardView.transform.SetSiblingIndex(Mathf.Clamp(cardViews.Count - 1, 0, cardViews.Count));
                }
                else
                {
                    cardView.transform.SetSiblingIndex(i);
                }
            }
        }

        private void SyncSlotViewsCount(int targetCount)
        {
            // Remove excess slots
            while (slotViews.Count > targetCount)
            {
                int lastIdx = slotViews.Count - 1;
                var slot = slotViews[lastIdx];
                slotViews.RemoveAt(lastIdx);
                if (slot != null)
                {
                    if (Application.isPlaying) Destroy(slot.gameObject);
                    else DestroyImmediate(slot.gameObject);
                }
            }

            // Add missing slots
            while (slotViews.Count < targetCount)
            {
                int newIdx = slotViews.Count;
                var slotObj = new GameObject($"Slot_{newIdx}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CardSlotView));
                slotObj.transform.SetParent(SlotsContainer, false);
                var slotView = slotObj.GetComponent<CardSlotView>();
                slotView.Initialize(newIdx, this);
                slotViews.Add(slotView);
            }
        }

        public void AddCard(CardData data = null)
        {
            if (data == null)
            {
                data = CreateRandomCardData();
            }

            cardsData.Add(data);

            if (Container != null)
            {
                CardView newCardView = null;
                if (cardPrefab != null)
                {
                    newCardView = Instantiate(cardPrefab, Container);
                }
                else
                {
                    newCardView = CreateProceduralCardView(Container);
                }

                if (newCardView != null)
                {
                    newCardView.RectTransform.sizeDelta = new Vector2(settings.cardWidth, settings.cardHeight);
                    newCardView.Initialize(data, this, cardViews.Count);
                    cardViews.Add(newCardView);
                }
            }

            onHandUpdated?.Invoke();
        }

        private CardView CreateProceduralCardView(Transform parent)
        {
            var cardObj = new GameObject("ProceduralCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CardView));
            cardObj.transform.SetParent(parent, false);

            var rt = cardObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(settings.cardWidth, settings.cardHeight);

            var img = cardObj.GetComponent<Image>();
            img.color = new Color(0.98f, 0.98f, 1f, 1f);
            img.raycastTarget = true;

            var view = cardObj.GetComponent<CardView>();
            view.EnsureDefaultCardVisuals();
            return view;
        }

        public void RemoveCard(int index)
        {
            if (index < 0 || index >= cardViews.Count) return;

            var view = cardViews[index];
            cardViews.RemoveAt(index);

            if (index < cardsData.Count)
            {
                cardsData.RemoveAt(index);
            }

            if (view != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(view.gameObject);
                }
                else
                {
                    DestroyImmediate(view.gameObject);
                }
            }

            // Refresh indices
            for (int i = 0; i < cardViews.Count; i++)
            {
                if (cardViews[i] != null) cardViews[i].SetIndex(i);
            }

            if (hoveredIndex.HasValue && hoveredIndex.Value >= cardViews.Count)
            {
                hoveredIndex = null;
            }

            onHandUpdated?.Invoke();
        }

        public void RemoveLastCard()
        {
            if (cardViews.Count > 0)
            {
                RemoveCard(cardViews.Count - 1);
            }
        }

        public void ClearHand()
        {
            for (int i = cardViews.Count - 1; i >= 0; i--)
            {
                if (cardViews[i] != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(cardViews[i].gameObject);
                    }
                    else
                    {
                        DestroyImmediate(cardViews[i].gameObject);
                    }
                }
            }

            cardViews.Clear();
            cardsData.Clear();
            SyncSlotViewsCount(0);
            hoveredIndex = null;
            isDragging = false;
            draggedCard = null;
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

        private CardData CreateRandomCardData()
        {
            CardSuit[] suits = { CardSuit.Spades, CardSuit.Hearts, CardSuit.Diamonds, CardSuit.Clubs };
            string[] ranks = { "A", "K", "Q", "J", "10", "9", "8", "7", "6", "5", "4", "3", "2" };

            var suit = suits[UnityEngine.Random.Range(0, suits.Length)];
            var rank = ranks[UnityEngine.Random.Range(0, ranks.Length)];
            return new CardData(null, rank, suit);
        }

        // --- Slot-Driven Interaction Callbacks ---

        internal void OnSlotPointerEnter(int slotIndex)
        {
            if (isDragging) return;
            if (slotIndex >= 0 && slotIndex < cardViews.Count)
            {
                hoveredIndex = slotIndex;
                onCardHovered?.Invoke(cardViews[slotIndex]);
            }
        }

        internal void OnSlotPointerMove(int slotIndex, PointerEventData eventData)
        {
            if (isDragging) return;
            if (slotIndex >= 0 && slotIndex < cardViews.Count && hoveredIndex != slotIndex)
            {
                hoveredIndex = slotIndex;
                onCardHovered?.Invoke(cardViews[slotIndex]);
            }
        }

        internal void OnSlotPointerExit(int slotIndex)
        {
            if (isDragging) return;
            if (hoveredIndex.HasValue && hoveredIndex.Value == slotIndex)
            {
                var card = (slotIndex >= 0 && slotIndex < cardViews.Count) ? cardViews[slotIndex] : null;
                hoveredIndex = null;
                if (card != null) onCardUnhovered?.Invoke(card);
            }
        }

        internal void OnSlotClicked(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < cardViews.Count)
            {
                onCardClicked?.Invoke(cardViews[slotIndex]);
            }
        }

        internal void OnSlotBeginDrag(int slotIndex, PointerEventData eventData)
        {
            if (slotIndex < 0 || slotIndex >= cardViews.Count) return;

            isDragging = true;
            draggedCard = cardViews[slotIndex];
            draggedIndex = slotIndex;
            dragTargetIndex = slotIndex;

            var metrics = CardHandLayoutEngine.ComputeHandMetrics(cardViews.Count, settings, slotIndex);
            initialDragX = CardHandLayoutEngine.GetSlotXPos(slotIndex, metrics, settings);
            dragOffset = Vector2.zero;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                Container,
                eventData.position,
                eventData.pressEventCamera,
                out dragStartPointerLocalPos
            );

            hoveredIndex = null;
        }

        internal void OnSlotDrag(int slotIndex, PointerEventData eventData)
        {
            if (!isDragging || Container == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                Container,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 currentLocalPos))
            {
                dragOffset = currentLocalPos - dragStartPointerLocalPos;

                float currentPointerX = currentLocalPos.x;
                var metrics = CardHandLayoutEngine.ComputeHandMetrics(cardViews.Count, settings, dragTargetIndex);
                int closestSlot = CardHandLayoutEngine.FindClosestSlotIndex(currentPointerX, metrics, settings);

                if (dragTargetIndex != closestSlot)
                {
                    dragTargetIndex = closestSlot;
                }
            }
        }

        internal void OnSlotEndDrag(int slotIndex, PointerEventData eventData)
        {
            if (!isDragging) return;

            int sourceIdx = draggedIndex ?? -1;
            int targetIdx = dragTargetIndex ?? -1;

            if (sourceIdx >= 0 && targetIdx >= 0 && sourceIdx != targetIdx && sourceIdx < cardViews.Count && targetIdx < cardViews.Count)
            {
                // Reorder card views
                var movedView = cardViews[sourceIdx];
                cardViews.RemoveAt(sourceIdx);
                cardViews.Insert(targetIdx, movedView);

                // Reorder data
                if (sourceIdx < cardsData.Count && targetIdx < cardsData.Count)
                {
                    var movedData = cardsData[sourceIdx];
                    cardsData.RemoveAt(sourceIdx);
                    cardsData.Insert(targetIdx, movedData);
                }

                // Update indices
                for (int i = 0; i < cardViews.Count; i++)
                {
                    if (cardViews[i] != null) cardViews[i].SetIndex(i);
                }

                onCardsReordered?.Invoke(sourceIdx, targetIdx);
            }

            hoveredIndex = targetIdx >= 0 ? targetIdx : (int?)null;
            isDragging = false;
            draggedCard = null;
            draggedIndex = null;
            dragTargetIndex = null;
            dragOffset = Vector2.zero;

            onHandUpdated?.Invoke();
        }

        // Direct Card Event Forwarding (in case user clicks/drags directly on card)
        internal void OnCardPointerEnter(CardView card)
        {
            int idx = cardViews.IndexOf(card);
            if (idx >= 0) OnSlotPointerEnter(idx);
        }

        internal void OnCardPointerExit(CardView card)
        {
            int idx = cardViews.IndexOf(card);
            if (idx >= 0) OnSlotPointerExit(idx);
        }

        internal void OnCardClicked(CardView card)
        {
            int idx = cardViews.IndexOf(card);
            if (idx >= 0) OnSlotClicked(idx);
        }

        internal void OnCardBeginDrag(CardView card, PointerEventData eventData)
        {
            int idx = cardViews.IndexOf(card);
            if (idx >= 0) OnSlotBeginDrag(idx, eventData);
        }

        internal void OnCardDrag(CardView card, PointerEventData eventData)
        {
            int idx = cardViews.IndexOf(card);
            if (idx >= 0) OnSlotDrag(idx, eventData);
        }

        internal void OnCardEndDrag(CardView card, PointerEventData eventData)
        {
            int idx = cardViews.IndexOf(card);
            if (idx >= 0) OnSlotEndDrag(idx, eventData);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!settings.drawGizmos || Container == null) return;

            Gizmos.matrix = Container.localToWorldMatrix;

            // Draw hand bounds
            Gizmos.color = new Color(0.2f, 0.8f, 0.8f, 0.4f);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(settings.handWidth, settings.cardHeight * 1.5f, 0f));

            if (cardViews.Count > 0)
            {
                var metrics = CardHandLayoutEngine.ComputeHandMetrics(cardViews.Count, settings, hoveredIndex);
                var zones = CardHandLayoutEngine.GetSlotHoverZones(metrics, settings);

                for (int i = 0; i < zones.Count; i++)
                {
                    var zone = zones[i];
                    bool isHov = hoveredIndex.HasValue && hoveredIndex.Value == i;

                    Gizmos.color = isHov ? new Color(1f, 0.2f, 0.6f, 0.8f) : new Color(0.4f, 0.6f, 1f, 0.3f);
                    Vector3 center = new Vector3(zone.CenterX, 0f, 0f);
                    Vector3 size = new Vector3(zone.Width, settings.cardHeight, 0f);
                    Gizmos.DrawWireCube(center, size);
                }
            }
        }
#endif
    }
}
