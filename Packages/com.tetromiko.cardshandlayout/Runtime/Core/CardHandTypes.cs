using System;
using UnityEngine;

namespace Tetromiko.CardsHandLayout
{
    public enum CardInteractionState
    {
        Idle,
        Hovered,
        Dragged
    }

    [Serializable]
    public struct CardTransformData
    {
        public Vector2 Position;
        public float Scale;
        public int ZIndex;
        public CardInteractionState State;

        public CardTransformData(Vector2 position, float scale, int zIndex, CardInteractionState state)
        {
            Position = position;
            Scale = scale;
            ZIndex = zIndex;
            State = state;
        }
    }

    [Serializable]
    public struct SlotHoverZone
    {
        public int Slot;
        public float CenterX;
        public float MinX;
        public float MaxX;
        public float Width;
        public float CardLeft;
        public float CardCenterX;

        public SlotHoverZone(int slot, float minX, float maxX, float cardLeft, float cardCenterX)
        {
            Slot = slot;
            MinX = minX;
            MaxX = maxX;
            Width = maxX - minX;
            CenterX = (minX + maxX) * 0.5f;
            CardLeft = cardLeft;
            CardCenterX = cardCenterX;
        }
    }

    [Serializable]
    public struct HandLayoutMetrics
    {
        public int Count;
        public float AvailableWidth;
        public float EffectiveDistance;
        public float CompressedHoverDistance;
        public float HoverDistance;
        public bool IsOverflowing;
        public bool IsHoverOverflowing;
        public bool HasActiveCard;
        public int? ActiveIndex;
    }
}
