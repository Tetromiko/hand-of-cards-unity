using System;
using UnityEngine;

namespace Tetromiko.CardsHandLayout
{
    [Serializable]
    public class HandSettings
    {
        [Header("Dimensions")]
        [Tooltip("Total available width of the hand container in units/pixels.")]
        public float handWidth = 680f;

        [Tooltip("Width of an individual card.")]
        public float cardWidth = 112f;

        [Tooltip("Height of an individual card.")]
        public float cardHeight = 160f;

        [Header("Spacing & Compression")]
        [Tooltip("Base distance between cards in idle/uncompressed state.")]
        public float cardDistance = 56f;

        [Tooltip("Minimum allowable distance between cards when hand is compressed.")]
        public float minCardDistance = 24f;

        [Header("Hover Interaction")]
        [Tooltip("Slot width allocated for the hovered/active card (>= cardWidth).")]
        public float hoverDistance = 112f;

        [Tooltip("Vertical lift distance when a card is hovered.")]
        public float hoverLift = 28f;

        [Tooltip("Scale multiplier for the hovered card.")]
        public float hoverScale = 1.05f;

        [Tooltip("Scale multiplier for the dragged card.")]
        public float dragScale = 1.1f;

        [Header("Animation & Physics")]
        [Tooltip("Smooth time for spring-like interpolation (seconds).")]
        [Range(0.01f, 0.5f)]
        public float smoothTime = 0.08f;

        [Tooltip("Maximum movement speed.")]
        public float maxSpeed = 3000f;

        [Header("Visual Debug")]
        [Tooltip("Draw gizmo debug lines for slots and hand bounds in Scene view.")]
        public bool drawGizmos = true;

        public static HandSettings CreateDefault()
        {
            return new HandSettings
            {
                handWidth = 680f,
                cardWidth = 112f,
                cardHeight = 160f,
                cardDistance = 56f,
                minCardDistance = 24f,
                hoverDistance = 112f,
                hoverLift = 28f,
                hoverScale = 1.05f,
                dragScale = 1.1f,
                smoothTime = 0.08f,
                maxSpeed = 3000f,
                drawGizmos = true
            };
        }
    }
}
