using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// DEBUG: Atiestatīt VISU (zivis, soļus, monētas). Pievieno Button.
/// </summary>

public class AtiestatitProgressuPoga : MonoBehaviour
{
    public GameObject izveleUI;
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            var akvarium = FindFirstObjectByType<AkvarijaParvaldnieks>();
            if (akvarium != null) akvarium.DzestVisasZivis();
            DatuParvaldnieks.Instance?.AtiestatitVisu();
            // Atiestata lietotāja loma arī (noņem viesi/registrets statusu)
            LietotajaLoma.AtiestatitLomu();
            
            var progress = FindFirstObjectByType<SpeletajaProgress>();
            if (progress != null)
            {
                progress.soli = 0;
                progress.monetas = 0;
                progress.kopejasMonetas = 0;
                if (progress.soluSkaitsTMP != null) progress.soluSkaitsTMP.text = "Soli: 0";
                if (progress.monetuSkaitsTMP != null) progress.monetuSkaitsTMP.text = "0";
            }

            izveleUI.GetComponent<CanvasGroup>().alpha = 1f;
            izveleUI.GetComponent<CanvasGroup>().blocksRaycasts = true;

            
            Debug.Log("DEBUG: Viss atiestatīts!");
        });
    }
}
