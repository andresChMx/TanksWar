using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnitySocketIO;
using UnitySocketIO.Events;
public class ChatManager : MonoBehaviour {
    public ColorRGB msgColor;
    public RectTransform container;
    public InputField inputField;

    public GameObject prefMessaje;
    public List<RectTransform> messageList;

	// Use this for initialization
	void Start () {
        messageList = new List<RectTransform>();

	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("ENTERRRR");
            if (inputField.text != "")
            {
                sendMessage(inputField.text);
                AddMessage(Network.player.alias,inputField.text,msgColor);
                inputField.text = "";
            }
        }
	}
    void sendMessage(string message)
    {
        Network.NewMessage(message,msgColor);
        Debug.Log(message);
    }
    public void AddMessage(string nombre,string message,ColorRGB color)
    {

        RectTransform go=Instantiate(prefMessaje,container).GetComponent<RectTransform>();
        go.anchoredPosition = new Vector3(0,-messageList.Count*20,0);

        Text tmp=go.GetComponent<Text>();
        tmp.text = nombre + " : " + message;
        tmp.color = new Color32((byte)color.r, (byte)color.g, (byte)color.b,255);
        messageList.Add(go);
        if (messageList.Count * 20 >=container.sizeDelta.y)
        {
            container.sizeDelta = new Vector2(container.sizeDelta.x,container.sizeDelta.y+20);
        }
    }
}
public struct ColorRGB
{
    public int r;
    public int g;
    public int b;
    public ColorRGB(int r,int g,int b)
    {
        this.r = r;
        this.g = g;
        this.b = b;
    }
}
