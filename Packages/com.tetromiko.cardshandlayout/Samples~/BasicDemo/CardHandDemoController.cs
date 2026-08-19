using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Tetromiko.CardsHandLayout.Samples
{
    public class CardHandDemoController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CardHandController handController;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Slider handWidthSlider;
        [SerializeField] private Slider cardDistanceSlider;
        [SerializeField] private Slider hoverLiftSlider;
        [SerializeField] private Button addCardButton;
        [SerializeField] private Button removeCardButton;
        [SerializeField] private Button resetButton;

        private void Start()
        {
            if (handController == null)
            {
                handController = FindObjectOfType<CardHandController>();
            }

            if (handController != null)
            {
                // Wire Events
                handController.onCardClicked.AddListener(OnCardClicked);
                handController.onCardHovered.AddListener(OnCardHovered);
                handController.onCardUnhovered.AddListener(OnCardUnhovered);
                handController.onCardsReordered.AddListener(OnCardsReordered);
                handController.onHandUpdated.AddListener(UpdateUI);

                // Wire UI Sliders
                if (handWidthSlider != null)
                {
                    handWidthSlider.minValue = 200f;
                    handWidthSlider.maxValue = 1200f;
                    handWidthSlider.value = handController.Settings.handWidth;
                    handWidthSlider.onValueChanged.AddListener((v) =>
                    {
                        handController.Settings.handWidth = v;
                    });
                }

                if (cardDistanceSlider != null)
                {
                    cardDistanceSlider.minValue = handController.Settings.minCardDistance;
                    cardDistanceSlider.maxValue = 200f;
                    cardDistanceSlider.value = handController.Settings.cardDistance;
                    cardDistanceSlider.onValueChanged.AddListener((v) =>
                    {
                        handController.Settings.cardDistance = v;
                    });
                }

                if (hoverLiftSlider != null)
                {
                    hoverLiftSlider.minValue = 0f;
                    hoverLiftSlider.maxValue = 100f;
                    hoverLiftSlider.value = handController.Settings.hoverLift;
                    hoverLiftSlider.onValueChanged.AddListener((v) =>
                    {
                        handController.Settings.hoverLift = v;
                    });
                }

                if (addCardButton != null) addCardButton.onClick.AddListener(() => handController.AddCard());
                if (removeCardButton != null) removeCardButton.onClick.AddListener(() => handController.RemoveLastCard());
                if (resetButton != null) resetButton.onClick.AddListener(() => handController.CreateDefaultCards(5));
            }

            UpdateUI();
        }

        private void OnCardClicked(CardView card)
        {
            if (statusText != null)
            {
                statusText.text = $"Clicked: {card.CardData?.rank} of {card.CardData?.suit} (#{card.CurrentIndex})";
            }
        }

        private void OnCardHovered(CardView card)
        {
            if (statusText != null)
            {
                statusText.text = $"Hovered: {card.CardData?.rank} of {card.CardData?.suit} (#{card.CurrentIndex})";
            }
        }

        private void OnCardUnhovered(CardView card)
        {
            if (statusText != null)
            {
                statusText.text = "Hand Idle";
            }
        }

        private void OnCardsReordered(int fromIndex, int toIndex)
        {
            if (statusText != null)
            {
                statusText.text = $"Reordered card from slot {fromIndex} -> {toIndex}";
            }
        }

        private void UpdateUI()
        {
            if (handController != null && statusText != null && !handController.CardViews.Count.Equals(0))
            {
                statusText.text = $"Cards in hand: {handController.CardsCount}";
            }
        }
    }
}
