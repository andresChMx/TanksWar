using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour {
    public Transform target;
    TankController targetController;
    public  Vector3 offset;
    float focusSpeed = 4f;
    public bool follow = false;

    public LayerMask groundLayer;
	// Use this for initialization
	void Start () {

	}
	public void StartFollowing()
    {
        targetController = target.GetComponent<TankController>();
        follow = true;
    }
	// Update is called once per frame
	void Update () {
        if (target != null)
        {
            transform.position = Vector3.Lerp(transform.position, target.position + offset, Time.deltaTime * focusSpeed);
            transform.LookAt(target);
        }
        if (follow)
        {


            Ray ray = gameObject.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit,groundLayer))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    targetController.CreateBullet(hit.point);
                }
                targetController.SetCanonAngle(hit.point);
            }
        }
     
    }
}
