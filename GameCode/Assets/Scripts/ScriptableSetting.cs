using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New settings",menuName ="Settings/new setting")]
public class ScriptableSetting : ScriptableObject {

    public string url;
    public int port;

    public bool sslEnabled;

    public int reconnectTime;

    public int timeToDropAck;

    public int pingTimeout;
    public int pingInterval;
}
