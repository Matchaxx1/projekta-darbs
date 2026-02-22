using UnityEngine;
using TMPro;
using Firebase.Auth;

public class ProfilaInformacija : MonoBehaviour
{
    [Header("Profila UI Elementi")]
    public TMP_Text lietotajvardsTeksts;
    public TMP_Text lietotajaIDTeksts;
    public TMP_Text lietotajaLomaTeksts;
    public GameObject izrakstitiesPoga;

    private FirebaseAuth auth;
    private FirebaseUser lietotajs;

    private void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    private void Start()
    {
        // Pārbauda vai loma ir ielādēta
        Debug.Log("ProfilaInformacija Start: Lietotāja loma = " + LietotajaLoma.PasreizejaLoma);
        
        AtjaunotProfiluInfo();
    }

    public void AtjaunotProfiluInfo()
    {
        // Iegūst pašreizējo Firebase lietotāju
        if (auth != null)
        {
            lietotajs = auth.CurrentUser;
            
            if (lietotajs != null)
            {
                Debug.Log($"Firebase lietotājs atrasts: {lietotajs.DisplayName} (ID: {lietotajs.UserId})");
            }
            else
            {
                Debug.Log("Firebase lietotājs NAV atrasts (CurrentUser ir null)");
            }
        }
        else
        {
            Debug.LogWarning("FirebaseAuth nav inicializēts!");
        }

        // Ja lietotājs ir pieslēdzies caur Firebase
        if (lietotajs != null)
        {
            // Parāda lietotājvārdu
            if (lietotajvardsTeksts != null)
            {
                string lietotajvards = lietotajs.DisplayName;
                lietotajvardsTeksts.text = string.IsNullOrEmpty(lietotajvards) 
                    ? "Lietotājvārds nav iestatīts" 
                    : lietotajvards;
            }

            // Parāda lietotāja ID
            if (lietotajaIDTeksts != null)
            {
                lietotajaIDTeksts.text = "ID: " + lietotajs.UserId;
            }
        }
        else
        {
            // Ja nav Firebase lietotāja (piemēram, viesis)
            if (lietotajvardsTeksts != null)
            {
                lietotajvardsTeksts.text = "Viesis";
            }

            if (lietotajaIDTeksts != null)
            {
                lietotajaIDTeksts.text = "ID: Nav pieejams";
            }
        }

        // Parāda lietotāja lomu
        if (lietotajaLomaTeksts != null)
        {
            string lomasNosaukums = "";
            switch (LietotajaLoma.PasreizejaLoma)
            {
                case LietotajaLoma.Loma.Nav:
                    lomasNosaukums = "Nav izvēlēts";
                    break;
                case LietotajaLoma.Loma.Viesis:
                    lomasNosaukums = "Viesis";
                    break;
                case LietotajaLoma.Loma.Registrets:
                    lomasNosaukums = "Reģistrēts lietotājs";
                    break;
            }
            lietotajaLomaTeksts.text = "Loma: " + lomasNosaukums;
        }

        // Parāda vai paslēpj izrakstīšanās pogu
        ParvaldītIzrakstīšanāsPogu();
    }

    // Parāda izrakstīšanās pogu tikai reģistrētiem lietotājiem
    private void ParvaldītIzrakstīšanāsPogu()
    {
        if (izrakstitiesPoga != null)
        {
            // Poga redzama tikai reģistrētiem lietotājiem
            bool irRegistrets = LietotajaLoma.IrRegistrets();
            izrakstitiesPoga.SetActive(irRegistrets);
        }
    }

    // Izrakstīšanās funkcija
    public void Izrakstities()
    {
        if (auth != null)
        {
            auth.SignOut();
            Debug.Log("Lietotājs izrakstījies no Firebase");
        }

        // Atiestatīt lietotāja lomu
        LietotajaLoma.AtiestatitLomu();
        
        // Atgriezties uz izvēles ekrānu vai sākuma ekrānu
        UnityEngine.SceneManagement.SceneManager.LoadScene("GalvenaisEkrans");
    }

    // Šo metodi var izsaukt, kad tiek atvērts profils
    private void OnEnable()
    {
        AtjaunotProfiluInfo();
    }
}
