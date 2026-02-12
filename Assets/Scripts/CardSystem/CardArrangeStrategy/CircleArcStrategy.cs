using UnityEngine;

namespace FishONU.CardSystem.CardArrangeStrategy
{
    public class CircleArcStrategy : IArrangeStrategy
    {
        public Vector3 CenterPosition = Vector3.zero; // 手牌中心的参考原点
        public float Radius = 15f; // 圆弧半径，值越大弧度越平缓
        public float MaxAngle = 40f; // 手牌展开的最大总角度
        public float CardSpacingAngle = 5f; // 每张牌之间的理想间隔角度

        public void Calc(int index, int totalCount, out Vector3 position, out Vector3 rotation, out Vector3 scale)
        {
            // 计算总跨度：如果牌少，按间隔算；如果牌多，压缩在 MaxAngle 内
            float currentTotalAngle = Mathf.Min(MaxAngle, CardSpacingAngle * (totalCount - 1));

            // 计算当前卡牌相对于中心点的偏转角 (弧度)
            // indexOffset 范围从 -0.5 到 0.5
            float indexOffset = (totalCount <= 1) ? 0 : (index / (float)(totalCount - 1) - 0.5f); // 归一化然后左移 0.5
            float angleValue = indexOffset * currentTotalAngle;
            float angleRad = angleValue * Mathf.Deg2Rad; // l = theta * r

            // 计算位置 (基于圆心在 CenterPosition 下方 Radius 处)
            // 圆心坐标 = CenterPosition + (0, -Radius, 0)
            Vector3 circleCenter = CenterPosition + Vector3.down * Radius;

            float x = Mathf.Sin(angleRad) * Radius;
            float y = Mathf.Cos(angleRad) * Radius;

            // 最终位置 = 圆心 + 偏移坐标 + Z轴深度避让(防止闪烁)
            position = circleCenter + new Vector3(x, y, -index * 0.01f);

            // 计算旋转：卡牌垂直于圆弧半径
            rotation = new Vector3(0, 0, -angleValue);

            scale = Vector3.one;
        }
    }
}