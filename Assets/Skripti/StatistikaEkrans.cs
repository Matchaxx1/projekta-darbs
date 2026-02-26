using UnityEngine;
using TMPro;

public class StatistikaEkrans : MonoBehaviour
{
    public TMP_Text soluSkaitsTeksts;
    public TMP_Text monetuSkaitsTeksts;
    public TMP_Text zivjuSkaitsTeksts;
    public AkvarijaParvaldnieks akvarijaParvaldnieks;
    public VeikalaParvalditajs veikalaParvalditajs;
    public SpeletajaProgress speletajaProgress;

    private void OnEnable()
    {
        // Automātiski meklē SpeletajaProgress ja nav piesaistīts
        if (speletajaProgress == null)
            speletajaProgress = FindFirstObjectByType<SpeletajaProgress>();

        AtjaunotUI();
    }

    // Atjauno statistiku katru kadru kamēr panelis ir redzams
    private void Update()
    {
        AtjaunotUI();
    }

    public void AtjaunotUI()
    {
        if (speletajaProgress == null)
        {
            UzstaditTekstu(soluSkaitsTeksts, "Nav datu");
            UzstaditTekstu(monetuSkaitsTeksts, "Nav datu");
            return;
        }

        UzstaditTekstu(soluSkaitsTeksts, speletajaProgress.soli + " soļi");
        UzstaditTekstu(monetuSkaitsTeksts, speletajaProgress.kopejasMonetas + " monētas");
        AtjaunotZivjuSkaituUI();
    }

    private void UzstaditTekstu(TMP_Text teksts, string saturs)
    {
        if (teksts != null) teksts.text = saturs;
    }

    private void AtjaunotZivjuSkaituUI()
    {
        if (zivjuSkaitsTeksts == null) return;

        int skaits = 0;
        if (akvarijaParvaldnieks != null)
            skaits = akvarijaParvaldnieks.IegutZivjuSkaitu();

        int maxs = 0;
        if (veikalaParvalditajs != null)
            maxs = veikalaParvalditajs.MaxKopejaisZivjuSkaits;

        zivjuSkaitsTeksts.text = skaits.ToString("D2") + "/" + maxs + " zivis";
    }

    private void OnApplicationPause(bool pauze)
    {
        if (pauze)
            LogotStatistiku("pauze");
    }

    private void OnApplicationQuit()
    {
        LogotStatistiku("izslēgšana");
    }

    private void LogotStatistiku(string iemesls)
    {
        if (speletajaProgress == null) return;

        int zivjuSkaits = 0;
        if (akvarijaParvaldnieks != null)
            zivjuSkaits = akvarijaParvaldnieks.IegutZivjuSkaitu();

        int maxZivju = 0;
        if (veikalaParvalditajs != null)
            maxZivju = veikalaParvalditajs.MaxKopejaisZivjuSkaits;

        string avots = "SQLite";
        if (LietotajaLoma.IrRegistrets())
            avots = "Firestore";

        Debug.Log("[Statistika | " + iemesls + "] " + avots +
                  " | Soļi: " + speletajaProgress.soli +
                  " | Monētas: " + speletajaProgress.monetas +
                  " | Zivis: " + zivjuSkaits + "/" + maxZivju);
    }
}
