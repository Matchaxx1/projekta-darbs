using UnityEngine;
using UnityEngine.UI;

public class AtiestatitProgressuPoga : MonoBehaviour
{
    // Atsauce uz izvēles UI paneli, kas tiek parādīts pēc atiestatīšanas
    public GameObject izveleUI;

    /// <summary>
    /// Inicializācijas brīdī pievieno klikšķa notikuma apstrādātāju pogai.
    /// Kad poga tiek nospiesta, tiek izpildīta pilnīga progresa atiestatīšana.
    /// </summary>
    void Start()
    {
        // Pievieno klikšķa notikuma apstrādātāju šī objekta Button komponentei
        GetComponent<Button>().onClick.AddListener(() =>
        {
            // Dzēš visas zivis no akvārija (vizuāli un no datubāzes)
            var akvarium = FindFirstObjectByType<AkvarijaParvaldnieks>();
            if (akvarium != null) akvarium.DzestVisasZivis();

            // Atiestata visu progresu datubāzē (soļi, monētas, zivis)
            DatuParvaldnieks.Instance?.AtiestatitVisu();

            // Atiestata lietotāja lomu (noņem viesa vai reģistrēta lietotāja statusu)
            LietotajaLoma.AtiestatitLomu();
            
            // Atiestata spēlētāja progresa mainīgos un UI tekstu uz sākotnējām vērtībām
            var progress = FindFirstObjectByType<SpeletajaProgress>();
            if (progress != null)
            {
                progress.soli = 0;
                progress.monetas = 0;
                progress.kopejasMonetas = 0;
                if (progress.soluSkaitsTMP != null) progress.soluSkaitsTMP.text = "Soli: 0";
                if (progress.monetuSkaitsTMP != null) progress.monetuSkaitsTMP.text = "0";
            }

            // Parāda izvēles UI paneli, lai lietotājs varētu atkārtoti izvēlēties lomu
            izveleUI.GetComponent<CanvasGroup>().alpha = 1f;
            izveleUI.GetComponent<CanvasGroup>().blocksRaycasts = true;

            Debug.Log("Viss atiestatīts!");
        });
    }
}
