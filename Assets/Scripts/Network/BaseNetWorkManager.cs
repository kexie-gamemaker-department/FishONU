using Mirror;
using R3;
using UnityEngine;

namespace FishONU.Network
{
    public class BaseNetworkManager : NetworkManager
    {
        public int PlayerCount => GameObject.FindGameObjectsWithTag("Player").Length;


        #region Observables

        private readonly Subject<Unit> _onClientDisconnected = new();
        public Observable<Unit> OnClientDisconnectedAsObservable() => _onClientDisconnected;

        private readonly Subject<Unit> _onClientConnected = new();
        public Observable<Unit> OnClientConnectedAsObservable() => _onClientConnected;

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            _onClientDisconnected.OnNext(Unit.Default);
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            _onClientConnected.OnNext(Unit.Default);
        }

        public override void OnDestroy()
        {
            _onClientDisconnected.OnCompleted();
            _onClientConnected.OnCompleted();
            base.OnDestroy();
        }

        #endregion
    }
}