using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetChildCaja : MonoBehaviour
{
    public bool pruebas;
    public GameObject cajaDesactivarActivar;
    void Update()
    {
        if (pruebas){
            DesactivarCaja();
        } else {
            ActivarCaja();
        }
    }
    public void DesactivarCaja(){
        cajaDesactivarActivar.SetActive(false);
    }
    public void ActivarCaja(){
        cajaDesactivarActivar.SetActive(true);
    }
}
