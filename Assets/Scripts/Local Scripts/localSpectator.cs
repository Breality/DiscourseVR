using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Dissonance;

public class localSpectator : NetworkBehaviour
{
    public GameObject rig;
    public GameObject camera;
    public ChatMan chatManager;
    debateManager mainManager;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Server finding:");
        GameObject managerObject = GameObject.Find("/DebateManager");
        Debug.Log(managerObject);
        mainManager = managerObject.GetComponent<debateManager>();
        Debug.Log(mainManager);

        if (isLocalPlayer)
        {
            ((Camera)camera.GetComponent<Camera>()).enabled = true;
            GameObject dissonance = GameObject.Find("DissonanceSetup");
            DissonanceComms comms = dissonance.GetComponent<DissonanceComms>();
            comms.IsMuted = true;
            chatManager.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer)
        {
            transform.Translate(Input.GetAxis("Horizontal") * Time.deltaTime, 0f, Input.GetAxis("Vertical") * Time.deltaTime);
        }
    }

    public void sendMessage(string message)
    {
        registerMessage(message);
    }

    [Command]
    void registerMessage(string message)
    {
        mainManager.RegisterMessage(this.gameObject, message);
    }
}
