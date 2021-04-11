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
    public GameObject spawnLocations;
    public debateManager debateHandler;

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

    public int players = 0;
    public int spectators = 0;
    public int allPlayers = 0;

    void OnCreateCharacter(NetworkConnection conn, PlayerDetails message)
    {
        Debug.Log("Creating character with mode: " + message.clientType);
        Dictionary<string, GameObject> hash = new Dictionary<string, GameObject>();
        hash.Add("Recorder", recorder);
        hash.Add("VR Client", VRPlayer);
        hash.Add("Web Client", spectator);

        // playerPrefab is the one assigned in the inspector in Network
        // Manager but you can use different prefabs per race for example
        string selectSpawn = "Camera";
        if (message.clientType == "VR Client")
        {
            selectSpawn = "Player " + players.ToString();
            players = (players + 1) % 2;
            allPlayers += 1;
        }
        else if (message.clientType == "Web Client")
        {
            selectSpawn = "Spectator " + spectators.ToString();
            spectators = (spectators + 1) % 5;
        }

        Transform spawn = spawnLocations.transform.Find(selectSpawn);
        GameObject gameobject = Instantiate(hash[message.clientType], spawn.position, spawn.rotation);

        // call this to use this gameobject as the primary controller
        NetworkServer.AddPlayerForConnection(conn, gameobject);
        debateHandler.spectatorIDs.Add(gameobject, spectators);
    }
}
