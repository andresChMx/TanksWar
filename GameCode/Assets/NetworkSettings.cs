using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class NetworkSettings:MonoBehaviour{
    public string currentIp;
    public SceneLoader loader; 
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void setIp(string ip)
    {
        currentIp = ip;
    }
}
