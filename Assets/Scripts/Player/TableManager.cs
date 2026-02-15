using Mirror;
using R3;
using System;
using System.Linq;
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

            seatOccupants.OnChange += OnSeatOccupantsChange;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

            seatOccupants.OnChange -= OnSeatOccupantsChange;
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

        private void OnDestroy()
        {
            _seatCountChangeSubject.OnCompleted();
        }

        private void OnSeatOccupantsChange(SyncList<NetworkIdentity>.Operation operation, int arg2, NetworkIdentity identity)
        {
            _seatCountChangeSubject.OnNext(
                seatOccupants
                .Where(x => x != null)
                .Count()); ;
        }

        #region Observable

        private readonly Subject<int> _seatCountChangeSubject = new();
        public Observable<int> OnSeatChangeAsObservable() => _seatCountChangeSubject;

        #endregion
    }
}