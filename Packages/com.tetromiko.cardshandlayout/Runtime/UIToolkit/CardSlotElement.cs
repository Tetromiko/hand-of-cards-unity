using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tetromiko.CardsHandLayout.UIToolkit
{
    /// <summary>
    /// UI Toolkit spatial slot element that handles pointer interactions and hover zones.
    /// Captures pointer events directly and forwards them to CardHandUIToolkitController.
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
            pickingMode = PickingMode.Position;
            style.position = Position.Absolute;
            style.top = 0;
            style.bottom = 0;
            // Tiny non-zero alpha ensures standard hit testing across all Unity UI Toolkit versions
            style.backgroundColor = new Color(0f, 0f, 0f, 0.001f);

            // Pointer Events (Modern UI Toolkit)
            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            RegisterCallback<PointerDownEvent>(OnPointerDown);

            // Mouse Events (Legacy/Fallback)
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        public void SetSlotIndex(int index)
        {
            this.SlotIndex = index;
            name = $"Slot_{index}";
        }

        public void UpdateBounds(SlotHoverZone zone, float containerCenter, float height)
        {
            float leftPos = containerCenter + zone.MinX;
            style.left = leftPos;
            style.width = zone.Width;
            style.top = 0;
            style.bottom = 0;
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            controller?.OnSlotPointerEnter(SlotIndex);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            controller?.OnSlotPointerMove(SlotIndex, evt.position);
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            controller?.OnSlotPointerLeave(SlotIndex);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button == 0) // Left mouse button
            {
                controller?.OnSlotPointerDown(SlotIndex, evt.pointerId, evt.position);
            }
        }

        // Mouse fallbacks
        private void OnMouseEnter(MouseEnterEvent evt)
        {
            controller?.OnSlotPointerEnter(SlotIndex);
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            controller?.OnSlotPointerMove(SlotIndex, evt.mousePosition);
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            controller?.OnSlotPointerLeave(SlotIndex);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                controller?.OnSlotPointerDown(SlotIndex, 0, evt.mousePosition);
            }
        }
    }
}
