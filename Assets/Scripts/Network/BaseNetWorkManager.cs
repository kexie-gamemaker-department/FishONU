using FishONU.Player;
using Mirror;
using UnityEngine;


namespace FishONU.Network
{
    public class BaseNetWorkManager : NetworkManager
    {
        public int PlayerCount => GameObject.FindGameObjectsWithTag("Player").Length;
    }
}