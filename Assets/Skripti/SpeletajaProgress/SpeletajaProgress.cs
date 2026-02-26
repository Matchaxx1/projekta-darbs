using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SpeletajaProgress : MonoBehaviour
{
    public int soliPrieksMonetas = 50;

    [SerializeField] private float autoSaveIntervalsSekundes = 30f;

    private Text soluSkaits;
    public TextMeshProUGUI soluSkaitsTMP;
    private Text monetuSkaits;
    public TextMeshProUGUI monetuSkaitsTMP;
    private Text monetuSkaits2;
    public TextMeshProUGUI monetuSkaitsTMP2;
    public Button pievienotSolusPoga;
    public Button pievienotVairakSolusPoga;

    public Image progressJosla;
    public TMP_Text cikNoSoliemTMP;


    // Spēlētāja dati
    public int soli = 0;
    public int monetas = 0;
    public int kopejasMonetas = 0;
    private int monetasNoSoliem = 0;

    // Auto-saglabāšanas coroutine reference
    private Coroutine autoSaveCoroutine;

    async void Start()
    {
        // Ielādē progresu caur DatuParvaldnieks (SQLite vai Firestore)
        if (DatuParvaldnieks.Instance != null)
        {
            try
            {
                var progress = await DatuParvaldnieks.Instance.IeladetProgresu();
                if (this == null) return;

                // Ja soļu skaitītājs jau mainīja vērtības kamēr gaidījām DB —
                // pārņem tikai ja DB vērtība ir lielāka
                if (progress.soli > soli)
                    soli = progress.soli;
                if (progress.monetas > monetas)
                    monetas = progress.monetas;
                if (progress.kopejasMonetas > kopejasMonetas)
                    kopejasMonetas = progress.kopejasMonetas;

                // Inicializē soļu-monētu izsekotāju
                monetasNoSoliem = soli / soliPrieksMonetas;

                // Migrācija veciem datiem — kopejasMonetas vismaz kā pašreizējais atlikums
                if (kopejasMonetas < monetas)
                    kopejasMonetas = monetas;

                Debug.Log("Progress ielādēts: " + soli + " soļi, " + monetas + " monētas");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Neizdevās ielādēt progresu: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("DatuParvaldnieks.Instance ir null!");
        }

        monetasNoSoliem = soli / soliPrieksMonetas;
        AtjaunotUI();

        // Sāk periodisko auto-save
        if (autoSaveCoroutine != null)
            StopCoroutine(autoSaveCoroutine);
        autoSaveCoroutine = StartCoroutine(AutoSaveIntervala());
    }

    void OnEnable()
    {
        // Reģistrē button listeners katru reizi, kad objekts tiek aktivizēts
        // Tas ir būtiski Android ierīcēs, lai nodrošinātu, ka klikšķi darbojas
        if (pievienotSolusPoga != null)
        {
            pievienotSolusPoga.onClick.RemoveListener(PievienotSolus);
            pievienotSolusPoga.onClick.AddListener(PievienotSolus);
        }

        if (pievienotVairakSolusPoga != null)
        {
            pievienotVairakSolusPoga.onClick.RemoveListener(PievienotTukstotsSolus);
            pievienotVairakSolusPoga.onClick.AddListener(PievienotTukstotsSolus);
        }
    }

    void OnDestroy()
    {
        // Notīrā listeners, lai izvairītos no atmiņas noplūdēm
        if (pievienotSolusPoga != null)
        {
            pievienotSolusPoga.onClick.RemoveListener(PievienotSolus);
        }

        if (pievienotVairakSolusPoga != null)
        {
            pievienotVairakSolusPoga.onClick.RemoveListener(PievienotTukstotsSolus);
        }

        // Aptur auto-save coroutine
        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
        }
    }

    public void PievienotSolus()
    {
        Debug.Log("PievienotSolus() izsaukts");

        // Pievieno 100 soļus testēšanai
        soli += 100;

        PievinktMonetasNoSoliem();

        SaglabatUnAtjaunotUI();
        // Saglabā datus UZREIZ aizverot izmaiņas
        SaglabatProgresu();
    }

    /// <summary>
    /// Pievieno 1000 soļus (otrajai pogai)
    /// </summary>
    public void PievienotTukstotsSolus()
    {
        Debug.Log("PievienotTukstotsSolus() izsaukts");

        soli += 1000;

        PievinktMonetasNoSoliem();

        SaglabatUnAtjaunotUI();
        // Saglabā datus UZREIZ aizverot izmaiņas
        SaglabatProgresu();
    }

    void SaglabatUnAtjaunotUI()
    {
        // Atjaunina tikai UI - datubaze tiek saglabata tikai aizverot speli
        AtjaunotUI();
    }

    /// <summary>
    /// Atjaunina soļus no solu skaitītāja un automātiski pārrēķina monētas
    /// </summary>
    public void AtjauninatSolusNoSkaitītaja(int jaunieSoli)
    {
        soli = jaunieSoli;

        if (PievinktMonetasNoSoliem())
        {
            // Saglabā uzreiz, lai jaunās monētas netiktu pazaudētas
            SaglabatProgresu();
        }

        // Vienmēr atjauno UI, ne tikai kad mainās monētas
        AtjaunotUI();
    }

    /// <summary>
    /// Pārbauda un pievieno monētas no soļiem (inkrementāli).
    /// Atgriež true, ja tika pievienotas jaunas monētas.
    /// </summary>
    private bool PievinktMonetasNoSoliem()
    {
        int jaunasMonetasNoSoliem = soli / soliPrieksMonetas;
        int pieaugums = jaunasMonetasNoSoliem - monetasNoSoliem;
        if (pieaugums > 0)
        {
            monetas += pieaugums;
            kopejasMonetas += pieaugums;
            monetasNoSoliem = jaunasMonetasNoSoliem;
            Debug.Log("Iegūtas " + pieaugums + " monētas no soļiem!");
            return true;
        }
        return false;
    }

    /// Saglabā pašreizējos soļus un monētas (SQLite vai Firestore, atkarībā no lomas)
    async void SaglabatProgresu()
    {
        if (DatuParvaldnieks.Instance != null)
        {
            await DatuParvaldnieks.Instance.SaglabatProgresu(soli, monetas, kopejasMonetas);
        }
        else
        {
            Debug.LogError("DatuParvaldnieks.Instance ir null! Nevar saglabāt progresu.");
        }
    }

    /// <summary>
    /// Periodisks auto-save — saglabā datus katru N sekunžu
    /// </summary>
    private IEnumerator AutoSaveIntervala()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveIntervalsSekundes);

            if (DatuParvaldnieks.Instance != null)
            {
                SaglabatProgresu();
                Debug.Log("[Auto-Save] Progress saglabāts automātiski: " + soli + " soļi, " + monetas + " monētas");
            }
        }
    }

    void AtjaunotUI()
    {
        // Atjaunina solus

        if (soluSkaitsTMP != null)
        {
            soluSkaitsTMP.text = "Soli: " + soli;
        }

        // Atjaunina monētas (TextMeshPro)
        if (monetuSkaitsTMP != null)
        {
            monetuSkaitsTMP.text = "" + monetas;
        }

        // Atjaunina otro monētu skaitītāju (TextMeshPro)
        if (monetuSkaitsTMP2 != null)
        {
            monetuSkaitsTMP2.text = "" + monetas;
        }

        // Atjaunina progress joslu
        if (progressJosla != null && soliPrieksMonetas > 0)
        {
            int soliKopaSajakot = soli % soliPrieksMonetas;
            progressJosla.fillAmount = (float)soliKopaSajakot / soliPrieksMonetas;
        }

        if (cikNoSoliemTMP != null)
        {
            int soliKopaSajakot = soli % soliPrieksMonetas;
            cikNoSoliemTMP.text = soliKopaSajakot.ToString("D2") + "/" + soliPrieksMonetas;
        }
    }


    // Android netiek uzticami pausēts - izmanto OnApplicationPause
    void OnApplicationPause(bool pauze)
    {
        if (pauze && DatuParvaldnieks.Instance != null)
        {
            SaglabatProgresu();
            SaglabatStatistiku("pauze");
        }
    }

    void OnApplicationQuit()
    {
        if (DatuParvaldnieks.Instance != null)
        {
            SaglabatProgresu();
        }
        SaglabatStatistiku("izslēgšana");
    }

    private void SaglabatStatistiku(string iemesls)
    {
        string avots = LietotajaLoma.IrRegistrets() ? "Firestore" : "SQLite";
        Debug.Log($"[Statistika | {iemesls}] Avots: {avots} | Soļi: {soli} | Monētas: {monetas}");
    }
}
