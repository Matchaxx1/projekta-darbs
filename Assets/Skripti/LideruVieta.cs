using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LideruDati
{
    public string Lietotajvards { get; set; }
    public int Soli { get; set; }
}

public class LideruVieta : MonoBehaviour
{
    private TextMeshProUGUI lietotajvardsTeksts;
    private TextMeshProUGUI soliTeksts;

    private void Awake()
    {
        lietotajvardsTeksts = transform.Find("Lietotajvards").GetComponent<TextMeshProUGUI>();
        soliTeksts = transform.Find("Soli").GetComponent<TextMeshProUGUI>();
    }

    public void IestatitLietotajaDatus(LideruDati userData)
    {
        if (lietotajvardsTeksts == null) Awake();
        
        lietotajvardsTeksts.text = userData.Lietotajvards;
        soliTeksts.text = userData.Soli.ToString() + "";
    }

}
