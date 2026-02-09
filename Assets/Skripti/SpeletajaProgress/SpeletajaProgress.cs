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
        var progress = DatuBaze.Instance.IeladetProgresu();
        if (progress != null)
        {
            soli = progress.Soli;
            monetas = progress.Monetas;
        }

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

        AtjaunotUI();
        Debug.Log("Spēlētāja progress ielādēts: " + soli + " soļi, " + monetas + " monētas");
    }
    
    public void PievienotSolus()
    {
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

    /// Saglabā pašreizējos soļus un monētas SQLite datubāzē
    void SaglabatProgresu()
    {
        DatuBaze.Instance.SaglabatProgresu(soli, monetas);
    }
    
    void AtjaunotUI()
    {
        if (soluSkaits != null)
            soluSkaits.text = "Soli: " + soli;
        else if (soluSkaitsTMP != null)
            soluSkaitsTMP.text = "Soli: " + soli;

        if (monetuSkaits != null)
            monetuSkaits.text = "Monētas: " + monetas;
        else if (monetuSkaitsTMP != null)
            monetuSkaitsTMP.text = "Monētas: " + monetas;
    }
    

    void OnApplicationQuit()
    {
        SaglabatProgresu();
    }
}
