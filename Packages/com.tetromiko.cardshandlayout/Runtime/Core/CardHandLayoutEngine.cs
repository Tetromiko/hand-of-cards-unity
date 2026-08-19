using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tetromiko.CardsHandLayout
{
    /// <summary>
    /// Pure mathematical layout engine for card hands.
    /// Free of MonoBehaviour dependencies, completely deterministic and unit-testable.
    /// </summary>
    public static class CardHandLayoutEngine
    {
        /// <summary>
        /// Calculates the minimum required hand width ensuring cards never exceed limits.
        /// Formula: Min Hand Width = (n - 2) * minCardDistance + CardWidth + hoverDistance
        /// </summary>
        public static float CalculateMinHandWidth(int count, float minCardDistance, float hoverDistance, float cardWidth)
        {
            float W = cardWidth;
            float H = Mathf.Max(W, hoverDistance);

            if (count <= 0) return W;
            if (count == 1) return W;
            if (count == 2) return W + H;
            return (count - 2) * minCardDistance + W + H;
        }

        private static (float fixedWidth, int remainingSlots) GetHoverOccupiedWidth(
            int count,
            int activeIndex,
            float W,
            float H)
        {
            if (activeIndex == count - 1)
            {
                return ((W + H) * 0.5f, count - 1);
            }
            if (activeIndex == 0)
            {
                return (W + (W + H) * 0.5f, Mathf.Max(1, count - 2));
            }
            return (W + H, Mathf.Max(1, count - 2));
        }

        /// <summary>
        /// Computes global layout metrics for the hand of cards based on settings.
        /// </summary>
        public static HandLayoutMetrics ComputeHandMetrics(
            int count,
            HandSettings settings,
            int? activeIndex)
        {
            float W = settings.cardWidth;
            float H = Mathf.Max(W, settings.hoverDistance);
            float minHandWidth = CalculateMinHandWidth(count, settings.minCardDistance, H, W);
            float availableWidth = Mathf.Max(minHandWidth, settings.handWidth);

            float naturalSpan = count > 1 ? (count - 1) * Mathf.Max(settings.minCardDistance, settings.cardDistance) + W : W;
            bool isOverflowing = count > 1 && naturalSpan > availableWidth;

            float effectiveDistance = isOverflowing
                ? Mathf.Max(settings.minCardDistance, Mathf.Floor((availableWidth - W) / (count - 1)))
                : Mathf.Max(settings.minCardDistance, settings.cardDistance);

            bool hasActiveCard = activeIndex.HasValue && count > 1;

            var (fixedWidth, remainingSlots) = hasActiveCard && activeIndex.HasValue
                ? GetHoverOccupiedWidth(count, activeIndex.Value, W, H)
                : (W, Mathf.Max(1, count - 1));

            float idealHoverSpan = hasActiveCard
                ? remainingSlots * effectiveDistance + fixedWidth
                : naturalSpan;

            bool isHoverOverflowing = hasActiveCard && idealHoverSpan > availableWidth;

            float compressedHoverDistance = effectiveDistance;
            if (hasActiveCard && count > 1)
            {
                if (isHoverOverflowing)
                {
                    if (count == 2)
                    {
                        compressedHoverDistance = settings.minCardDistance;
                    }
                    else
                    {
                        compressedHoverDistance = Mathf.Max(
                            settings.minCardDistance,
                            Mathf.Floor((availableWidth - fixedWidth) / remainingSlots)
                        );
                    }
                }
            }

            return new HandLayoutMetrics
            {
                Count = count,
                AvailableWidth = availableWidth,
                EffectiveDistance = effectiveDistance,
                CompressedHoverDistance = compressedHoverDistance,
                HoverDistance = H,
                IsOverflowing = isOverflowing,
                IsHoverOverflowing = isHoverOverflowing,
                HasActiveCard = hasActiveCard,
                ActiveIndex = activeIndex
            };
        }

        public static float ComputeSlotWidth(
            int slot,
            int count,
            float step,
            float hoverDistance,
            int? activeIndex,
            float cardWidth)
        {
            float W = cardWidth;
            float H = Mathf.Max(W, hoverDistance);

            if (count <= 1) return activeIndex.HasValue && activeIndex.Value == 0 ? H : W;

            bool isHovered = activeIndex.HasValue && slot == activeIndex.Value;
            bool isEdge = slot == 0 || slot == count - 1;

            if (isHovered)
            {
                return isEdge ? (W + H) * 0.5f : H;
            }

            if (step >= W)
            {
                return isEdge ? (W + step) * 0.5f : step;
            }

            return slot == count - 1 ? W : step;
        }

        public static (float cardLeft, float cardCenterX) ComputeCardPositionInSlot(
            int slot,
            int count,
            float minX,
            float maxX,
            float step,
            bool isHovered,
            float cardWidth)
        {
            float W = cardWidth;
            float centerX = (minX + maxX) * 0.5f;

            if (count == 1)
            {
                return (centerX - W * 0.5f, centerX);
            }

            float cardLeft;
            if (slot == 0)
            {
                // First card docks flush to the left boundary
                cardLeft = minX;
            }
            else if (slot == count - 1)
            {
                // Last card docks flush to the right boundary
                cardLeft = maxX - W;
            }
            else if (isHovered || step >= W)
            {
                // Internal cards center inside their slot when hovered or when hand is expanded
                cardLeft = centerX - W * 0.5f;
            }
            else
            {
                // Internal unhovered cards cascade from left edge when hand is compressed
                cardLeft = minX;
            }

            return (cardLeft, cardLeft + W * 0.5f);
        }

        /// <summary>
        /// Computes exact positions and hover zones for all card slots with neighbor compensation.
        /// </summary>
        public static List<SlotHoverZone> GetSlotHoverZones(
            HandLayoutMetrics metrics,
            HandSettings settings)
        {
            int count = metrics.Count;
            if (count == 0) return new List<SlotHoverZone>();

            float W = settings.cardWidth;
            float H = Mathf.Max(W, settings.hoverDistance);
            bool hasActive = metrics.HasActiveCard && metrics.ActiveIndex.HasValue;
            float step = hasActive ? metrics.CompressedHoverDistance : metrics.EffectiveDistance;

            // 1. Calculate slot widths
            float[] slotWidths = new float[count];
            float totalSpan = 0f;
            for (int s = 0; s < count; s++)
            {
                slotWidths[s] = ComputeSlotWidth(s, count, step, H, metrics.ActiveIndex, W);
                totalSpan += slotWidths[s];
            }

            // 2. Center symmetrically around X = 0
            float currentLeft = -totalSpan * 0.5f;
            List<SlotHoverZone> rawZones = new List<SlotHoverZone>(count);

            for (int s = 0; s < count; s++)
            {
                float width = slotWidths[s];
                float minX = currentLeft;
                float maxX = minX + width;
                bool isHovered = hasActive && s == metrics.ActiveIndex.Value;

                var (cardLeft, cardCenterX) = ComputeCardPositionInSlot(
                    s,
                    count,
                    minX,
                    maxX,
                    step,
                    isHovered,
                    W
                );

                rawZones.Add(new SlotHoverZone(s, minX, maxX, cardLeft, cardCenterX));
                currentLeft = maxX;
            }

            // 3. Hover Space Compensation for left neighbor
            if (hasActive && metrics.ActiveIndex.Value > 0)
            {
                int activeIdx = metrics.ActiveIndex.Value;
                int prevIdx = activeIdx - 1;

                var prevZone = rawZones[prevIdx];
                var activeZone = rawZones[activeIdx];

                float prevCardRight = prevZone.CardLeft + W;
                float activeCardLeft = activeZone.CardLeft;
                float uncoveredRightX = Mathf.Min(prevCardRight, activeCardLeft);

                if (uncoveredRightX > prevZone.MaxX && uncoveredRightX < activeZone.MaxX - 10f)
                {
                    float newSplitX = uncoveredRightX;

                    prevZone.MaxX = newSplitX;
                    prevZone.Width = prevZone.MaxX - prevZone.MinX;
                    prevZone.CenterX = (prevZone.MinX + prevZone.MaxX) * 0.5f;

                    activeZone.MinX = newSplitX;
                    activeZone.Width = activeZone.MaxX - activeZone.MinX;
                    activeZone.CenterX = (activeZone.MinX + activeZone.MaxX) * 0.5f;

                    rawZones[prevIdx] = prevZone;
                    rawZones[activeIdx] = activeZone;
                }
            }

            return rawZones;
        }

        public static float GetSlotXPos(int slot, HandLayoutMetrics metrics, HandSettings settings)
        {
            var zones = GetSlotHoverZones(metrics, settings);
            if (slot >= 0 && slot < zones.Count)
            {
                return zones[slot].CardCenterX;
            }
            return 0f;
        }

        public static int GetCardTargetSlot(
            int cardIndex,
            bool isDragging,
            int? draggedIndex,
            int? dragTargetIndex)
        {
            if (!isDragging || !draggedIndex.HasValue || !dragTargetIndex.HasValue)
            {
                return cardIndex;
            }

            int src = draggedIndex.Value;
            int tgt = dragTargetIndex.Value;

            if (src < tgt)
            {
                if (cardIndex > src && cardIndex <= tgt)
                {
                    return cardIndex - 1;
                }
            }
            else if (src > tgt)
            {
                if (cardIndex >= tgt && cardIndex < src)
                {
                    return cardIndex + 1;
                }
            }

            return cardIndex;
        }

        public static int FindClosestSlotIndex(
            float mouseX,
            HandLayoutMetrics metrics,
            HandSettings settings)
        {
            var zones = GetSlotHoverZones(metrics, settings);
            if (zones.Count == 0) return 0;

            for (int i = 0; i < zones.Count; i++)
            {
                if (mouseX >= zones[i].MinX && mouseX <= zones[i].MaxX)
                {
                    return zones[i].Slot;
                }
            }

            if (mouseX < zones[0].MinX) return 0;
            return zones[zones.Count - 1].Slot;
        }

        public static CardTransformData ComputeCardTransform(
            int cardIndex,
            string cardId,
            HandLayoutMetrics metrics,
            HandSettings settings,
            int? hoveredIndex,
            bool isDragging,
            string draggingCardId,
            int? draggedIndex,
            int? dragTargetIndex,
            float initialDragX,
            Vector2 dragOffset)
        {
            bool isThisCardDragged = isDragging && draggingCardId == cardId;
            bool isHovered = !isDragging && hoveredIndex.HasValue && hoveredIndex.Value == cardIndex;
            float hoverLift = settings.hoverLift;

            int slot = GetCardTargetSlot(cardIndex, isDragging, draggedIndex, dragTargetIndex);
            float targetX = GetSlotXPos(slot, metrics, settings);
            float targetY = isHovered ? hoverLift : 0f;

            if (isThisCardDragged)
            {
                targetX = initialDragX + dragOffset.x;
                targetY = hoverLift + dragOffset.y;
            }

            int zIndex = isThisCardDragged ? 1000 : cardIndex + 1;
            float scale = isThisCardDragged ? settings.dragScale : (isHovered ? settings.hoverScale : 1.0f);
            var state = isThisCardDragged ? CardInteractionState.Dragged : (isHovered ? CardInteractionState.Hovered : CardInteractionState.Idle);

            return new CardTransformData(new Vector2(targetX, targetY), scale, zIndex, state);
        }
    }
}
