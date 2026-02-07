using UnityEngine;

namespace FishONU.CardSystem.CardArrangeStrategy
{
    /// <summary>
    /// 卡片摆放策略接口
    /// </summary>
    public interface IArrangeStrategy
    {
        public Vector3 Calc(int index, int totalCount, out Vector3 position, out Vector3 rotation,
            out Vector3 scale);
    }
}