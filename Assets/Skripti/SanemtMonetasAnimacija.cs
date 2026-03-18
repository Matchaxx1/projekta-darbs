using UnityEngine;
using DG.Tweening;

/// <summary>
/// Pārvalda monētas saņemšanas animāciju interfeisā, vizuāli pārvietojot vienu monētu 
/// no sākuma pozīcijas uz šī objekta pozīciju.
/// </summary>
public class SanemtMonetasAnimacija : MonoBehaviour
{
    [Range(0, 2)]
    public float monetasLidojumaLaiks = 0.5f; // Cik ilgi monēta lido līdz galamērķim

    public Ease monetaLidojumsEase = Ease.OutCubic; // DOTween animācijas līkne
    public GameObject monetaPrefabs; // Monētas UI elementa sagatave

    /// <summary>
    /// Sāk vienas monētas lidošanas animāciju no norādītās sākuma vietas uz šī objekta galamērķi.
    /// </summary>
    public void SaktAnimaciju(int skaits, Transform sakumaVieta)
    {
        // Mērķis, uz kuru virzās monēta
        Vector3 beiguPozicija = transform.position;

        // Atrast galveno Canvas, lai monēta tiktu zīmēta pa virsu pilnīgi visam
        Canvas galvenaisCanvas = GetComponentInParent<Canvas>();
        Transform animacijasTevecs = galvenaisCanvas != null ? galvenaisCanvas.transform : transform;

        // Izveido jaunu monētu
        GameObject moneta = Instantiate(monetaPrefabs, animacijasTevecs);
        moneta.transform.SetAsLastSibling(); 
        moneta.transform.localScale = Vector3.one; 
        
        moneta.transform.position = sakumaVieta.position;

        // Izmanto DOTween, lai pārvietotu monētu, un izdzēstu to animācijas galā
        moneta.transform.DOMove(beiguPozicija, monetasLidojumaLaiks)
            .SetEase(monetaLidojumsEase)
            .OnComplete(() => Destroy(moneta))    
            .Play();
    }
}
