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
        if(zivsSO == null) return;
        
        // Pārbaudīt limitu SQL datubāzē (max 3 no katra tipa)
        if (!DatuBaze.Instance.VaiVarPirkt(zivsSO.id))
        {
            Debug.Log("Nevar nopirkt vairāk no šī tipa zivis! (Max: 3)");
            return;
        }
        
        if(speletajaProgress.monetas >= cena) 
        {
            speletajaProgress.monetas -= cena;
            speletajaProgress.monetuSkaitsTMP.text = "Monētas: " + speletajaProgress.monetas.ToString();

            if (akvarijaParvaldnieks != null)
            {
                akvarijaParvaldnieks.IeliktZivi(zivsSO);
                // Saglabā pozīcijas un pirkumu datubāzē
                akvarijaParvaldnieks.SaglabatPozicijas();
            }
            
            DatuBaze.Instance.SaglabatProgresu(speletajaProgress.soli, speletajaProgress.monetas);
        }
    }
}

[System.Serializable]
public class PrecuSaraksts
{
    public ZivsSO ZivsSO;
    public int cena;
}