using UnityEngine;
using TMPro;
using Firebase.Auth;

public class ProfilaInformacija : MonoBehaviour
{
    [Header("Profila UI Elementi")]
    public TMP_Text lietotajvardsTeksts;     // Teksta lauks lietotājvārda attēlošanai
    public TMP_Text lietotajaIDTeksts;       // Teksta lauks lietotāja unikālā ID attēlošanai
    public TMP_Text lietotajaLomaTeksts;     // Teksta lauks lietotāja lomas attēlošanai
    public GameObject izrakstitiesPoga;       // Izrakstīšanās pogas objekts (redzams tikai reģistrētiem)

    // Firebase autentifikācijas atsauces
    private FirebaseAuth auth;               // Firebase Auth instance
    private FirebaseUser lietotajs;          // Pašreizējais Firebase lietotājs

    /// <summary>
    /// Inicializē Firebase Auth instanci.
    /// </summary>
    private void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    /// <summary>
    /// Start brīdī atjauno profila informāciju un ieraksta lomu žurnālā.
    /// </summary>
    private void Start()
    {
        // Pārbauda, vai loma ir ielādēta no PlayerPrefs
        Debug.Log("ProfilaInformacija Start: Lietotāja loma = " + LietotajaLoma.PasreizejaLoma);
        
        AtjaunotProfiluInfo();
    }

    /// <summary>
    /// Galvenā profila informācijas atjaunošanas metode.
    /// Iegūst pašreizējo Firebase lietotāju un attēlo visus profila datus UI elementos.
    /// Ja lietotājs nav pieslēdzies (viesis), parāda atbilstošus noklusējuma tekstus.
    /// </summary>
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

        // Ja lietotājs ir pieslēdzies caur Firebase, tad parāda reālos datus
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

            // Parāda lietotāja unikālo Firebase ID
            if (lietotajaIDTeksts != null)
            {
                lietotajaIDTeksts.text = "ID: " + lietotajs.UserId;
            }
        }
        else
        {
            
            if (lietotajvardsTeksts != null)
            {
                lietotajvardsTeksts.text = "Viesis";
            }

            if (lietotajaIDTeksts != null)
            {
                lietotajaIDTeksts.text = "";
            }
        }

        // Parāda lietotāja lomu teksta formātā
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

        // Pārvalda izrakstīšanās pogas redzamību
        ParvaldītIzrakstīšanāsPogu();
    }

    /// <summary>
    /// Poga ir redzama tikai reģistrētiem lietotājiem, jo viesiem nav no kā izrakstīties.
    /// </summary>
    private void ParvaldītIzrakstīšanāsPogu()
    {
        if (izrakstitiesPoga != null)
        {
            // Izrakstīšanās poga ir redzama tikai reģistrētiem lietotājiem
            bool irRegistrets = LietotajaLoma.IrRegistrets();
            izrakstitiesPoga.SetActive(irRegistrets);
        }
    }

    /// <summary>
    /// Saglabā spēlētāja progresu, izrakstās no Firebase Auth un atiestata lietotāja lomu.
    /// Pēc izrakstīšanās novirza uz galveno ekrānu.
    /// </summary>
    public async void Izrakstities()
    {
        // Saglabā progresu pirms izrakstīšanās, lai dati netiktu pazaudēti
        SpeletajaProgress progress = FindFirstObjectByType<SpeletajaProgress>();
        if (progress != null && DatuParvaldnieks.Instance != null)
        {
            await DatuParvaldnieks.Instance.SaglabatProgresu(progress.soli, progress.monetas, progress.kopejasMonetas);
            Debug.Log("Progress saglabāts pirms izrakstīšanās");
        }

        // Izrakstās no Firebase autentifikācijas sistēmas
        if (auth != null)
        {
            try
            {
                auth.SignOut();
                Debug.Log("Lietotājs izrakstījies no Firebase");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("SignOut izsauca kļūdu: " + ex);
            }
        }

        // Atiestata lietotāja lomu
        LietotajaLoma.AtiestatitLomu();

        // Notīrām jebkādus lokālos viesu datus, lai pēc izrakstīšanās viesis sāktu no nulles
        if (DatuBaze.Instance != null)
        {
            DatuBaze.Instance.AtiestatitVisu();
            Debug.Log("Lokālā SQLite datubāze iztīrīta pēc izrakstīšanās");
        }
        
        // Notīra arī mākoņa datubāzes piesaistīto lokālo kešatmiņu (PlayerPrefs), lai neļautu datiem pārnesties
        if (MakonaDB.Instance != null)
        {
            MakonaDB.Instance.NotiritLokalosDatus();
        }

        // Atiestatām atmiņā esošo progresu, lai nākamajā ciklā vecais nenosūtītos
        if (progress != null)
        {
            progress.soli = 0;
            progress.monetas = 0;
            progress.kopejasMonetas = 0;
        }
        
        // Novirza uz galveno ekrānu, kur lietotājs varēs izvēlēties lomu no jauna
        UnityEngine.SceneManagement.SceneManager.LoadScene("GalvenaisEkrans");
    }

    /// <summary>
    /// Kad profila panelis kļūst aktīvs, automātiski atjauno profila informāciju.
    /// </summary>
    private void OnEnable()
    {
        AtjaunotProfiluInfo();
    }
}
