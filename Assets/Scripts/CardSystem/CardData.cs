using UnityEngine;

namespace FishONU.CardSystem
{
    public enum Color
    {
        Red,
        Blue,
        Green,
        Yellow,
        Black
    }

    public enum Face
    {
        Zero = 0,
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Skip,
        Reverse,
        DrawTwo,
        Wild,
        WildDrawFour,
        Back // 背面
    }

    [System.Serializable]
    public class CardData
    {
        public Color color;

        public Face face;

        // 卡牌唯一标识符
        [SerializeField] private string guid = "";

        public string Guid
        {
            get => guid;
            set => guid = value;
        }

        public CardData()
        {
            this.color = Color.Black;
            this.face = Face.Back;
            Guid = System.Guid.NewGuid().ToString();
        }

        public CardData(Color color = Color.Black, Face face = Face.Back)
        {
            this.color = color;
            this.face = face;
            Guid = System.Guid.NewGuid().ToString();
        }
    }
}