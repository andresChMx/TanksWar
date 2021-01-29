using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TankController : MonoBehaviour {
    public ColorRGB tankColor;
    public string id;
    public string alias;
    public bool isPlayer=false;
    Rigidbody rb;
    public Animator anim;
    public int health=100;
    public bool isDeath=false;
    float speed = 9;
    float angularSpeed = 130;
    Text textNombre;
    public Slider healthBar;
    public GameObject prefBullet;
    public GameObject[]  bullets;
    public int bulletsCounter=0;

    public GameObject canon;
    public GameObject particles;
	// Use this for initialization
	void Start () {

        bullets = new GameObject[10];
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        rb.position = new Vector3(Random.value * 4, Random.value * 4, 0);

        healthBar = GetComponentInChildren<Slider>();
    }
	
	// Update is called once per frame
	void Update () {

        if (!isPlayer) { return; }
        Vector3 movement=new Vector3();
        Vector3 angularMovement = new Vector3();
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        angularMovement = Vector3.up;
        angularMovement *= horizontal*angularSpeed*Time.deltaTime;
        movement = transform.forward;
        movement = movement * vertical*speed*Time.deltaTime;
        rb.position = rb.position + movement;
        rb.rotation = rb.rotation * Quaternion.Euler(angularMovement);
        Network.Moved(transform.position, transform.eulerAngles,-((canon.transform.eulerAngles.y-90)));
        if (bulletsCounter > 0)
        {
            EntityBasicInfo[] arrInfo = new EntityBasicInfo[bulletsCounter];
            Debug.Log(bulletsCounter);
            for(int i = 0; i < bulletsCounter; i++)
            {
                Transform b=bullets[i].transform;
                arrInfo[i] = new EntityBasicInfo(b.position.x,b.position.y,b.position.z,id,0,0,0);
            }
            string data = JsonHelper.ToJson(arrInfo);
            Network.BulletsMoved(data);
        }
        
    }
    public void CreateBullet(Vector3 point)
    {
        float tiempo = 1;
        GameObject go = Instantiate(prefBullet, transform.position+Vector3.up,Quaternion.identity);
        go.layer = 10;

        go.GetComponent<BulletController>().id=bulletsCounter;
        go.GetComponent<BulletController>().parentId = this.id;
        go.GetComponent<BulletController>().isBullet = true;
        bullets[bulletsCounter] = go;
        bulletsCounter++;
        float dist = Vector3.Distance(go.transform.position,point);

        float dx=point.x-go.transform.position.x;
        float dy=point.z-go.transform.position.z;
        float direction = Mathf.Atan2(dy,dx);

        float velX = dist / tiempo;

        float velY =(dist-((0.5f)*(9.81f*tiempo*tiempo)))/tiempo;
        go.GetComponent<BulletController>().setMovementParams(velX,velY,tiempo,direction);
        Network.SendCreateBullet(transform.position,Vector3.zero,this.id);

    }
    public void CreateBulletAsEnemy(Vector3 pos,Vector3 dir)
    {
        GameObject go = Instantiate(prefBullet, pos, Quaternion.identity);
        go.layer = 10;
        go.GetComponent<BulletController>().parentId = this.id;
        bullets[bulletsCounter] = go;
        bulletsCounter++;
    }
    public void RemoveBullet(int id)
    {
        for(int i = 0; i < bulletsCounter; i++)
        {
            if (i == id)
            {
                Destroy(bullets[id]);
                bullets[i] = null;
                for(int k = i; k < bulletsCounter; k++)
                {
                    if ((k + 1) >= bulletsCounter) { bullets[k] = null; }
                    else
                    {
                        bullets[k] = bullets[k + 1];
                        bullets[k].GetComponent<BulletController>().id = k;
                    }

                }
                bulletsCounter--;
                break;
            }
        }
    }

    public void SetPosition(Vector3 pos) {
        if (rb != null)
        {
            rb.position = pos;

        }
    }
    public void SetRotation(Vector3 rot)
    {
        if (rb != null)
        {
            rb.rotation = Quaternion.Euler(rot);
        }
        }
    public void SetParamsAfterConnection()
    {
        textNombre = GetComponentInChildren<Text>();
        textNombre.text = this.alias;
        
    }
    public void SetColorMaterial(int r,int g,int b)
    {
        tankColor = new ColorRGB(r,g,b);
        GetComponentsInChildren<Renderer>()[0].materials[0].color = new Color32((byte)r, (byte)g, (byte)b, 255);
        canon.GetComponent<Renderer>().materials[0].color = new Color32((byte)r, (byte)g, (byte)b, 255);
    }
    public void SetCanonAngle(Vector3 point)
    {
        Vector3 dir = point - transform.position;
        dir.y = 0;
        Quaternion angle = Quaternion.LookRotation(dir);
        canon.transform.rotation = angle;
    }
    public void SetCanonAngleAsEnemy(Vector3 dir)
    {
        canon.transform.rotation = Quaternion.LookRotation(dir);
    }
    public void Death()
    {
        DeathFbx();
        isDeath = true;
        Camera.main.GetComponent<PlayerCamera>().follow = false;
        for(int i = 0; i < Network.tankInstances.Count; i++)
        {
            if (Network.tankInstances[i] != null && Network.tankInstances[i].id!=id)
            {
                //Debug.Log("se tuvo qu estaleser nuevo target");
                Camera.main.GetComponent<PlayerCamera>().target = Network.tankInstances[i].transform;

                break;
            }
        }
        Network.SendSomeoneDeath(this.id);

    }
    public void DeathFbx()
    {
        particles.GetComponent<ParticleSystem>().Play();
        anim.SetTrigger("Death");
    }
    void OnTriggerEnter(Collider other)
    {

        if (false)
        {
            Debug.Log("ME TUVO QUE HACER DANOO");
            BulletController bc = other.GetComponent<BulletController>();
            if(other.CompareTag("Bullet") && bc)
            {

                if (bc.parentId == this.id) { return; }

                this.health -= 10;
                healthBar.value = health;
                Network.DamagePlayer(health, this.id);
                if (health <= 20)
                {
                    Debug.Log("SEBIO HACER MUERTOOO");
                    health = 0;
                    string parentId=other.GetComponent<BulletController>().parentId;
                    for(int i = 0; i < Network.tankInstances.Count; i++)
                    {
                        if (Network.tankInstances[i].id == parentId)
                        {
                            //Camera.main.GetComponent<PlayerCamera>().target=Network.tankInstances[i].transform;

                        }
                    }
                    //gameObject.SetActive(false);
                }
            }

        }
    }

}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}
