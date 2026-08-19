using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tetromiko.CardsHandLayout.UIToolkit
{
    /// <summary>
    /// UI Toolkit VisualElement representing an individual playing card.
    /// Handles visual presentation, rank/suit formatting, and smooth animation.
    /// Uses PickingMode.Ignore so spatial Slot elements receive pointer events seamlessly.
    /// </summary>
    public class CardElement : VisualElement
    {
        public CardData Data { get; private set; }
        public int Index { get; private set; }

        // Child Visual Elements
        private readonly Label topRankLabel;
        private readonly Label centerSuitLabel;
        private readonly Label bottomRankLabel;
        private readonly Label centerIndexLabel;

        // Animation State
        private Vector2 targetPosition;
        private Vector2 currentPosition;
        private Vector2 velocity;
        private float targetScale = 1f;
        private float currentScale = 1f;
        private float scaleVelocity;
        private CardInteractionState visualState = CardInteractionState.Idle;

        public CardElement(CardData cardData, int index)
        {
            this.Data = cardData;
            this.Index = index;

            name = $"Card_{index}";
            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;

            // Standard Card Styling
            style.backgroundColor = new Color(0.96f, 0.96f, 0.98f, 1f);
            style.borderTopLeftRadius = 8f;
            style.borderTopRightRadius = 8f;
            style.borderBottomLeftRadius = 8f;
            style.borderBottomRightRadius = 8f;
            style.borderTopWidth = 1f;
            style.borderBottomWidth = 1f;
            style.borderLeftWidth = 1f;
            style.borderRightWidth = 1f;
            style.borderTopColor = new Color(0.3f, 0.35f, 0.45f, 0.4f);
            style.borderBottomColor = new Color(0.3f, 0.35f, 0.45f, 0.4f);
            style.borderLeftColor = new Color(0.3f, 0.35f, 0.45f, 0.4f);
            style.borderRightColor = new Color(0.3f, 0.35f, 0.45f, 0.4f);
            style.overflow = Overflow.Hidden;

            // 1. Top-Left Rank & Suit
            topRankLabel = new Label
            {
                name = "TopRank",
                pickingMode = PickingMode.Ignore,
                style =
                {
                    position = Position.Absolute,
                    left = 6f,
                    top = 4f,
                    fontSize = 14f,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };
            Add(topRankLabel);

            // 2. Center Large Suit Watermark
            centerSuitLabel = new Label
            {
                name = "CenterSuit",
                pickingMode = PickingMode.Ignore,
                style =
                {
                    position = Position.Absolute,
                    left = 0,
                    right = 0,
                    top = 0,
                    bottom = 0,
                    fontSize = 36f,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };
            Add(centerSuitLabel);

            // 3. Bottom-Right Rank & Suit
            bottomRankLabel = new Label
            {
                name = "BottomRank",
                pickingMode = PickingMode.Ignore,
                style =
                {
                    position = Position.Absolute,
                    right = 6f,
                    bottom = 4f,
                    fontSize = 14f,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };
            Add(bottomRankLabel);

            // 4. Center Index (for Layout Details mode)
            centerIndexLabel = new Label
            {
                name = "CenterIndex",
                pickingMode = PickingMode.Ignore,
                style =
                {
                    position = Position.Absolute,
                    left = 0,
                    right = 0,
                    top = 0,
                    bottom = 0,
                    fontSize = 24f,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    color = new Color(0.2f, 0.25f, 0.35f, 1f),
                    display = DisplayStyle.None
                }
            };
            Add(centerIndexLabel);

            UpdateCardVisuals(false);
        }

        public void SetIndex(int newIndex)
        {
            this.Index = newIndex;
            name = $"Card_{newIndex}";
            if (centerIndexLabel != null)
            {
                centerIndexLabel.text = $"#{newIndex}";
            }
        }

        public void SetCardData(CardData data, bool showLayoutDetails)
        {
            this.Data = data;
            UpdateCardVisuals(showLayoutDetails);
        }

        public void UpdateCardVisuals(bool showLayoutDetails)
        {
            if (Data == null) return;

            string suitSymbol = Data.GetSuitSymbol();
            Color suitColor = Data.GetSuitColor();

            if (showLayoutDetails)
            {
                centerIndexLabel.text = $"#{Index}";
                centerIndexLabel.style.display = DisplayStyle.Flex;
                topRankLabel.style.display = DisplayStyle.None;
                centerSuitLabel.style.display = DisplayStyle.None;
                bottomRankLabel.style.display = DisplayStyle.None;
            }
            else
            {
                centerIndexLabel.style.display = DisplayStyle.None;
                topRankLabel.style.display = DisplayStyle.Flex;
                centerSuitLabel.style.display = DisplayStyle.Flex;
                bottomRankLabel.style.display = DisplayStyle.Flex;

                topRankLabel.text = $"{Data.rank} {suitSymbol}";
                topRankLabel.style.color = suitColor;

                centerSuitLabel.text = suitSymbol;
                centerSuitLabel.style.color = new Color(suitColor.r, suitColor.g, suitColor.b, 0.55f);

                bottomRankLabel.text = $"{Data.rank} {suitSymbol}";
                bottomRankLabel.style.color = suitColor;
            }
        }

        public void SetVisualState(CardInteractionState state)
        {
            this.visualState = state;
            if (state == CardInteractionState.Hovered)
            {
                style.borderTopColor = new Color(0.2f, 0.5f, 0.9f, 1f);
                style.borderBottomColor = new Color(0.2f, 0.5f, 0.9f, 1f);
                style.borderLeftColor = new Color(0.2f, 0.5f, 0.9f, 1f);
                style.borderRightColor = new Color(0.2f, 0.5f, 0.9f, 1f);
                style.borderTopWidth = 2f;
                style.borderBottomWidth = 2f;
                style.borderLeftWidth = 2f;
                style.borderRightWidth = 2f;
            }
            else if (state == CardInteractionState.Dragged)
            {
                style.borderTopColor = new Color(0.2f, 0.8f, 0.4f, 1f);
                style.borderBottomColor = new Color(0.2f, 0.8f, 0.4f, 1f);
                style.borderLeftColor = new Color(0.2f, 0.8f, 0.4f, 1f);
                style.borderRightColor = new Color(0.2f, 0.8f, 0.4f, 1f);
                style.borderTopWidth = 2f;
                style.borderBottomWidth = 2f;
                style.borderLeftWidth = 2f;
                style.borderRightWidth = 2f;
            }
            else
            {
                style.borderTopColor = new Color(0.3f, 0.35f, 0.45f, 0.4f);
                style.borderBottomColor = new Color(0.3f, 0.35f, 0.45f, 0.4f);
                style.borderLeftColor = new Color(0.3f, 0.35f, 0.45f, 0.4f);
                style.borderRightColor = new Color(0.3f, 0.35f, 0.45f, 0.4f);
                style.borderTopWidth = 1f;
                style.borderBottomWidth = 1f;
                style.borderLeftWidth = 1f;
                style.borderRightWidth = 1f;
            }
        }

        public void SetTargetTransform(Vector2 targetPos, float targetSc)
        {
            this.targetPosition = targetPos;
            this.targetScale = targetSc;
        }

        public void UpdateMotion(float cardWidth, float cardHeight, float smoothTime, float maxSpeed, float deltaTime, bool isDirectDrag)
        {
            if (isDirectDrag)
            {
                currentPosition = targetPosition;
                currentScale = targetScale;
                velocity = Vector2.zero;
            }
            else
            {
                currentPosition = Vector2.SmoothDamp(currentPosition, targetPosition, ref velocity, smoothTime, maxSpeed, deltaTime);
                currentScale = Mathf.SmoothDamp(currentScale, targetScale, ref scaleVelocity, smoothTime, maxSpeed, deltaTime);
            }

            style.left = currentPosition.x - (cardWidth * 0.5f);
            style.top = currentPosition.y - (cardHeight * 0.5f);
            style.scale = new Scale(new Vector3(currentScale, currentScale, 1f));
        }

        public void SnapToTarget(float cardWidth, float cardHeight)
        {
            currentPosition = targetPosition;
            currentScale = targetScale;
            velocity = Vector2.zero;
            scaleVelocity = 0f;
            style.left = currentPosition.x - (cardWidth * 0.5f);
            style.top = currentPosition.y - (cardHeight * 0.5f);
            style.scale = new Scale(new Vector3(currentScale, currentScale, 1f));
        }
    }
}
