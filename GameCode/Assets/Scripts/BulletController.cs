using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour {
    public string alias;
    public int id;
    public string parentId;
    public bool isBullet=false;

    public float velocityX;
    public float velocityY;
    public float time;
    public float direction;

    public float elapsetTime=0;
    public Vector3 startingPosition;
    bool startMovement = false;
	// Use this for initialization
	void Start () {
        startingPosition = transform.position;
        Invoke("TimeOut",4f);
	}
	
	// Update is called once per frame
	void Update () {
        if (startMovement)
        {
            elapsetTime += Time.deltaTime;
            Vector3 dir = new Vector3(Mathf.Cos(direction),0, Mathf.Sin(direction));
            Vector3 movement = new Vector3(velocityX * elapsetTime, velocityY* elapsetTime -((0.5f)*9.81f* elapsetTime * elapsetTime) ,velocityX * elapsetTime);

            Vector3 pos = startingPosition+(new Vector3(dir.x*movement.x,dir.y*movement.y,dir.z*movement.z)); 
            transform.position = pos;

        }
	}
    public void setMovementParams(float velX,float velY,float tiemp,float dir)
    {
        velocityX = 860f*Time.deltaTime;
        time = tiemp;
        direction = dir;

        velocityY = 0.5f;
        startMovement = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (isBullet)
        {

            /**/
            TankController tc = other.GetComponent<TankController>();
            if (!tc) { return; }
            if (tc.id == this.parentId) { return; }
            tc.health -= 10;
            tc.healthBar.value = tc.health/100f;
            Network.DamagePlayer(tc.health, tc.id);
            TankController tank = GameObject.FindGameObjectWithTag("Player").GetComponent<TankController>();
            Network.sendRemoveBullet(id.ToString(), tank.id);
            tank.RemoveBullet(id);



        }
    }
    void TimeOut()
    {
        if (isBullet)
        {
            TankController tank = GameObject.FindGameObjectWithTag("Player").GetComponent<TankController>();
            Network.sendRemoveBullet(id.ToString(), tank.id);
            tank.RemoveBullet(id);

        }
    }
}
