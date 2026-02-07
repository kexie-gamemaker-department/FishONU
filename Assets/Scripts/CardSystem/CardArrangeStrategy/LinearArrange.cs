using System;
using System.Collections.Generic;
using UnityEngine;

namespace FishONU.CardSystem.CardArrangeStrategy
{
    public class LinearArrange : IArrangeStrategy
    {
        public Vector3 StartPosition = new(0f, 0f, 0f);
        public Vector3 PositionOffset = new(1.3f, 1.9f, 0f);

        public Vector3 StartRotation = new(0f, 0f, 0f);
        public Vector3 RotationOffset = new(0f, 0f, 0f);


        public Vector3 Calc(int index, int totalCount, out Vector3 position, out Vector3 rotation, out Vector3 scale)
        {
            position = StartPosition + PositionOffset * index;
            rotation = StartRotation + RotationOffset * index;
            scale = Vector3.one;
            return position;
        }
    }
}