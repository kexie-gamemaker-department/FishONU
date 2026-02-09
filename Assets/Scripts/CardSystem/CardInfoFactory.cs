using System.Collections.Generic;

namespace FishONU.CardSystem
{
    public static class CardInfoFactory
    {
        public static CardInfo CreateCard(Color color, Face face)
        {
            return new CardInfo(color, face);
        }

        public static List<CardInfo> CreateCards(Color color, Face face, int number = 1)
        {
            var cards = new List<CardInfo>();

            for (int i = 0; i < number; i++)
            {
                cards.Add(new CardInfo(color, face));
            }

            return cards;
        }

        public static List<CardInfo> CreateStandardDeck()
        {
            List<CardInfo> deck = new List<CardInfo>();

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
    }
}