using UnityEngine;
using TMPro;

public class StatistikaEkrans : MonoBehaviour
{
    // UI teksta lauki statistikas attēlošanai
    public TMP_Text soluSkaitsTeksts;        // Teksta lauks soļu skaita parādīšanai
    public TMP_Text monetuSkaitsTeksts;      // Teksta lauks monētu skaita parādīšanai
    public TMP_Text zivjuSkaitsTeksts;       // Teksta lauks zivju skaita parādīšanai

    // Atsauces uz citiem pārvaldniekiem, no kuriem tiek iegūti dati
    public AkvarijaParvaldnieks akvarijaParvaldnieks;  // Akvārija pārvaldnieks zivju skaita iegūšanai
    public VeikalaParvalditajs veikalaParvalditajs;    // Veikala pārvaldnieks maksimālā zivju skaita iegūšanai
    public SpeletajaProgress speletajaProgress;        // Spēlētāja progresa objekts soļu un monētu datiem

    /// <summary>
    /// Kad panelis kļūst redzams, automātiski meklē SpēlētājaProgress komponentu, ja tā vēl nav piesaistīta, un atjauno UI ar aktuālajiem datiem.
    /// </summary>
    private void OnEnable()
    {
        // Automātiski meklē SpeletajaProgress, ja nav piesaistīts inspektorā
        if (speletajaProgress == null)
            speletajaProgress = FindFirstObjectByType<SpeletajaProgress>();

        AtjaunotUI();
    }

    /// <summary>
    /// Atjauno statistikas UI katru kadru, kamēr panelis ir aktīvs un redzams.
    /// </summary>
    private void Update()
    {
        AtjaunotUI();
    }

    /// <summary>
    /// Attēlo soļu skaitu, monētu skaitu un izsauc zivju skaita atjaunošanu. Ja progresa objekts nav pieejams, parāda paziņojumu "Nav datu".
    /// </summary>
    public void AtjaunotUI()
    {
        // Ja progresa dati nav pieejami, parāda noklusējuma tekstu
        if (speletajaProgress == null)
        {
            UzstaditTekstu(soluSkaitsTeksts, "Nav datu");
            UzstaditTekstu(monetuSkaitsTeksts, "Nav datu");
            return;
        }

        // Atjauno soļu un monētu tekstu ar aktuālajām vērtībām
        UzstaditTekstu(soluSkaitsTeksts, speletajaProgress.soli + " soļi");
        UzstaditTekstu(monetuSkaitsTeksts, speletajaProgress.kopejasMonetas + " monētas");
        AtjaunotZivjuSkaituUI();
    }

    /// <summary>
    /// Droši iestata teksta lauka saturu, pārbaudot, vai elements nav tukšs.
    /// </summary>
    private void UzstaditTekstu(TMP_Text teksts, string saturs)
    {
        if (teksts != null) teksts.text = saturs;
    }

    /// <summary>
    /// Atjauno zivju skaita UI tekstu, parāda pašreizējo un maksimālo zivju skaitu.
    /// </summary>
    private void AtjaunotZivjuSkaituUI()
    {
        if (zivjuSkaitsTeksts == null) return;

        // Iegūst pašreizējo aktīvo zivju skaitu no akvārija pārvaldnieka
        int skaits = 0;
        if (akvarijaParvaldnieks != null)
            skaits = akvarijaParvaldnieks.IegutZivjuSkaitu();

        // Iegūst maksimālo atļauto zivju skaitu no veikala pārvaldnieka
        int maxs = 0;
        if (veikalaParvalditajs != null)
            maxs = veikalaParvalditajs.MaxKopejaisZivjuSkaits;

        // Formatē tekstu ar diviem cipariem pirms slīpsvītras
        zivjuSkaitsTeksts.text = skaits.ToString("D2") + "/" + maxs + " zivis";
    }

    /// <summary>
    /// Unity izsauc šo metodi, kad lietotne tiek pauzēta (piemēram, samazināta fonā).
    /// Saglabā statistikas ierakstu žurnālā.
    /// </summary>
    private void OnApplicationPause(bool pauze)
    {
        if (pauze)
            LogotStatistiku("pauze");
    }

    /// <summary>
    /// Unity izsauc šo metodi, kad lietotne tiek aizvērta.
    /// Saglabā statistikas ierakstu žurnālā.
    /// </summary>
    private void OnApplicationQuit()
    {
        LogotStatistiku("izslēgšana");
    }

    /// <summary>
    /// Ieraksta detalizētu statistikas kopsavilkumu Unity žurnālā.
    /// Iekļauj datu avotu (SQLite vai Firestore), soļu skaitu, monētas un zivju skaitu.
    /// </summary>
    private void LogotStatistiku(string iemesls)
    {
        if (speletajaProgress == null) return;

        // Iegūst pašreizējo zivju skaitu
        int zivjuSkaits = 0;
        if (akvarijaParvaldnieks != null)
            zivjuSkaits = akvarijaParvaldnieks.IegutZivjuSkaitu();

        // Iegūst maksimālo zivju skaitu
        int maxZivju = 0;
        if (veikalaParvalditajs != null)
            maxZivju = veikalaParvalditajs.MaxKopejaisZivjuSkaits;

        // Nosaka datu avotu, reģistrēti lietotāji izmanto Firestore, viesi SQLite
        string avots = "SQLite";
        if (LietotajaLoma.IrRegistrets())
            avots = "Firestore";

        // Ieraksta visu statistiku žurnālā vienā rindā
        Debug.Log("[Statistika | " + iemesls + "] " + avots +
                  " | Soļi: " + speletajaProgress.soli +
                  " | Monētas: " + speletajaProgress.monetas +
                  " | Zivis: " + zivjuSkaits + "/" + maxZivju);
    }
}
