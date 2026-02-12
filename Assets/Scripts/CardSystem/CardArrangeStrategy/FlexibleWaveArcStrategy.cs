using UnityEngine;

namespace FishONU.CardSystem.CardArrangeStrategy
{
    public class FlexibleWaveArcStrategy : IArrangeStrategy
    {
        public Vector3 CenterPosition = Vector3.zero;
        public float Radius = 10f; // 基础圆半径
        public float MaxAngle = 40f; // 最大扇形角度
        public float HeightIntensity = 1.5f; // 控制上下浮动的强度
        public float Spacing = 1.2f; // 牌与牌之间的基础横向间距

        public void Calc(int index, int totalCount, out Vector3 position, out Vector3 rotation, out Vector3 scale)
        {
            // 1. 计算标准化的索引偏移 (-0.5 到 0.5)
            float indexOffset = (totalCount <= 1) ? 0 : (index / (float)(totalCount - 1) - 0.5f);

            // 2. 计算横向位置 (X) - 这里的 Spacing 可以让中心更密集或更稀疏
            float x = indexOffset * (totalCount - 1) * Spacing;

            // 3. 计算高度位置 (Y) 
            // 我们不直接用圆公式，而是用 Cosine 波形来模拟弧度，这样更平滑且易控
            // 当 indexOffset 为 0 (中间) 时，Cos 为 1；当靠近边缘时，Cos 变小
            // 这里的 1.5f 弧度范围可以让曲线在边缘处下降得更明显
            float wave = Mathf.Cos(indexOffset * Mathf.PI * 0.8f);
            float y = wave * HeightIntensity;

            // 4. 结合基础高度和 Z 轴深度偏移
            position = CenterPosition + new Vector3(x, y, -index * 0.02f);

            // 5. 旋转计算 (基于角度的偏转)
            float angle = indexOffset * MaxAngle;
            rotation = new Vector3(0, 0, -angle);

            scale = Vector3.one;
        }
    }
}