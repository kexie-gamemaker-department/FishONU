using UnityEngine;

namespace FishONU.Player
{
    public static class SeatHelper
    {
        private const int MaxSeatCount = 4;

        public static int CalcLocalSeatIndex(int ownerSeatIndex, int playerSeatIndex)
        {
            var localSeatIndex = (playerSeatIndex - ownerSeatIndex + MaxSeatCount) % MaxSeatCount;
            return localSeatIndex;
        }

        public static void SitAt(int localSeatIndex, GameObject player)
        {
            var anchor = GameObject.Find($"SeatAnchor{localSeatIndex}");

            if (anchor == null)
            {
                Debug.LogError($"SeatAnchor{localSeatIndex} not found");
            }

            player.transform.position = anchor.transform.position;

            switch (localSeatIndex)
            {
                case 0:
                    player.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
                    break;
                case 1:
                    player.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
                    break;
                case 2:
                    player.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
                    break;
                case 3:
                    player.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -90f));
                    break;
            }
        }
    }
}