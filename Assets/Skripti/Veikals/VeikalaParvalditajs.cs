using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class VeikalaParvalditajs : MonoBehaviour
{
    [SerializeField] private List<PrecuSaraksts> precuSaraksts;
    [SerializeField] private PrecesVieta[] precesVieta;
    [SerializeField] private GameObject veikalaCanvas;
    [SerializeField] private SpeletajaProgress speletajaProgress;
    [SerializeField] private AkvarijaParvaldnieks akvarijaParvaldnieks;

    private void Start()
    {
        pievienotVeikalam();
    }

    public void pievienotVeikalam()
    {
        for (int i = 0; i < precuSaraksts.Count && i < precesVieta.Length; i++)
        {
            PrecuSaraksts veikalaPrece = precuSaraksts[i];
            precesVieta[i].Uzstadit(veikalaPrece.ZivsSO, veikalaPrece.cena);
            precesVieta[i].gameObject.SetActive(true);
        }

        for (int i = precuSaraksts.Count; i < precesVieta.Length; i++)
        {
            precesVieta[i].gameObject.SetActive(false);
        }
    }

    public void meginatPirkt(ZivsSO zivsSO, int cena){
        if(zivsSO != null && speletajaProgress.monetas >= cena) 
        {
            speletajaProgress.monetas -= cena;
            speletajaProgress.monetuSkaitsTMP.text = "Monētas: " + speletajaProgress.monetas.ToString();

            // Ielikt nopirkto zivi akvārijā
            if (akvarijaParvaldnieks != null)
            {
                akvarijaParvaldnieks.IeliktZivi(zivsSO);
            }
            else
            {
                Debug.LogWarning("VeikalaParvalditajs: AkvarijaParvaldnieks nav piesaistīts!");
            }
        }
    }
}

[System.Serializable]
public class PrecuSaraksts
{
    public ZivsSO ZivsSO;
    public int cena;
}