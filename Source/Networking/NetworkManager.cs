using System.Collections.Generic;
using UnityEngine;

namespace CWVR.Networking;

public class NetworkManager : MonoBehaviour
{
    private readonly Dictionary<global::Player, NetworkPlayer> vrPlayers = [];

    public bool InVR(global::Player player)
    {
        return vrPlayers.ContainsKey(player);
    }
    
    public NetworkPlayer RegisterVRPlayer(global::Player player)
    {
        var netPlayer = player.gameObject.AddComponent<NetworkPlayer>();
        vrPlayers.Add(player, netPlayer);

        return netPlayer;
    }

    public bool TryGetNetworkPlayer(global::Player player, out NetworkPlayer netPlayer)
    {
        return vrPlayers.TryGetValue(player, out netPlayer);
    }
}