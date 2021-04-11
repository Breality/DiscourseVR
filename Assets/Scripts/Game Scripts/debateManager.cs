using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public struct intermissionMode
{
    public string message;
    public int timer;
}

public class debateManager : NetworkBehaviour
{
    public Dictionary<GameObject, int> spectatorIDs = new Dictionary<GameObject, int> { };
    public PlayerOveride mainScript;
    public SyncText textSync;

    List<Message> historyLogs = new List<Message> { };
    float totalTimeSpent = 0;
    float timeSpent = 0;
    int mode = 0;

    List<intermissionMode> modes = new List<intermissionMode> { 
        new intermissionMode { message  = "Waiting For Players", timer = 600 },
        new intermissionMode { message  = "Preperation", timer = 20 },
        new intermissionMode { message  = "Debater 1", timer = 60 },
        new intermissionMode { message  = "Intermission", timer = 15 },
        new intermissionMode { message  = "Debater 2", timer = 60 },
        new intermissionMode { message  = "Debate Conclusion", timer = 5 }
    };
    
    void nextMode()
    {
        Debug.Log("Switching modes");
        mode = (mode + 1) % 6;
        timeSpent = 0; //Time.unscaledTime;
        
        textSync.countdown = modes[mode].timer;
        textSync.stringMode = modes[mode].message;

        if (mode == 2 || mode == 0) // start recording here
        {
            Debug.Log("Start or end recording client");
            RpcAutomateCamera(mode == 2);
            if (mode == 2)
            {
                totalTimeSpent = 0;
                historyLogs = new List<Message> { };
                getNewList(historyLogs);
            }
        }
    }

    [ClientRpc]
    public void RpcAutomateCamera(bool mode)
    {
        string itemName = "CameraTing(Clone)";
        GameObject recorderCamera = GameObject.Find(itemName);

        if (recorderCamera != null && recorderCamera.transform.Find("VideoCaptureCtrl").gameObject.activeSelf)
        {
            Debug.Log("Got the request");
            localCamera direct_script = recorderCamera.GetComponent<localCamera>();
            direct_script.AutomateCamera(mode);
        }
    }


    public void RegisterMessage(GameObject sender, string message)
    {
        string timeShown = "0" + ((int)totalTimeSpent / 60).ToString() + ":" + (((int)totalTimeSpent % 60) < 10 ? "0" : "") + ((int)totalTimeSpent % 60).ToString();
        if (!spectatorIDs.ContainsKey(sender))
        {
            Debug.Log("No key????");
            return;
        }

        int connectID = spectatorIDs[sender];
        Debug.Log(message);
        Debug.Log(timeShown);
        Debug.Log(connectID);

        Message newMessage = new Message { text = message, timestamp = timeShown, spectator = connectID };
        historyLogs.Add(newMessage);

        Debug.Log(historyLogs);
        getNewList(historyLogs);
    }

    [ClientRpc]
    public void getNewList(List<Message> newLogs)
    {
        string itemName = "Cube(Clone)";
        foreach (GameObject item in SceneManager.GetActiveScene().GetRootGameObjects()) // check to see if we are a spectator
        {
            if (item.name == itemName && item.transform.Find("Camera").gameObject.GetComponent<Camera>().enabled)
            {
                localSpectator localScript = item.GetComponent<localSpectator>();
                localScript.chatManager.makeNewChat(newLogs);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            if (mode == 0)
            {
                if (mainScript.allPlayers > 1) // should be > 1 but testing rn
                {
                    nextMode();
                }
            }
            else
            {
                timeSpent += Time.deltaTime;
                totalTimeSpent += Time.deltaTime;

                int remaining = modes[mode].timer - (int)timeSpent;
                if (remaining < 0)
                {
                    nextMode();
                }
                else
                {
                    textSync.countdown = remaining;
                }
            }
        }
    }
}
