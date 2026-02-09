using System.Collections.Generic;
using UnityEngine;

namespace FishONU.CardSystem
{
    public static class CardInfoFactory
    {
        public static CardData CreateCard(Color color, Face face)
        {
            return new CardData(color, face);
        }

        public static List<CardData> CreateCards(Color color, Face face, int number = 1)
        {
            var cards = new List<CardData>();

            for (int i = 0; i < number; i++)
            {
                cards.Add(new CardData(color, face));
            }

            return cards;
        }

        public static List<CardData> CreateStandardDeck()
        {
            List<CardData> deck = new List<CardData>();

            // 有色牌
            Color[] standardColor = { Color.Red, Color.Blue, Color.Green, Color.Yellow };
            foreach (var color in standardColor)
            {
                deck.Add(CardInfoFactory.CreateCard(color, Face.Zero));

                for (int i = 1; i < 10; i++) deck.AddRange(CardInfoFactory.CreateCards(color, (Face)i, 2));

                deck.AddRange(CardInfoFactory.CreateCards(color, Face.Skip, 2));
                deck.AddRange(CardInfoFactory.CreateCards(color, Face.Reverse, 2));
                deck.AddRange(CardInfoFactory.CreateCards(color, Face.DrawTwo, 2));
            }

            deck.AddRange(CardInfoFactory.CreateCards(Color.Black, Face.Wild, 4));
            deck.AddRange(CardInfoFactory.CreateCards(Color.Black, Face.WildDrawFour, 4));

            return deck;
        }

        public static CardData CreateRandomCard()
        {
            return CreateCard((Color)Random.Range(0, 4), (Face)Random.Range(0, 15));
        }
    }
}