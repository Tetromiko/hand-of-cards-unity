using System;
using UnityEngine;

namespace Tetromiko.CardsHandLayout
{
    public enum CardSuit
    {
        Spades,
        Hearts,
        Diamonds,
        Clubs
    }

    [Serializable]
    public class CardData
    {
        public string id;
        public string title;
        public CardSuit suit;
        public string rank;
        public Sprite cardSprite;
        public int manaCost;

        public CardData()
        {
            id = Guid.NewGuid().ToString("N").Substring(0, 8);
            title = "New Card";
            suit = CardSuit.Spades;
            rank = "A";
        }

        public CardData(string id, string rank, CardSuit suit, Sprite sprite = null, string title = "")
        {
            this.id = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString("N").Substring(0, 8) : id;
            this.rank = rank;
            this.suit = suit;
            this.cardSprite = sprite;
            this.title = string.IsNullOrEmpty(title) ? $"{rank} of {suit}" : title;
        }

        public Color GetSuitColor()
        {
            return (suit == CardSuit.Hearts || suit == CardSuit.Diamonds)
                ? new Color(0.95f, 0.25f, 0.25f, 1f)
                : new Color(0.15f, 0.15f, 0.18f, 1f);
        }

        public string GetSuitSymbol()
        {
            switch (suit)
            {
                case CardSuit.Spades: return "♠";
                case CardSuit.Hearts: return "♥";
                case CardSuit.Diamonds: return "♦";
                case CardSuit.Clubs: return "♣";
                default: return "";
            }
        }
    }
}
