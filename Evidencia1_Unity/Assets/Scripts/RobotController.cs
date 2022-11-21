using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;


/* ########################## ROBOTS ############################# */


// Asigna posiciones de los robots

[Serializable]
public class Walle
{
    public string walleId;
    public float x, y, z;

    public Walle(string walleId, float x, float y, float z)
    {
        this.walleId = walleId;
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

/*----------------------*/


// Crea a los robots en su posicion respectiva

[Serializable]
public class WalleMov
{
    public List<Walle> position;

    public WalleMov() => this.position = new List<Walle>();
}




/* ############################ CAJAS ############################# */


// Asigna posiciones de las cajas

[Serializable]
public class Cajas
{
    public string cajaId;
    public float x, y, z;

    public Cajas(string cajaId, float x, float y, float z)
    {
        this.cajaId = cajaId;
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

/*-----------------------*/

// Crea las cajas en su posicion respectiva

[Serializable]
public class CajasR
{
    public List<Cajas> position;

    public CajasR() => this.position = new List<Cajas>();
}

/* ########################## REPISAS ############################# */


// Asigna posiciones de las repisas

[Serializable]
public class Repisas
{
    public string repisaId;
    public float x, y, z;

    public Repisas(string repisaId, float x, float y, float z)
    {
        this.repisaId = repisaId;
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

/*----------------------*/


// Crea a las repisas en su posicion respectiva

[Serializable]
public class RepisaR
{
    public List<Repisas> position;

    public RepisaR() => this.position = new List<Repisas>();
}




/* ########################### CONTROLADOR ############################## */

public class RobotController : MonoBehaviour
{

    // Puerto donde corre el servidor
    private string uri = "http://localhost:8521";


    // Para el robot

    WalleMov walleMov;
    Dictionary<string, Vector3> prevPos, currentPos; // Actualizar posicion robots

    public GameObject walleP;
    Dictionary<string, GameObject> walle;


    // Para las cajas

    CajasR cajasR;
    Dictionary<string, Vector3> prevPosC, currentPosC; // Actualizar posicion cajas (al momento de que las recogen)

    public GameObject cajasP;
    Dictionary<string, GameObject> cajas;


    // Para las repisas

    RepisaR repisaR;
    Dictionary<string, Vector3> prevPosR, currentPosR; // Actualizar posicion robots

    public GameObject repisaP;
    Dictionary<string, GameObject> repisa;




    public float time = 2.0f; // Tiempo de actualizacion
    private float distance, tiempo; // Para calculos
    private bool comienza = false, updated = false;



    // Variables que deben inicializarse
    void Start()
    {

        // Inicializar robot

        walleMov = new WalleMov();
        walle = new Dictionary<string, GameObject>();
        prevPos = new Dictionary<string, Vector3>();
        currentPos = new Dictionary<string, Vector3>();


        // Inicializar cajas

        cajasR = new CajasR();
        cajas = new Dictionary<string, GameObject>();
        prevPosC = new Dictionary<string, Vector3>();
        currentPosC = new Dictionary<string, Vector3>();

        // Inicializar repisas


        repisaR = new RepisaR();
        repisa = new Dictionary<string, GameObject>();


        tiempo = time;

        StartCoroutine(WalleInicia());
    }


    IEnumerator WalleInicia()
    {
        using (UnityWebRequest webReq = UnityWebRequest.Get($"{uri}"))
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

                // ROBOT

                walleMov = JsonUtility.FromJson<WalleMov>(webReq.downloadHandler.text);
                foreach (Walle robot in walleMov.position)
                {
                    Vector3 walleNuevaPos = new Vector3(robot.x, robot.y, robot.z);

                    if(!comienza)
                    {
                        prevPos[robot.walleId] = walleNuevaPos;
                        walle[robot.walleId] = Instantiate(walleP, walleNuevaPos, Quaternion.identity);
                    }
                    else
                    {
                        Vector3 currentPosition = new Vector3();
                        if(currentPos.TryGetValue(robot.walleId, out currentPosition))
                            prevPos[robot.walleId] = currentPosition;
                        currentPos[robot.walleId] = walleNuevaPos;
                    }

                }
                
                
                // CAJAS
                
                cajasR = JsonUtility.FromJson<CajasR>(webReq.downloadHandler.text);
                foreach(Cajas caja in cajasR.position)
                {

                }

                updated = true;
                if(!comienza) comienza = true;
                
            }
        }
    }

        // Update is called once per frame
    void Update()
    {
        if(updated)
        {
            tiempo -= Time.deltaTime;
            distance = 1.0f - (tiempo / time);
        }

        if(tiempo < 0)
        {
            tiempo = time; 
            updated = false;
            StartCoroutine(WalleInicia());  
        }

        foreach(var robot in currentPos)
        {
            Vector3 cPosition = robot.Value;
            Vector3 pPosition = prevPos[robot.Key];

            Vector3 interpolated = Vector3.Lerp(pPosition, cPosition, distance);
            Vector3 direction = cPosition - pPosition;

            walle[robot.Key].transform.localPosition = interpolated;
            walle[robot.Key].transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}