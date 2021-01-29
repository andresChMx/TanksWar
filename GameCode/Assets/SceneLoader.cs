using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
public class SceneLoader : MonoBehaviour {
    public GameObject loadingBar;
    Slider fill;
    Text textLoading;

    [DllImport("__Internal")]
    private static extern void InitBrowserConf();
    // Use this for initialization
    void Start () {
        fill = loadingBar.GetComponent<Slider>();
        textLoading = loadingBar.GetComponentInChildren<Text>();

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            InitBrowserConf();
        }
        else
        {
            LoadSceneAsync("SampleScene");
        }


	}
    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadScene(sceneName));
    }
	IEnumerator LoadScene(string name)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(name);
        while (!operation.isDone)
        {
            float progress= operation.progress / 0.9f;
            fill.value = progress;
            textLoading.text =((int)(progress*100f)).ToString() + "%";
            yield return null;
        }
    }

}
