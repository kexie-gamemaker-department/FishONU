using System.Collections.Generic;
using UnityEngine;

namespace FishONU.CardSystem.CardArrangeStrategy
{
    public class RandomSpreadArrange : IArrangeStrategy
    {
        public Vector3 CenterPosition = Vector3.zero;
        public float MaxOffset = 1.0f;
        public float MaxRotaion = 30.0f;

        private readonly List<Vector3> _posOffset = new();
        private readonly List<float> _rotOffset = new();

        public void Calc(int index, int totalCount, out Vector3 position, out Vector3 rotation, out Vector3 scale)
        {
            // 如果请求的索引超出了当前存储的随机值，则生成新的随机值 
            while (_posOffset.Count <= totalCount)
            {
                _posOffset.Add(new Vector3(
                    Random.Range(-MaxOffset, MaxOffset),
                    Random.Range(-MaxOffset, MaxOffset),
                    -index * 0.01f
                ));
                _rotOffset.Add(Random.Range(-MaxRotaion, MaxRotaion));
            }

            position = CenterPosition + _posOffset[index];
            rotation = new Vector3(0f, 0f, _rotOffset[index]);
            scale = Vector3.one;
        }

        public void Clear()
        {
            _posOffset.Clear();
            _rotOffset.Clear();
        }
    }
}