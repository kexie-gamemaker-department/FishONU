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
        Zero,
        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Skip,
        Reverse,
        DrawTwo,
        Wild,
        WildDrawFour,
        Back // 背面
    }

    [System.Serializable]
    public class CardInfo
    {
        public Color color = Color.Black;
        public Face face = Face.Back;

        // 卡牌唯一标识符
        public readonly string Guid = System.Guid.NewGuid().ToString();
    }
}