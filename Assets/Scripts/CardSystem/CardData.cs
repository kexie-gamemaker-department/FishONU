using System;
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

    [Serializable]
    public class CardData : IEquatable<CardData>
    {
        public Color color;
        public Color secondColor; // 用于黑牌变色，默认值是 Black

        public Face face;

        // 卡牌唯一标识符
        [SerializeField] public string guid = "";

        public string Guid
        {
            get => guid;
            set => guid = value;
        }

        public CardData()
        {
            color = Color.Black;
            face = Face.Back;
            secondColor = Color.Black;
            Guid = System.Guid.NewGuid().ToString();
        }

        public CardData(Color color = Color.Black, Face face = Face.Back)
        {
            this.color = color;
            this.face = face;
            Guid = System.Guid.NewGuid().ToString();
        }


        public bool Equals(CardData other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return guid == other.guid && color == other.color && face == other.face;
        }

        public override bool Equals(object obj) => Equals(obj as CardData);

        public override int GetHashCode()
        {
            return guid != null ? guid.GetHashCode() : 0;
        }

        public static bool operator ==(CardData left, CardData right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(CardData left, CardData right) => !(left == right);
    }
}