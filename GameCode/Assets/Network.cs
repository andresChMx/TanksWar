using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySocketIO;
using UnitySocketIO.Events;
using System.Runtime.InteropServices;
using System.Text;
public class Network : MonoBehaviour {
    #region Singleton
    static public Network instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    [DllImport("__Internal")]
    private static extern void BrowserNotification(string message);

    public static  SocketIOController io;

    public static List<TankController> tankInstances;
    public GameObject prefTank;


    static public TankController player=null;
    public ChatManager chat;
    PlayerCamera playerCamera;

    static bool readyToUpdate = false;
    
    // Use this for initialization
    void Start() {
        tankInstances = new List<TankController>();
        io = GetComponent<SocketIOController>();
        playerCamera = Camera.main.GetComponent<PlayerCamera>();
        chat = GameObject.FindGameObjectWithTag("Chat").GetComponent<ChatManager>();
        io.On("connect", (SocketIOEvent e) => {

        });
        io.On("send-notification", (SocketIOEvent e) => {
            Valor nombre = JsonUtility.FromJson<Valor>(e.data);
            if (Application.platform == RuntimePlatform.WebGLPlayer) { BrowserNotification((nombre.val + " se acaba de conectar")); };
        });
        io.On("player-disconnected", (SocketIOEvent e) =>
        {
            DeletePlayer(e.data);
        });
        io.On("spawn", (SocketIOEvent e) => {
            Login aux= JsonUtility.FromJson<Login>(e.data);
            Spawn(aux, false);
        });
        io.On("spawn-me", (SocketIOEvent e) => {
            Login aux = JsonUtility.FromJson<Login>(e.data);
            Spawn(aux, true);
        });
        io.On("player-move",(SocketIOEvent e)=>{
            EnemyMoved(e.data);
        });
        /**/
        io.On("spawn-bullet",(SocketIOEvent e)=>
        {
            SpawnBullet(e.data);
        });
        io.On("enemy-bullets-update",(SocketIOEvent e)=> {
            EnemyBulletsMoved(e.data);
        });
        /**/
        io.On("take-message", (SocketIOEvent e) => 
        {
            Message msg=JsonUtility.FromJson<Message>(e.data);
            chat.AddMessage(msg.nombre,msg.message,new ColorRGB(msg.r,msg.g,msg.b));
        });
        io.On("set-new-color",(SocketIOEvent e)=> {
            ColourEnemy(e.data);
        });
        io.On("remove-enemy-bullet",(SocketIOEvent e)=> {
            EnemyBulletRemoved(e.data);
        });
        /*damage*/
        io.On("enemy-damage",(SocketIOEvent e)=> {
            EnemyHealthChange(e.data);
        });
        io.On("someone-death", (SocketIOEvent e) => {

            SomeoneDeath(e.data);
        });
        StartCoroutine("asa");
        Invoke("setReady",1f);
    }
    IEnumerator asa()
    {
        yield return new WaitForSeconds(1f);
        io.Connect();
    }
    void setReady()
    {
        readyToUpdate = true;
    }
    void Spawn(Login info,bool isMe)
    {   
        TankController go = Instantiate(prefTank).GetComponent<TankController>();
        go.id = info.num.ToString();
        go.alias = info.name;
        go.SetParamsAfterConnection();
        go.SetColorMaterial(info.r, info.g, info.b);
        if (isMe) {
            go.gameObject.layer = 9;
            go.tag = "Player";
            go.isPlayer = true;
            player = go;
            playerCamera.target = go.transform;
            playerCamera.StartFollowing();

            chat.msgColor = go.tankColor;
        }
        if (info.isDeath) { go.gameObject.SetActive(false); }

        tankInstances.Add(go);

    }
    void DeletePlayer(string data)
    {

        Valor code = JsonUtility.FromJson<Valor>(data);
        for (int i = 0; i < tankInstances.Count; i++)
        {
            if (tankInstances[i].id == code.val)
            {

                Destroy(tankInstances[i].gameObject);
                tankInstances.Remove(tankInstances[i]);
                break;
            }
        }
    }
    static public void DamagePlayer(int health,string id)
    {
        Value2 heal = new Value2();
        heal.val1 = health.ToString();
        heal.val2 = id;
        string data = JsonUtility.ToJson(heal);
        io.Emit("damage",data);
    }
    static public void EnemyHealthChange(string data)
    {
        Value2 info = JsonUtility.FromJson<Value2>(data);
        for(int i = 0; i < tankInstances.Count; i++)
        {
            if (tankInstances[i].id == info.val2)
            {
                tankInstances[i].health = int.Parse(info.val1);
                tankInstances[i].healthBar.value = tankInstances[i].health/100f;
                if (tankInstances[i].health <= 0)
                {
                    if (tankInstances[i] == player)
                    {
                        tankInstances[i].Death();
                    }
                }
                break;
            }
        }
    }
    public static void SendSomeoneDeath(string id)
    {
        Valor valor = new Valor();
        valor.val = id;
        io.Emit("someone-death",JsonUtility.ToJson(valor));

    }
    public void SomeoneDeath(string data)
    {
        Debug.Log("alguien musriosdsdfad");
        Valor valor = JsonUtility.FromJson<Valor>(data);
        for(int i = 0; i < tankInstances.Count; i++)
        {
            if (tankInstances[i].id == valor.val)
            {

                tankInstances[i].DeathFbx();
            }
        }
    }
    static public void Moved(Vector3 position,Vector3 rotation,float canonAng)
    {
        if (!readyToUpdate) { return; }
        PlayerInfo info = new PlayerInfo(position.x, position.y, position.z, player.id, rotation.x, rotation.y, rotation.z, canonAng);
        string data = JsonUtility.ToJson(info);
        io.Emit("move", data);
    }
    void EnemyMoved(string info)
    {
        if (!readyToUpdate) { return; }
        PlayerInfo infoEnemy = JsonUtility.FromJson<PlayerInfo>(info);
        if (infoEnemy.id == player.id) { Debug.Log("EERRORRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR"); }
        for(int i = 0; i < tankInstances.Count; i++)
        {
            if (tankInstances[i].id == infoEnemy.id)
            {
                Vector3 newPos = new Vector3(infoEnemy.x, infoEnemy.y, infoEnemy.z);
                Vector3 newRot = new Vector3(infoEnemy.angX, infoEnemy.angY, infoEnemy.angZ);
                Vector3 canonRot = new Vector3(Mathf.Cos(infoEnemy.canonAngle/180*Mathf.PI),0,Mathf.Sin(infoEnemy.canonAngle/180*Mathf.PI));
                tankInstances[i].SetPosition(newPos);
                tankInstances[i].SetRotation(newRot);
                tankInstances[i].SetCanonAngleAsEnemy(canonRot);
                //Debug.Log("se movio al enemigoo: " + newPos);
                break;
            }
        }
    }
    void ColourEnemy(string info)
    {
        TankColor col = JsonUtility.FromJson<TankColor>(info);
        for(int i = 0; i < tankInstances.Count; i++)
        {

            if (tankInstances[i].id == col.id)
            {
                tankInstances[i].SetColorMaterial(col.r,col.g,col.b);
            }
        }
    }
    public static void SendCreateBullet(Vector3 position,Vector3 rotation,string id)
    {
        EntityBasicInfo bulletInfo = new EntityBasicInfo(position.x,position.y,position.z,id,rotation.x,rotation.y,rotation.z);
        io.Emit("new-bullet",JsonUtility.ToJson(bulletInfo));
    }
    public static void SpawnBullet(string data)
    {
        EntityBasicInfo bulletInfo = JsonUtility.FromJson<EntityBasicInfo>(data);
        Vector3 pos = new Vector3(bulletInfo.x,bulletInfo.y,bulletInfo.z);
        Vector3 rot = new Vector3(bulletInfo.angX,bulletInfo.angY,bulletInfo.angZ);
        for (int i=0;i<tankInstances.Count;i++)
        {
            if (tankInstances[i].id == bulletInfo.id)
            {
                tankInstances[i].CreateBulletAsEnemy(pos,rot);
                break;
            }
        }
         
    }
    public static void BulletsMoved(string data)
    {
        io.Emit("bullets-update",data);

    }
    public static void EnemyBulletsMoved(string data)
    {
        EntityBasicInfo[] bulletInfo = JsonHelper.FromJson<EntityBasicInfo>(data);
        for (int i = 0; i < tankInstances.Count; i++)
        {
            if (tankInstances[i].id == bulletInfo[0].id)
            {
                for (int j=0;j<bulletInfo.Length;j++)
                {
                    if (tankInstances[i].bullets[j] == null) { Debug.Log("FUERA DE LIMITE ARREGLOOOO UPDATE BULLETS ENEMY");continue; }
                    Vector3 pos = new Vector3(bulletInfo[j].x, bulletInfo[j].y, bulletInfo[j].z);
                    //Vector3 rot = new Vector3(bulletInfo[j].angX, bulletInfo[j].angY, bulletInfo[j].angZ);
                    tankInstances[i].bullets[j].transform.position = pos;
                }

                break;
            }
        }
    }
    public static void sendRemoveBullet(string idBullet,string idPlayer)
    {
        string data = JsonUtility.ToJson(new BulletOwner(idBullet,idPlayer));
        io.Emit("remove-bullet",data);
    }
    public static void EnemyBulletRemoved(string data)
    {
        BulletOwner bulletOwner = JsonUtility.FromJson<BulletOwner>(data);
        for (int i = 0; i < tankInstances.Count; i++)
        {
            if (tankInstances[i].id == bulletOwner.idPlayer)
            {
                tankInstances[i].RemoveBullet(int.Parse(bulletOwner.idBullet));
            }
        }
    }
    public static void NewMessage(string message,ColorRGB color)
    {
        Message msg = new Message();
        msg.nombre = player.alias;
        msg.message = message;
        msg.r = color.r;
        msg.g = color.g;
        msg.b = color.b;
        io.Emit("chat-message", JsonUtility.ToJson(msg));
    }

}
[System.Serializable]
struct EntityBasicInfo
{
    public float x;
    public float y;
    public float z;
    public string id;
    public float angX;
    public float angY;
    public float angZ;
    public EntityBasicInfo(float x, float y, float z, string id, float aX, float aY, float aZ)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.id = id;
        this.angX = aX;
        this.angY = aY;
        this.angZ = aZ;
    }
}

struct PlayerInfo
{
    public float x;
    public float y;
    public float z;
    public string id;
    public float angX;
    public float angY;
    public float angZ;
    public float canonAngle;


    public PlayerInfo(float x, float y, float z, string id, float aX, float aY, float aZ,float canAng)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.id = id;
        this.angX = aX;
        this.angY = aY;
        this.angZ = aZ;
        this.canonAngle=canAng;
}
}
struct BulletOwner
{
    public string idBullet;
    public string idPlayer;
    public BulletOwner(string id1,string id2)
    {
        idBullet = id1;
        idPlayer = id2;
    }
}
struct Message
{

    public string nombre;
    public string message;
    public int r;
    public int g;
    public int b;
}
struct TankColor
{
    public string id;
    public int r;
    public int g;
    public int b;
    public TankColor(string id, int r,int g,int b)
    {
        this.id = id;
        this.r = r;
        this.g = g;
        this.b = b;
    }
}
struct Login
{
    public int num;
    public string name;
    public int r;
    public int g;
    public int b;
    public bool isDeath;

}
struct Valor
{
    public string val;
}
struct Value2
{
    public string val1;
    public string val2;
}
