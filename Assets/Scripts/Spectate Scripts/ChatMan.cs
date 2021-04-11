using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public struct Message
{
    public int spectator;
    public string timestamp;
    public string text;
}
public class ChatMan : MonoBehaviour
{
    public localSpectator networkedScript;

    // Start is called before the first frame update
    public GameObject iField;
    public GameObject contentParent;
    public GameObject testMessageTemplate;
    private List<string> colors = new List<string> { "#bada55", "#7fe5f0", "#ffbe00",
                                                     "#ff80ed", "#5ac18e", "#ffc0cb",
                                                     "#ccff00", "#00ffff", "#c39797", "#ff0000"};
    // Update is called once per frame
    void Update()
    {
        string message = iField.GetComponent<TMP_InputField>().text;
        if (message != "" && Input.GetKeyUp(KeyCode.Return))
        {
            networkedScript.sendMessage(message);
            iField.GetComponent<TMP_InputField>().text = "";
        }
    }

    public void makeNewChat(Message[] messages)
    {
        foreach(Transform child in contentParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach(Message m in messages)
        {
            int spectator = m.spectator;
            string spectat = "<color="+colors[(spectator - 1) % 10]+">"+ " Spectator #" + spectator.ToString() + ": " + "</color>";
            string timestamp = "<color=orange>[" + m.timestamp + "]</color>";
            string message = timestamp + spectat + m.text;
            GameObject newItem = Instantiate(testMessageTemplate, contentParent.transform);
            TMP_Text m_TextComponent = newItem.GetComponent<TMP_Text>();
            m_TextComponent.text = message;
            newItem.SetActive(true);

        }
    }
}
