using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// DEBUG: Atiestatīt VISU (zivis, soļus, monētas). Pievieno Button.
/// </summary>
public class AtiestatitProgressuPoga : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            var akvarium = FindFirstObjectByType<AkvarijaParvaldnieks>();
            if (akvarium != null) akvarium.DzestVisasZivis();
            DatuBaze.Instance?.AtiestatitVisu();
            
            var progress = FindFirstObjectByType<SpeletajaProgress>();
            if (progress != null)
            {
                progress.soli = 0;
                progress.monetas = 0;
                if (progress.soluSkaitsTMP != null) progress.soluSkaitsTMP.text = "Soli: 0";
                if (progress.monetuSkaitsTMP != null) progress.monetuSkaitsTMP.text = "Monētas: 0";
            }
            
            Debug.Log("DEBUG: Viss atiestatīts!");
        });
    }
}
