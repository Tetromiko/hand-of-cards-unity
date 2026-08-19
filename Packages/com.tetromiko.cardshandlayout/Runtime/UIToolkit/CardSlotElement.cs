using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tetromiko.CardsHandLayout.UIToolkit
{
    /// <summary>
    /// UI Toolkit spatial slot element that handles pointer interactions and hover zones.
    /// </summary>
    public class CardSlotElement : VisualElement
    {
        public int SlotIndex { get; private set; }
        private readonly CardHandUIToolkitController controller;

        public CardSlotElement(int index, CardHandUIToolkitController handController)
        {
            this.SlotIndex = index;
            this.controller = handController;

            name = $"Slot_{index}";
            style.position = Position.Absolute;
            style.top = 0;
            style.bottom = 0;
            style.backgroundColor = Color.clear;

            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
        }

        public void SetSlotIndex(int index)
        {
            this.SlotIndex = index;
            name = $"Slot_{index}";
        }

        public void UpdateBounds(SlotHoverZone zone, float containerCenter, float height)
        {
            // Position relative to container center
            float leftPos = containerCenter + zone.MinX;
            style.left = leftPos;
            style.width = zone.Width;
            style.height = height;
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            controller?.OnSlotPointerEnter(SlotIndex);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            controller?.OnSlotPointerMove(SlotIndex, evt);
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            controller?.OnSlotPointerLeave(SlotIndex);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button == 0) // Left mouse button
            {
                controller?.OnSlotPointerDown(SlotIndex, evt);
            }
        }
    }
}
