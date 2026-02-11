using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeletajaProgress : MonoBehaviour
{
    public int soliPrieksMonetas = 0;
    

    private Text soluSkaits;
    public TextMeshProUGUI soluSkaitsTMP;
    private Text monetuSkaits;
    public TextMeshProUGUI monetuSkaitsTMP;
    public Button pievienotSolusPoga;
    public Button pievienotVairakSolusPoga;
    

    // tikai prieks testa, velak janonem
    public int soli = 0;
    public int monetas = 0;

    void Start()
    {
        // Ielādē no SQLite datubāzes
        if (DatuBaze.Instance != null)
        {
            var progress = DatuBaze.Instance.IeladetProgresu();
            if (progress != null)
            {
                soli = progress.Soli;
                monetas = progress.Monetas;
            }
        }
        else
        {
            Debug.LogError("DatuBaze.Instance ir null! Pārliecinies, ka DatuBaze objekts eksistē scēnā.");
        }

        AtjaunotUI();
        Debug.Log("Spēlētāja progress ielādēts: " + soli + " soļi, " + monetas + " monētas");
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
    }
    
    public void PievienotSolus()
    {
        Debug.Log("PievienotSolus() izsaukts");
        
        // Pievieno 100 soļus testēšanai
        soli += 100;
        
        // Pārveido soļus uz monētām
        int jaunasMonetas = soli / soliPrieksMonetas;
        if (jaunasMonetas > monetas) //japarmaina lai sitas paistam stradatu
        {
            int iegutasMonetas = jaunasMonetas - monetas;
            monetas = jaunasMonetas;
            Debug.Log("Iegūtas " + iegutasMonetas + " monētas!");
        }
        
        SaglabatUnAtjaunotUI();
    }

    /// <summary>
    /// Pievieno 1000 soļus (otrajai pogai)
    /// </summary>
    public void PievienotTukstotsSolus()
    {
        Debug.Log("PievienotTukstotsSolus() izsaukts");
        
        soli += 1000;

        // Pārveido soļus uz monētām
        int jaunasMonetas = soli / soliPrieksMonetas;
        if (jaunasMonetas > monetas)
        {
            int iegutasMonetas = jaunasMonetas - monetas;
            monetas = jaunasMonetas;
            Debug.Log("Iegūtas " + iegutasMonetas + " monētas!");
        }

        SaglabatUnAtjaunotUI();
    }
    
    void SaglabatUnAtjaunotUI()
    {
        // Saglabā progresu un atjaunina UI
        SaglabatProgresu();
        AtjaunotUI();
    }

    /// <summary>
    /// Atjaunina soļus no solu skaitītāja un automātiski pārrēķina monētas
    /// </summary>
    public void AtjauninatSolusNoSkaitītaja(int jaunieSoli)
    {
        soli = jaunieSoli;
        
        // Pārveido soļus uz monētām
        int jaunasMonetas = soli / soliPrieksMonetas;
        if (jaunasMonetas > monetas)
        {
            monetas = jaunasMonetas;
        }
        
        AtjaunotUI();
    }

    /// Saglabā pašreizējos soļus un monētas SQLite datubāzē
    void SaglabatProgresu()
    {
        if (DatuBaze.Instance != null)
        {
            DatuBaze.Instance.SaglabatProgresu(soli, monetas);
        }
        else
        {
            Debug.LogError("DatuBaze.Instance ir null! Nevar saglabāt progresu.");
        }
    }
    
    void AtjaunotUI()
    {
        // Atjaunina solus
        if (soluSkaits != null)
            soluSkaits.text = "Soli: " + soli;
        else if (soluSkaitsTMP != null)
            soluSkaitsTMP.text = "Soli: " + soli;

        // Atjaunina monētas
        if (monetuSkaits != null)
            monetuSkaits.text = "Monētas: " + monetas;
        else if (monetuSkaitsTMP != null)
            monetuSkaitsTMP.text = "Monētas: " + monetas;

        // Force Canvas update lai nodrošinātu, ka izmaiņas ir redzamas uz Android
        Canvas.ForceUpdateCanvases();
        
        Debug.Log("UI atjaunots: " + soli + " soļi, " + monetas + " monētas");
    }
    

    void OnApplicationQuit()
    {
        if (DatuBaze.Instance != null)
        {
            SaglabatProgresu();
        }
    }
}
