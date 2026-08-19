using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Tetromiko.CardsHandLayout
{
    /// <summary>
    /// Represents an interaction slot in the card hand.
    /// The slot is the authoritative hit and spatial activation zone for card operations.
    /// When the pointer enters or hovers over a slot, its associated card is activated.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class CardSlotView : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerMoveHandler,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [SerializeField] private int slotIndex;
        private CardHandController controller;
        private RectTransform rectTransform;
        private Image raycastImage;

        public int SlotIndex => slotIndex;
        public RectTransform RectTransform => rectTransform != null ? rectTransform : (rectTransform = GetComponent<RectTransform>());

        public void Initialize(int index, CardHandController handController)
        {
            this.slotIndex = index;
            this.controller = handController;

            rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // Ensure slot has an invisible raycast receiver
            raycastImage = GetComponent<Image>();
            if (raycastImage == null)
            {
                raycastImage = gameObject.AddComponent<Image>();
            }
            raycastImage.color = Color.clear;
            raycastImage.raycastTarget = true;
        }

        public void SetSlotIndex(int newIndex)
        {
            this.slotIndex = newIndex;
            name = $"Slot_{newIndex}";
        }

        public void UpdateZone(SlotHoverZone zone, float height)
        {
            if (RectTransform == null) return;
            RectTransform.anchoredPosition = new Vector2(zone.CenterX, 0f);
            RectTransform.sizeDelta = new Vector2(zone.Width, height);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (controller != null)
            {
                controller.OnSlotPointerEnter(slotIndex);
            }
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (controller != null)
            {
                controller.OnSlotPointerMove(slotIndex, eventData);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (controller != null)
            {
                controller.OnSlotPointerExit(slotIndex);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (controller != null)
            {
                controller.OnSlotClicked(slotIndex);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (controller != null)
            {
                controller.OnSlotBeginDrag(slotIndex, eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (controller != null)
            {
                controller.OnSlotDrag(slotIndex, eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (controller != null)
            {
                controller.OnSlotEndDrag(slotIndex, eventData);
            }
        }
    }
}
