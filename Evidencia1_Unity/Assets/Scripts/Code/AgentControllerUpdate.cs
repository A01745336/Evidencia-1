using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;


//[Serializable]
public class Walle
{
    public string walleId;
    public float x, y, z;

    public Walle(string walle, float x, float y, float z)
    {
        this.walleId = walleId;
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

public class Cajas
{
    public string cajaId;
    public float x, y, z;

    public Cajas(string walle, float x, float y, float z)
    {
        this.cajaId = cajaId;
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

public class WalleMov
{
    public List<Walle> position;

    public WalleMov() => this.position = new List<Walle>();
}


public class CajasR
{
    public List<Cajas> position;

    public CajasR() => this.position = new List<Cajas>();
}

public class AgentControllerUpdate : MonoBehaviour
{

    private string uri = "http://localhost:8585";

    WalleMov walleMov;
    Dictionary<string, Vector3> prevPos, currentPos;

    CajasR cajasR;
    Dictionary<string, Vector3> prevPosC, currentPosC;

    public GameObject walleP;
    Dictionary<string, GameObject> walle;

    public GameObject cajasP;
    Dictionary<string, GameObject> cajas;

    public float time = 2.0f;
    public float distance, tiempo;
    private bool comienza = false, update = false;

    void Start()
    {
        walleMov = new WalleMov();
        cajasR = new CajasR();

        walle = new Dictionary<string, GameObject>();
        cajas = new Dictionary<string, GameObject>();

        prevPos = new Dictionary<string, Vector3>();
        currentPos = new Dictionary<string, Vector3>();


        tiempo = time;

        StartCoroutine(WalleInicia());
    }


    IEnumerator WalleInicia()
    {
        using (UnityWebRequest webReq = UnityWebRequest.Get($"{uri}/init"))
        {
            Debug.Log("Iniciando conexion ...");

            yield return webReq.SendWebRequest();

            if (webReq.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(webReq.error);
            }
            else
            {
                Debug.Log($"Wall-E iniciando con: {webReq.downloadHandler.text}");
                StartCoroutine(WalleRecoge());
            }
        }
    }


    IEnumerator WalleRecoge()
    {
        using (UnityWebRequest webReq = UnityWebRequest.Get(uri))
        {
            yield return webReq.SendWebRequest();

            if (webReq.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(webReq.error);
            }
            else
            {

                Debug.Log(webReq.downloadHandler.text);
                cajasR = JsonUtility.FromJson<CajasR>(webReq.downloadHandler.text);

                foreach(Cajas caja in cajasR.position)
                {

                }


                walleMov = JsonUtility.FromJson<WalleMov>(webReq.downloadHandler.text);

                foreach (Walle wall in walleMov.position)
                {

                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

    }
}