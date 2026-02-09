using UnityEngine;

namespace FishONU.CardSystem.CardArrangeStrategy
{
    public class CenterLinearWithArc : IArrangeStrategy
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
            var indexOffset = index - (totalCount - 1) * 0.5f;

            // 位置：X轴平铺，Y轴根据距离中心的远近稍微下垂（形成弧形）;
            position = HalfCardOffset + CenterPosition + new Vector3(
                indexOffset * PositionOffset.x,
                -Mathf.Abs(indexOffset) * PositionOffset.y,
                -index * 0.01f
            );

            // 旋转：越靠两边旋转角度越大
            rotation = CenterRotation + new Vector3(
                RotationOffset.x,
                RotationOffset.y,
                indexOffset * RotationOffset.z);

            scale = Vector3.one;
            return position;
        }
    }
}