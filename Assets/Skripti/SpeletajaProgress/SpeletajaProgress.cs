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
    
    private const string SOLI_KEY = "Soli";
    private const string MONETAS_KEY = "Monetas";

    void Start()
    {

        soli = PlayerPrefs.GetInt(SOLI_KEY, 0);
        monetas = PlayerPrefs.GetInt(MONETAS_KEY, 0);

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
        if (jaunasMonetas > monetas)
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

    /// Saglabā pašreizējos soļus un monētas PlayerPrefs
    void SaglabatProgresu()
    {
        PlayerPrefs.SetInt(SOLI_KEY, soli);
        PlayerPrefs.SetInt(MONETAS_KEY, monetas);
        PlayerPrefs.Save();
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
