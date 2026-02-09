using UnityEngine;

namespace FishONU.CardSystem.CardArrangeStrategy
{
    public class CenterLinearArrange : IArrangeStrategy
    {
        public Vector3 CenterPosition = new(0f, 0f, 0f);
        public Vector3 PositionOffset = new(1.3f, 1.9f, 0f);

        public Vector3 CenterRotation = new(0f, 0f, 0f);
        public Vector3 RotationOffset = new(0f, 0f, 0f);

        // card width is 1.3f and height is 1.9f
        private static readonly Vector3 HalfCardOffset = new(0.65f, 0f, 0f);

        public Vector3 Calc(int index, int totalCount, out Vector3 position, out Vector3 rotation, out Vector3 scale)
        {
            // calc position
            var indexRate = index - (totalCount - 1) * 0.5f;
            position = HalfCardOffset + CenterPosition + indexRate * PositionOffset;

            rotation = CenterRotation + RotationOffset * index;
            scale = Vector3.one;
            return position;
        }
    }
}