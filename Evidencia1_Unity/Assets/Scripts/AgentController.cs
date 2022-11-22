// TC2008B. Sistemas Multiagentes y Gráficas Computacionales
// C# client to interact with Python. Based on the code provided by Sergio Ruiz.
// Octavio Navarro. October 2021

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class WalleData
{
    public string id;
    public float x, y, z;
    public bool box;

    public WalleData(string id, float x, float y, float z, bool box)
    {
        this.id = id;
        this.x = x;
        this.y = y;
        this.z = z;
        this.box = box;
    }
}

[Serializable]

public class WalleListData
{
    public List<WalleData> positions;

    public WalleListData() => this.positions = new List<WalleData>();
}


[Serializable]
public class CajasData
{
    public string id;
    public float x, y, z;
    public bool recoge;

    public CajasData(string id, float x, float y, float z, bool recoge)
    {
        this.id = id;
        this.x = x;
        this.y = y;
        this.z = z;
        this.recoge = recoge;
    }
}

[Serializable]

public class CajasListData
{
    public List<CajasData> positions;

    public CajasListData() => this.positions = new List<CajasData>();
}

[Serializable]
public class RepisaData
{
    public string id;
    public float x, y, z;
    public int numero;

    public RepisaData(string id, float x, float y, float z, int numero)
    {
        this.id = id;
        this.x = x;
        this.y = y;
        this.z = z;
        this.numero = numero;
    }
}

[Serializable]

public class RepisaListData
{
    public List<RepisaData> positions;

    public RepisaListData() => this.positions = new List<RepisaData>();
}




public class AgentController : MonoBehaviour
{
    // private string url = "https://agents.us-south.cf.appdomain.cloud/";
    string serverUrl = "http://localhost:8521";
    string getWalleEndpoint = "/getWalle";
    string getCajaEndpoint = "/getCaja";
    string getRepisasEndpoint = "/getRepisas";
    string sendConfigEndpoint = "/init";
    string updateEndpoint = "/update";
    WalleListData walleData;
    CajasListData cajasData;
    RepisaListData repisasData;
    Dictionary<string, GameObject> robots;
    Dictionary<string, GameObject> caja;
    Dictionary<string, GameObject> repisa;
    Dictionary<string, Vector3> prevPositions, currPositions;

    bool updated = false, started = false;
    bool iniciaCaja = false, iniciaRepisa = false;

    public GameObject cajasPrefab, wallePrefab, repisasPrefab;
    public int NCaja, width, height;
    public float timeToUpdate;
    private float timer, dt;

    void Start()
    {
        walleData = new WalleListData();
        cajasData = new CajasListData();
        repisasData = new RepisaListData();

        prevPositions = new Dictionary<string, Vector3>();
        currPositions = new Dictionary<string, Vector3>();

        robots = new Dictionary<string, GameObject>();
        caja = new Dictionary<string, GameObject>();
        repisa = new Dictionary<string, GameObject>();

        timer = timeToUpdate;

        StartCoroutine(SendConfiguration());
    }

    IEnumerator SendConfiguration()
    {
        WWWForm form = new WWWForm();

        form.AddField("NCaja", NCaja.ToString());
        form.AddField("width", width.ToString());
        form.AddField("height", height.ToString());

        UnityWebRequest www = UnityWebRequest.Post(serverUrl + sendConfigEndpoint, form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Configuration upload complete!");
            Debug.Log("Getting Agents positions");
            StartCoroutine(GetWalleData());
            StartCoroutine(GetCajaData());
            StartCoroutine(GetRepisasData());
        }
    }


    IEnumerator GetWalleData() 
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getWalleEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            walleData = JsonUtility.FromJson<WalleListData>(www.downloadHandler.text);

            foreach(WalleData walleRobot in walleData.positions)
            {
                Vector3 newAgentPosition = new Vector3(walleRobot.x, walleRobot.y, walleRobot.z);

                    if(!started)
                    {
                        prevPositions[walleRobot.id] = newAgentPosition;
                        robots[walleRobot.id] = Instantiate(wallePrefab, newAgentPosition, Quaternion.identity);
                    }
                    else
                    {
                        Vector3 currentPosition = new Vector3();
                        if(currPositions.TryGetValue(walleRobot.id, out currentPosition))
                            prevPositions[walleRobot.id] = currentPosition;
                        currPositions[walleRobot.id] = newAgentPosition;
                    }
            }

            updated = true;
            if(!started) started = true;
        }
    }

    IEnumerator GetCajaData() 
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getCajaEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            cajasData = JsonUtility.FromJson<CajasListData>(www.downloadHandler.text);

            Debug.Log(cajasData.positions);

            foreach(CajasData cajaRoja in cajasData.positions)
            {
                if (!iniciaCaja)
                {
                    Vector3 cajaPos = new Vector3(cajaRoja.x, cajaRoja.y, cajaRoja.z);
                    caja[cajaRoja.id] = Instantiate(cajasPrefab, cajaPos, Quaternion.identity);
                }
                else
                {
                    if(cajaRoja.recoge){
                        caja[cajaRoja.id].SetActive(false);
                    }
                }
            }
            if (!iniciaCaja) iniciaCaja = true;
        }
    }


    IEnumerator GetRepisasData() 
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getRepisasEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            repisasData = JsonUtility.FromJson<RepisaListData>(www.downloadHandler.text);

            Debug.Log(repisasData.positions);

            foreach(RepisaData rep in repisasData.positions)
            {
                Vector3 repisaPos = new Vector3(rep.x, rep.y, rep.z);
                repisa[rep.id] = Instantiate(repisasPrefab, new Vector3(rep.x, rep.y, rep.z), Quaternion.identity);
            }

            if (!iniciaRepisa) iniciaRepisa = true;
        }
    }


    private void Update() 
    {
        if(timer < 0)
        {
            timer = timeToUpdate;
            updated = false;
            StartCoroutine(UpdateSimulation());
        }

        if (updated)
        {
            timer -= Time.deltaTime;
            dt = 1.0f - (timer / timeToUpdate);

            foreach(var walle in currPositions)
            {
                Vector3 currentPosition = walle.Value;
                Vector3 previousPosition = prevPositions[walle.Key];

                Vector3 interpolated = Vector3.Lerp(previousPosition, currentPosition, dt);
                Vector3 direction = currentPosition - interpolated;

                robots[walle.Key].transform.localPosition = interpolated;
                if(direction != Vector3.zero) robots[walle.Key].transform.rotation = Quaternion.LookRotation(direction);
            }

            // float t = (timer / timeToUpdate);
            // dt = t * t * ( 3f - 2f*t);
        }
    }
 
    IEnumerator UpdateSimulation()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + updateEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            StartCoroutine(GetWalleData());
            StartCoroutine(GetCajaData());
            StartCoroutine(GetRepisasData());
        }
    }

    

    
}
