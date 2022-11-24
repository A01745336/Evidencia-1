using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetChild : MonoBehaviour
{
    public GameObject hijoSinCaja;
    public GameObject hijoConCaja;
    // Start is called before the first frame update
    void Start()
    {
        hijoSinCaja = this.gameObject.transform.GetChild(0).gameObject;
        hijoConCaja = this.gameObject.transform.GetChild(1).gameObject;
    }

    public void RecojerCaja()
    {
        hijoSinCaja.SetActive(false);
        hijoConCaja.SetActive(true);
    }
    public void SinCaja()
    {
        hijoSinCaja.SetActive(true);
        hijoConCaja.SetActive(false);
    }
}
