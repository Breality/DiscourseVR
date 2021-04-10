using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public struct PlayerDetails : NetworkMessage
{
    public string clientType;
    // add whatever else we need as we go
}

public class PlayerOveride : NetworkManager
{
    public GameObject VRPlayer;
    public GameObject spectator;
    public GameObject recorder;

    string wantedMode;
    public void setMode(string wanted)
    {
        wantedMode = wanted;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        NetworkServer.RegisterHandler<PlayerDetails>(OnCreateCharacter);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        // you can send the message here, or wherever else you want
        PlayerDetails characterMessage = new PlayerDetails
        {
            clientType = wantedMode
        };

        conn.Send(characterMessage);
    }

    void OnCreateCharacter(NetworkConnection conn, PlayerDetails message)
    {
        Dictionary<string, GameObject> hash = new Dictionary<string, GameObject>();
        hash.Add("Recorder", recorder);
        hash.Add("VR Client", VRPlayer);
        hash.Add("Web Client", spectator);

        // playerPrefab is the one assigned in the inspector in Network
        // Manager but you can use different prefabs per race for example
        GameObject gameobject = Instantiate(hash[message.clientType]);

        // call this to use this gameobject as the primary controller
        NetworkServer.AddPlayerForConnection(conn, gameobject);
    }
}
