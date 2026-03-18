using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Datu klase, kas glabā viena lietotāja informāciju līderu tabulai.
/// </summary>
public class LideruDati
{
    public string Lietotajvards { get; set; }
    public int Soli { get; set; }
}

/// <summary>
/// Pārvalda viena lietotāja ieraksta UI līderu tabulā.
/// Atrod un kontrolē teksta elementus vārda un soļu attēlošanai.
/// </summary>
public class LideruVieta : MonoBehaviour
{
    private TextMeshProUGUI lietotajvardsTeksts;
    private TextMeshProUGUI soliTeksts;

    private void Awake()
    {
        // Atrod teksta elementus bērnu objektos pēc to nosaukumiem
        lietotajvardsTeksts = transform.Find("Lietotajvards").GetComponent<TextMeshProUGUI>();
        soliTeksts = transform.Find("Soli").GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// Iestata UI elementu tekstus ar dotajiem lietotāja datiem.
    /// </summary>
    public void IestatitLietotajaDatus(LideruDati userData)
    {
        // Ja komponentes vēl nav atrastas (piemēram, ja tiek izsaukts pirms Awake), mēģina tās atrast
        if (lietotajvardsTeksts == null) Awake();
        
        // Atjaunina teksta laukus
        lietotajvardsTeksts.text = userData.Lietotajvards;
        soliTeksts.text = userData.Soli.ToString() + "";
    }

}
