using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace FishONU.Player
{
    public class TableManager : NetworkBehaviour
    {
        public readonly SyncList<NetworkIdentity> seatOccupants = new();

        public int maxPlayers { get; private set; } = 4;

        // private List<GameObject> seatAnchors;

        public override void OnStartServer()
        {
            base.OnStartServer();

            for (int i = 0; i < maxPlayers; i++)
            {
                // TODO: 
                seatOccupants.Add(null);
            }
        }

        [Server]
        public int RequestSitDown(NetworkIdentity identity)
        {
            for (var i = 0; i < maxPlayers; i++)
            {
                Debug.Log($"{identity}:{seatOccupants[i]}");
                if (seatOccupants[i] != null) continue;
                seatOccupants[i] = identity;
                return i;
            }

            return -1;
        }

        [Server]
        public int RequestStandUp(NetworkIdentity identity)
        {
            for (var i = 0; i < maxPlayers; i++)
            {
                if (seatOccupants[i] != identity) continue;
                seatOccupants[i] = null;
                return i;
            }

            return -1;
        }
    }
}