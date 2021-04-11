using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.IO;

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
    public string filePath;

    List<Message> historyLogs = new List<Message> { };
    float totalTimeSpent = 0;
    float timeSpent = 0;
    int mode = 0;

    List<intermissionMode> modes = new List<intermissionMode> { 
        new intermissionMode { message  = "Waiting For Players", timer = 600 },
        new intermissionMode { message  = "Preperation", timer = 5 },
        new intermissionMode { message  = "Debater 1", timer = 1 },
        new intermissionMode { message  = "Intermission", timer = 1 },
        new intermissionMode { message  = "Debater 2", timer = 1 },
        new intermissionMode { message  = "Debate Conclusion", timer = 1 }
    };

    private List<Message> hiddenHistory;
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

            if (mode == 2)
            {
                totalTimeSpent = 0;
            }
            else // print history logs
            {
                hiddenHistory = new List<Message> {
                new Message { spectator = 1, text = "testing first line haha", timestamp = "00:05"},
                new Message { spectator = 1, text = "testing second line haha", timestamp = "00:35"},
                new Message { spectator = 2, text = "testing third line with second spectator haha", timestamp = "01:25"}
                };
            }

            RpcAutomateCamera(mode == 2);
            historyLogs = new List<Message> { };
            getNewList(historyLogs.ToArray());
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
        Message newMessage = new Message { text = message, timestamp = timeShown, spectator = connectID };
        historyLogs.Add(newMessage);

        Message[] items = historyLogs.ToArray();
        getNewList(items);
    }

    [ClientRpc]
    public void getNewList(Message[] newLogs)
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

    public void RegisterFileName(string extra)
    {
        createFile(extra);
        Debug.Log("Sent request to make file with " + extra);
    }

    [Command(requiresAuthority = false)]
    void createFile(string naming)
    {
        int count = hiddenHistory.Count;
        Debug.Log("Will now create " + naming + " file for " + count + " messages");
        string fullPath = filePath + naming.Replace(".mp4", ".json");

        StreamWriter writer = new StreamWriter(fullPath, true);

        writer.WriteLine("[");
        int i = 0;
        foreach (Message m in hiddenHistory)
        {
            i++;
            writer.WriteLine("{");

            writer.WriteLine("\"sender\" : " + m.spectator.ToString() + ",");
            int seconds = int.Parse(m.timestamp.Substring(0, 2)) * 60 + int.Parse(m.timestamp.Substring(3, 2));
            writer.WriteLine("\"timestamp\" : " + seconds.ToString() + ",");
            writer.WriteLine("\"message\" : \"" + System.Net.WebUtility.HtmlEncode(m.text) + "\"");

            writer.WriteLine("}" + (i < count ? "," : "") );
        }
        writer.WriteLine("]");


        writer.Close();

        Debug.Log("Finished making folder!");
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
                if (mainScript.allPlayers > -1) // should be > 1 but testing rn
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
