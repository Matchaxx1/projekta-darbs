using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SpeletajaProgress : MonoBehaviour
{
    public int soliPrieksMonetas = 50; // Cik soļu nepieciešams vienas monētas iegūšanai

    // UI elementi soļu un monētu attēlošanai
    public TextMeshProUGUI soluSkaitsTMP;           // TMP teksts soļu skaitam
    public TextMeshProUGUI monetuSkaitsTMP;         // TMP teksts monētu skaitam
    public TextMeshProUGUI monetuSkaitsTMP2;        // TMP teksts monētu skaitam
    public Button pievienotSolusPoga;               // Debug poga, pievieno 100 soļus
    public Button pievienotVairakSolusPoga;         // Debug poga, pievieno 1000 soļus

    public Image progressJosla;                     // Progresa josla laukā līdz nākošajai monētai
    public TMP_Text cikNoSoliemTMP;                 // Teksts “XX/50” soļu līdz nākošajai monētai

    public SanemtMonetasAnimacija monetasAnimacija; // Atsauce uz animācijas skriptu
    public Transform monetasParadisanasVieta;       // Sākuma pozīcija animācijai
    public AudioSource monetasIegusanasSkana;       // Skaņa, kas atskan, kad iekrājas monēta

    // Spēlētāja dati
    public int soli = 0;              // Pašreizējais soļu skaits
    public int monetas = 0;           // Pašreizējais monētu atlikums
    public int kopejasMonetas = 0;    // Kopējās iepelnītās monētas (visu mūžu)
    private int monetasNoSoliem = 0;  // Cik monētu jau piešķirtas no soļiem (lai apreķinātu pieaugumu)
    public bool datiIeladeti = false; // Vai dati no datubāzes ir ielādēti

    /// <summary>
    /// Ielādē spēlētāja progresu no datubāzes, inicializē monētu skaitītāju.
    /// </summary>
    async void Start()
    {
        // Ielādē progresu caur DatuParvaldnieks (SQLite vai Firestore)
        if (DatuParvaldnieks.Instance != null)
        {
            try
            {
                var progress = await DatuParvaldnieks.Instance.IeladetProgresu();
                if (this == null) return;

                // Ja soļu skaitītājs jau mainīja vērtības kamēr gaidījām DB, pārņem tikai ja DB vērtība ir lielāka
                if (progress.soli > soli)
                    soli = progress.soli;
                if (progress.monetas > monetas)
                    monetas = progress.monetas;
                if (progress.kopejasMonetas > kopejasMonetas)
                    kopejasMonetas = progress.kopejasMonetas;

                // Inicializē soļu-monētu izsekotāju
                monetasNoSoliem = soli / soliPrieksMonetas;

                // Migrācija veciem datiem, kopejasMonetas vismaz kā pašreizējais atlikums
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

        datiIeladeti = true;

        monetasNoSoliem = soli / soliPrieksMonetas;
        AtjaunotUI();
    }

    void OnEnable()
    {
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
        // Saglabā datus, pirms objekts tiek iznīcināts (piemēram, mainot ainu)
        if (datiIeladeti && DatuParvaldnieks.Instance != null)
        {
            SaglabatProgresu();
        }

        // Notīra pogu apstrādātājus
        if (pievienotSolusPoga != null)
        {
            pievienotSolusPoga.onClick.RemoveListener(PievienotSolus);
        }

        if (pievienotVairakSolusPoga != null)
        {
            pievienotVairakSolusPoga.onClick.RemoveListener(PievienotTukstotsSolus);
        }
    }

    /// <summary>
    /// Debug metode, pievieno 100 soļus, pārrēķina monētas un saglabā progresu.
    /// </summary>
    public void PievienotSolus()
    {
        Debug.Log("PievienotSolus() izsaukts");

        // Pievieno 100 soļus testēšanai
        soli += 100;

        // Pārrēķina monētas no jaunajiem soļiem
        PievienotMonetasNoSoliem();

        SaglabatUnAtjaunotUI();
        // Saglabā datus UZREIZ aizverot izmaiņas
        SaglabatProgresu();
    }

    /// <summary>
    /// Debug metode, pievieno 1000 soļus, pārrēķina monētas un saglabā progresu.
    /// </summary>
    public void PievienotTukstotsSolus()
    {
        Debug.Log("PievienotTukstotsSolus() izsaukts");

        // Pievieno 1000 soļus
        soli += 1000;

        // Pārrēķina monētas no jaunajiem soļiem
        PievienotMonetasNoSoliem();

        SaglabatUnAtjaunotUI();
        // Saglabā datus UZREIZ aizverot izmaiņas
        SaglabatProgresu();
    }

    /// <summary>
    /// Atjauno tikai UI elementus, datubāzē tiek saglabāts atsevišķi.
    /// </summary>
    void SaglabatUnAtjaunotUI()
    {
        // Atjauno UI attēlojumu ar pašreizējām vērtībām
        AtjaunotUI();
    }

    /// <summary>
    /// Atjaunina soļus no ārējā soļu skaitītāja (SoluNolasitajs) un automātiski pārrēķina monētas.
    /// Ja monētu skaits mainījās, uzreiz saglabā progresu.
    /// </summary>
    public void AtjauninatSolusNoSkaitītaja(int jaunieSoli)
    {
        soli = jaunieSoli;

        if (PievienotMonetasNoSoliem())
        {
            // Saglabā uzreiz, lai jaunās monētas netiktu pazaudētas
            SaglabatProgresu();
        }

        // Vienmēr atjauno UI, ne tikai kad mainās monētas
        AtjaunotUI();
    }

    /// <summary>
    /// Pārbauda un pievieno monētas no soļiem.
    /// Atgriež true, ja tika pievienotas jaunas monētas.
    /// </summary>
    private bool PievienotMonetasNoSoliem()
    {
        // Aprēķina, cik monētu pieder pēc pašreizējā soļu skaita
        int jaunasMonetasNoSoliem = soli / soliPrieksMonetas;
        int pieaugums = jaunasMonetasNoSoliem - monetasNoSoliem;
        if (pieaugums > 0)
        {
            monetas += pieaugums;
            kopejasMonetas += pieaugums;
            monetasNoSoliem = jaunasMonetasNoSoliem;
            Debug.Log("Iegūtas " + pieaugums + " monētas no soļiem!");

            // Palaiž monētu animāciju interfeisā
            if (monetasAnimacija != null && monetasParadisanasVieta != null)
            {
                monetasAnimacija.SaktAnimaciju(pieaugums, monetasParadisanasVieta);
            }

            // Atskaņo monētas iegūšanas skaņu
            if (monetasIegusanasSkana != null)
            {
                monetasIegusanasSkana.Play();
            }

            return true;
        }
        return false;
    }

    /// SSaglabā pašreizējos soļus un monētas (SQLite vai Firestore, atkarībā no lomas)
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
    /// Atjauno visus UI elementus, soļu tekstu, monētu tekstus, progresa joslu un soļu līdz nākošajai monētai attēlojumu.
    /// </summary>
    void AtjaunotUI()
    {
        // Atjauno soļu skaita attēlojumu

        if (soluSkaitsTMP != null)
        {
            soluSkaitsTMP.text = "Soli: " + soli;
        }

        // Atjauno monētu attēlojumu (pirmais TMP lauks)
        if (monetuSkaitsTMP != null)
        {
            monetuSkaitsTMP.text = "" + monetas;
        }

        // Atjauno otro monētu skaitītāja attēlojumu (otrs TMP lauks)
        if (monetuSkaitsTMP2 != null)
        {
            monetuSkaitsTMP2.text = "" + monetas;
        }

        // Atjauno progresa joslu, rāda, cik soļu vēl vajag līdz nākošajai monētai
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


    // Android lietojumprogramma var tikt apturēta fonā, saglabā progresu pauzē
    /// <summary>
    /// Android ierīcēs lietojumprogramma var tikt apturēta fonā.
    /// Šajā brīdī saglabā progresu un statistiku.
    /// </summary>
    void OnApplicationPause(bool pauze)
    {
        if (pauze && DatuParvaldnieks.Instance != null)
        {
            SaglabatProgresu();
            SaglabatStatistiku("pauze");
        }
    }

    /// <summary>
    /// Saglabā progresu un statistiku, kad lietojumprogramma tiek aizvērta.
    /// </summary>
    void OnApplicationQuit()
    {
        if (DatuParvaldnieks.Instance != null)
        {
            SaglabatProgresu();
        }
        SaglabatStatistiku("izslēgšana");
    }

    /// <summary>
    /// Izdrukā statistiku konsolē diagnostikas nolūkos.
    /// Norāda datu avotu (Firestore vai SQLite), soļu un monētu skaitu.
    /// </summary>
    private void SaglabatStatistiku(string iemesls)
    {
        string avots = LietotajaLoma.IrRegistrets() ? "Firestore" : "SQLite";
        Debug.Log($"[Statistika | {iemesls}] Avots: {avots} | Soļi: {soli} | Monētas: {monetas}");
    }
}
