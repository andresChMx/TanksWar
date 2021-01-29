using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Desabler : MonoBehaviour {

    public void SetActiveFalse()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
