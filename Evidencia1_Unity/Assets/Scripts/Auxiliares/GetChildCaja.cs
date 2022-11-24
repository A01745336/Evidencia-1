using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetChildCaja : MonoBehaviour
{
    public GameObject cajaActivar;
    public GameObject cajaDesactivar;
    void Start(){
        cajaActivar= this.gameObject.transform.GetChild(0).gameObject;
        cajaDesactivar = this.gameObject.transform.GetChild(1).gameObject;
    }
    public void DesactivarCaja(){
        cajaActivar.SetActive(false);
        cajaDesactivar.SetActive(true);
    }
    public void ActivarCaja(){
        cajaActivar.SetActive(true);
        cajaDesactivar.SetActive(false);
    }
}
