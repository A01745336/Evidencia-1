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
    public bool tieneCaja;

    public WalleData(string id, float x, float y, float z, bool tieneCaja)
    {
        this.id = id;
        this.x = x;
        this.y = y;
        this.z = z;
        this.tieneCaja = tieneCaja;
    }
}

[Serializable]

public class AgentsData
{
    public List<WalleData> positions;

    public AgentsData() => this.positions = new List<WalleData>();
}

[Serializable]
public class CajasData
{
    public string id;
    public float x, y, z;
    public bool desatcivarCaja;

    public CajasData(string id, float x, float y, float z, bool desactivarCaja)
    {
        this.id = id;
        this.x = x;
        this.y = y;
        this.z = z;
        this.desatcivarCaja = desactivarCaja;
    }
}

[Serializable]

public class CajaData
{
    public List<CajasData> positions;

    public CajaData() => this.positions = new List<CajasData>();
}

[Serializable]
public class RepisasData
{
    public string id;
    public float x, y, z;
    public int valor;

    public RepisasData(string id, float x, float y, float z, int valor)
    {
        this.id = id;
        this.x = x;
        this.y = y;
        this.z = z;
        this.valor = valor;
    }
}

[Serializable]

public class RepisaData
{
    public List<RepisasData> positions;

    public RepisaData() => this.positions = new List<RepisasData>();
}


public class AgentController : MonoBehaviour
{
    string serverUrl = "http://localhost:8585";
    string getWalleEndpoint = "/getWalle";
    string getCajasEndpoint = "/getCajas";
    string getRepisasEndpoint = "/getRepisas";
    string sendConfigEndpoint = "/init";
    string updateEndpoint = "/update";
    AgentsData wallesData;
    CajaData cajasData;
    RepisaData repisasData;
    Dictionary<string, GameObject> walles;
    Dictionary<string, GameObject> cajas;
    Dictionary<string, GameObject> repisas;
    Dictionary<string, Vector3> prevPositions, currPositions;

    bool updated = false, started = false, startedCaja = false, updatedCaja = false;

    public GameObject wallePrefab, cajaPrefab, repisaPrefab, floor;
    public int NAgents, width, height;
    public float timeToUpdate = 5.0f;
    private float timer, dt;

    void Start()
    {
        wallesData = new AgentsData();
        cajasData = new CajaData();
        repisasData = new RepisaData();

        prevPositions = new Dictionary<string, Vector3>();
        currPositions = new Dictionary<string, Vector3>();

        walles = new Dictionary<string, GameObject>();
        cajas = new Dictionary<string, GameObject>();
        repisas = new Dictionary<string, GameObject>();

        floor.transform.localScale = new Vector3((float)(width + 1) / 10, 1, (float)(height + 1) / 10);
        floor.transform.localPosition = new Vector3((float)width/2-0.5f, 0.1f, (float)height/2-0.5f);

        timer = timeToUpdate;

        StartCoroutine(SendConfiguration());
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

            foreach(var agent in currPositions)
            {
                Vector3 currentPosition = agent.Value;
                Vector3 previousPosition = prevPositions[agent.Key];

                Vector3 interpolated = Vector3.Lerp(previousPosition, currentPosition, dt);
                Vector3 direction = currentPosition - interpolated;

                walles[agent.Key].transform.localPosition = interpolated;
                if(direction != Vector3.zero) walles[agent.Key].transform.rotation = Quaternion.LookRotation(direction);
            }
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
            StartCoroutine(GetWallesData());
            StartCoroutine(GetCajasData());
            StartCoroutine(GetRepisasData());
        }
    }

    IEnumerator SendConfiguration()
    {
        WWWForm form = new WWWForm();

        form.AddField("NAgents", NAgents.ToString());
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
            StartCoroutine(GetWallesData());
            StartCoroutine(GetCajasData());
            StartCoroutine(GetRepisasData());
        }
    }

    IEnumerator GetWallesData()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getWalleEndpoint);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else
        {
            wallesData = JsonUtility.FromJson<AgentsData>(www.downloadHandler.text);

            foreach(WalleData agent in wallesData.positions)
            {
                Vector3 newAgentPosition = new Vector3(agent.x, agent.y, agent.z);

                    if(!started)
                    {
                        prevPositions[agent.id] = newAgentPosition;
                        walles[agent.id] = Instantiate(wallePrefab, newAgentPosition, Quaternion.identity);
                    }
                    else
                    {
                        if (agent.tieneCaja){
                            walles[agent.id].GetComponent<GetChild>().RecojerCaja();
                        } else {
                            walles[agent.id].GetComponent<GetChild>().SinCaja();
                        }
                        Vector3 currentPosition = new Vector3();
                        if(currPositions.TryGetValue(agent.id, out currentPosition))
                            prevPositions[agent.id] = currentPosition;
                        currPositions[agent.id] = newAgentPosition;
                    }
            }

            updated = true;
            if(!started) started = true;
        }
    }


IEnumerator GetCajasData()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getCajasEndpoint);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else
        {
            cajasData = JsonUtility.FromJson<CajaData>(www.downloadHandler.text);

            foreach(CajasData caja in cajasData.positions)
            {
                if (!startedCaja)
                {
                    Vector3 boxPosition = new Vector3(caja.x, caja.y, caja.z);
                    cajas[caja.id] = Instantiate(cajaPrefab, new Vector3(caja.x, caja.y, caja.z), Quaternion.identity);
                }
                else
                {
                    if (caja.desatcivarCaja){
                        cajas[caja.id].GetComponent<GetChildCaja>().DesactivarCaja();
                    } else {
                        cajas[caja.id].GetComponent<GetChildCaja>().ActivarCaja();
                    }
                }
            }
            if (!startedCaja) startedCaja = true;
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
            repisasData = JsonUtility.FromJson<RepisaData>(www.downloadHandler.text);

            Debug.Log(repisasData.positions);

            foreach(RepisasData obstacle in repisasData.positions)
            {
                Instantiate(repisaPrefab, new Vector3(obstacle.x, obstacle.y, obstacle.z), Quaternion.identity);
            }
        }
    }
}
