using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
using Unity.VisualScripting;
using System.Collections;
using System.Threading.Tasks;
using Firebase.Extensions;
using EasyPopupSystem;

public class PieslegsanasParvaldnieks : MonoBehaviour
{
    public DependencyStatus dependencyStatus; // Firebase atkarību stāvoklis
    public FirebaseAuth auth;                   // Firebase autentifikācijas instance
    public FirebaseUser lietotajs;              // Pašreizējais Firebase lietotājs

    [Header("Pieslegsanas")]
    public TMP_InputField epastsPieslegsanasIevade;      // E-pasta ievadlauks pieslēgšanās formā
    public TMP_InputField parolePieslegsanasIevade;      // Paroles ievadlauks pieslēgšanās formā
    public TMP_Text bridinajumsPieslegsanasTeksts;       // Brīdinājuma teksts pieslēgšanās kļūdām
    public TMP_Text apstiprinatPieslegsanasTeksts;       // Apstiprinājuma teksts veiksmīgai pieslēgšanās gadījumā

    [Header("Reģistrācija")]
    public TMP_InputField lietotājvārdsRegistracijaIevade;       // Lietotājvārda ievadlauks reģistrācijā
    public TMP_InputField epastsRegistracijaIevade;              // E-pasta ievadlauks reģistrācijā
    public TMP_InputField paroleRegistracijaIevade;              // Paroles ievadlauks reģistrācijā
    public TMP_InputField paroleApstiprinatRegistracijaIevade;   // Paroles apstiprināšanas ievadlauks
    public TMP_Text bridinajumsRegistracijaTeksts;               // Brīdinājuma teksts reģistrācijas kļūdām

    [Header("Aizmirstā parole")]
    public TMP_InputField aizmirstaParoleEpastsIevade;   // E-pasta ievadlauks paroles atiestatīšanai


    /// <summary>
    /// Inicializācijā pārbauda Firebase atkarības un, ja tās ir pieejamas, izsauc Firebase autentifikācijas uzstādīšanu.
    /// </summary>
    private void Awake()
    {
        // Pārbauda, vai visas Firebase atkarības ir pieejamas
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                UzsaktFirebase();
            }
            else
            {
                Debug.LogError("Nevarēja atrisināt visus Firebase dependecies" + dependencyStatus);
            }
        });
    }

    /// <summary>
    /// Inicializē Firebase autentifikācijas instanci.
    /// </summary>
    private void UzsaktFirebase()
    {
        Debug.Log("Setting up firebase auth");
        auth = FirebaseAuth.DefaultInstance;
    }

    /// <summary>
    /// Izsauc pieslēgšanās korutīnu ar ievadīto e-pastu un paroli.
    /// </summary>
    public void PieslegtiesPoga()
    {
        StartCoroutine(Pieslegties(epastsPieslegsanasIevade.text, parolePieslegsanasIevade.text));
    }

    /// <summary>
    /// Izsauc reģistrācijas korutīnu ar ievadīto e-pastu, paroli un lietotājvārdu.
    /// </summary>
    public void RegistracijasPoga()
    {
        StartCoroutine(Registreties(epastsRegistracijaIevade.text, paroleRegistracijaIevade.text, lietotājvārdsRegistracijaIevade.text));
    }

    /// <summary>
    /// Pieslēgšanās korutīna mēģina pieslēgties ar Firebase Auth.
    /// Kļūdas gadījumā attēlo paziņojumu caur EasyPopup.
    /// Veiksmē iestata lietotāja lomu un pāriet uz galveno ekrānu.
    /// </summary>
    private IEnumerator Pieslegties(string epasts, string parole)
    {
        // Sūta pieslēgšanās pieprasījumu Firebase servisam
        Task<AuthResult> PieslegtiesTask = auth.SignInWithEmailAndPasswordAsync(epasts, parole);
        // Gaida, kamēr Firebase operācija pabeidzas
        yield return new WaitUntil(predicate: () => PieslegtiesTask.IsCompleted);

        if (PieslegtiesTask.Exception != null)
        {
            // Izšķir Firebase kļūdas kodu no izņēmuma
            Debug.LogWarning(message: $"Nesanāca reģistrēties {PieslegtiesTask.Exception}");
            FirebaseException firebaseEx = PieslegtiesTask.Exception.GetBaseException() as FirebaseException;
            AuthError kludasKods = (AuthError)firebaseEx.ErrorCode;

            // Attēlo atbilstošu kļūdas paziņojumu atkarībā no kļūdas veida
            string kluda = "Nesanāca pieslēgties!";
            switch (kludasKods)
            {
                case AuthError.MissingEmail:
                    kluda = "Nav ievadīts e-pasts!";
                    break;
                case AuthError.MissingPassword:
                    kluda = "Nav ievadīta parole!";
                    break;
                case AuthError.WrongPassword:
                    kluda = "Nepareiza parole!";
                    break;
                case AuthError.InvalidEmail:
                    kluda = "Nepareiza e-pasta adrese!";
                    break;
                case AuthError.UserNotFound:
                    kluda = "Konts neeksistē!";
                    break;
            }
            EasyPopup.Create("Kļūda", kluda, "PopupError");
        }
            // Saglabā lietotāja atsauci
            lietotajs = PieslegtiesTask.Result.User;
            Debug.LogFormat("Lietotājs veiksmīgi pieslēdzies: {0} ({1})", lietotajs.DisplayName, lietotajs.Email);

            // Iestata lomu kā reģistrēts lietotājs (jo jau ir Firebase lietotājs)
            LietotajaLoma.IestatitKaRegistretu();
            Debug.Log("Loma iestatīta: Registrets (pēc pieslēgšanās)");

            // Pāriet uz galveno ekrānu
            UnityEngine.SceneManagement.SceneManager.LoadScene("GalvenaisEkrans");
    }

    /// <summary>
    /// Validē ievadītos datus un reizē izveido jaunu Firebase lietotāju. Ja lietotājs iepriekš bija viesis, pārnes lokālos SQLite datus uz Firestore mākoni.un pēc migrācijas lokālo datubāzi iztīra, lai pazaudētās liekais saturs netiktu atkārtoti izmests.
    /// </summary>
    private IEnumerator Registreties(string epasts, string parole, string lietotajvards)
    {
        // Pārbauda, vai visi ievadlauki ir aizpildīti un parole ir pietiekami gara
        if (lietotajvards == "")
        {
            EasyPopup.Create("Kļūda", "Nav ievadīts lietotājvārds!", "PopupError");
            bridinajumsRegistracijaTeksts.text = "Nav ievadīts lietotājvārds!";
        }
        else if (epasts == "")
        {
            EasyPopup.Create("Kļūda", "Nav ievadīts e-pasts!", "PopupError");
            bridinajumsRegistracijaTeksts.text = "Nav ievadīts e-pasts!";
        }
        else if (parole == "")
        {
            EasyPopup.Create("Kļūda", "Nav ievadīta parole!", "PopupError");
            bridinajumsRegistracijaTeksts.text = "Nav ievadīta parole!";
        }
        else if (parole.Length < 6)
        {
            EasyPopup.Create("Kļūda", "Parolei jābūt vismaz 6 simbolu garai!", "PopupError");
            bridinajumsRegistracijaTeksts.text = "Parole ir par īsu!";
        }
        else if (paroleRegistracijaIevade.text != paroleApstiprinatRegistracijaIevade.text)
        {
            EasyPopup.Create("Kļūda", "Paroles nav vienādas!", "PopupError");
            bridinajumsRegistracijaTeksts.text = "Paroles nav vienādas!";
        }
        else
        {
            // Visi lauki validīti, sūta reģistrācijas pieprasījumu Firebase
            Task<AuthResult> RegistracijaTask = auth.CreateUserWithEmailAndPasswordAsync(epasts, parole);
            // Gaida, līdz Firebase operācija pabeidzas
            yield return new WaitUntil(predicate: () => RegistracijaTask.IsCompleted);

            if (RegistracijaTask.Exception != null)
            {
                // Izšķir kļūdas kodu un attēlo atbilstošu paziņojumu
                Debug.LogWarning(message: $"Reģistrācija nesanāca ar {RegistracijaTask.Exception}");
                FirebaseException firebaseEx = RegistracijaTask.Exception.GetBaseException() as FirebaseException;
                AuthError kludasKods = (AuthError)firebaseEx.ErrorCode;

                string kluda = "Nesanāca reģistrēties!";
                switch (kludasKods)
                {
                    case AuthError.MissingEmail:
                        kluda = "Nav ievadīts e-pasts!";
                        break;
                    case AuthError.MissingPassword:
                        kluda = "Nav ievadīta parole!";
                        break;
                    case AuthError.WeakPassword:
                        kluda = "Parolei jābūt vismaz 6 simbolu garai!";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        kluda = "E-pastu jau kāds izmanto!";
                        break;
                    case AuthError.InvalidEmail:
                        kluda = "Nav ievadīta pareiza e-pasta adrese!";
                        break;
                }
                EasyPopup.Create("Kļūda", kluda, "PopupError");
            }
            else
            {
                // Veiksmīga reģistrācija, saglabā lietotāja atsauci
                lietotajs = RegistracijaTask.Result.User;

                if (lietotajs != null)
                {
                    // Iestata lietotājvārdu Firebase profilā
                    UserProfile profils = new UserProfile{DisplayName = lietotajvards};

                    // Gaida profila atjaunināšanas operāciju
                    Task ProfilsTask = lietotajs.UpdateUserProfileAsync(profils);
                    yield return new WaitUntil(predicate: () => ProfilsTask.IsCompleted);

                    if (ProfilsTask.Exception != null)
                    {
                        // Profila atjaunināšana neizdevās
                        Debug.LogWarning(message: $"Nesanāca reģistrēties {ProfilsTask.Exception}");
                        FirebaseException firebaseEx = ProfilsTask.Exception.GetBaseException() as FirebaseException;
                        AuthError kludasKods = (AuthError)firebaseEx.ErrorCode;
                        
                    }
                    else
                    {
                        // Saglabā informāciju, vai lietotājs iepriekš bija viesis
                        bool bijViesis = LietotajaLoma.IrViesis();

                        // Iestata lomu kā reģistrēts lietotājs
                        LietotajaLoma.IestatitKaRegistretu();
                        Debug.Log("Loma iestatīta: Registrets (pēc reģistrācijas)");

                        // Ja bijis viesis, pārnes lokālos SQLite datus uz Firestore mākoni
                        if (bijViesis && DatuParvaldnieks.Instance != null)
                        {
                            Debug.Log("Viesis reģistrējas - pārnesam lokālos datus uz Firestore...");
                            Task migracijasTask = DatuParvaldnieks.Instance.ParnestViesaDatusUzMakoni();
                            yield return new WaitUntil(() => migracijasTask.IsCompleted);

                            if (migracijasTask.Exception != null)
                            {
                                Debug.LogWarning("Datu pārnešana neizdevās: " + migracijasTask.Exception);
                            }
                            else
                            {
                                Debug.Log("Viesa dati veiksmīgi pārnesti uz Firestore!");
                                // sinhronizē no mākoņa uz prefsi, lai datus var redzēt uzreiz
                                if (DatuParvaldnieks.Instance != null)
                                {
                                    var _task = DatuParvaldnieks.Instance.IegutVisasZivis();
                                    yield return new WaitUntil(() => _task.IsCompleted);
                                }
                                // Pēc migrācijas izdzēš tikai lokālās datu struktūras (sqlite).
                                if (DatuBaze.Instance != null)
                                {
                                    DatuBaze.Instance.AtiestatitVisu();
                                    // arī tieši izdzēš SQLite failu, ja pastāv
                                    DatuBaze.Instance?.IzdzestDbFailu();
                                }
                                // neatkārto lomas datu dzēšanu; PlayerPrefs paliek, lai saglabātu reģistrētā lietotāja lomu
                                Debug.Log("Lokālā SQLite datubāze iztīrīta pēc migrācijas.");
                            }
                        }

                        bridinajumsRegistracijaTeksts.text = "";
                        UnityEngine.SceneManagement.SceneManager.LoadScene("GalvenaisEkrans");
                    }
                }
            }
        }
    }


    /// <summary>
    /// Validē e-pasta lauku un izsauc paroles atiestatīšanas metodi.
    /// </summary>
    public void AizmirstaParolePoga()
    {
        // Nolasīta un notīra e-pasta ievadlauku
        string epasts = aizmirstaParoleEpastsIevade != null ? aizmirstaParoleEpastsIevade.text.Trim() : "";

        // Pārbauda, vai e-pasts ir ievadīts
        if (string.IsNullOrEmpty(epasts))
        {
            EasyPopup.Create("Kļūda", "Lūdzu ievadiet e-pasta adresi!", "PopupError");
            return;
        }

        // Izsauc paroles atiestatīšanas metodi
        aizmirstaParole(epasts);
    }

    /// <summary>
    /// Nosūta paroles atiestatīšanas e-pastu caur Firebase Auth.
    /// Apstrādā kļūdas un attēlo rezultātu ar EasyPopup popupiem.
    /// </summary>
    private void aizmirstaParole(string epasts)
    {
        // Sūta paroles atiestatīšanas pieprasījumu Firebase servisam
        auth.SendPasswordResetEmailAsync(epasts).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                // Nosūtīšana neizdevās, nosaka kļūdas veidu
                string kluda = "Kļūda nosūtīšanā!";

                if (task.Exception != null)
                {
                    FirebaseException firebaseEx = task.Exception.GetBaseException() as FirebaseException;
                    if (firebaseEx != null)
                    {
                        AuthError kods = (AuthError)firebaseEx.ErrorCode;
                        switch (kods)
                        {
                            case AuthError.InvalidEmail:
                                kluda = "Nepareiza e-pasta adrese!";
                                break;
                            case AuthError.UserNotFound:
                                kluda = "Konts ar šādu e-pastu neeksistē!";
                                break;
                            case AuthError.MissingEmail:
                                kluda = "Lūdzu ievadiet e-pasta adresi!";
                                break;
                        }
                    }
                }

                // Attēlo kļūdu ar EasyPopup
                EasyPopup.Create("Kļūda", kluda, "PopupError");
                Debug.LogWarning("SendPasswordResetEmailAsync kļūda: " + task.Exception);
                return;
            }

            // Veiksmīgi nosūtīts, attēlo apstiprinājuma paziņojumu
            EasyPopup.Create("Sanāca!", "Paroles atiestatīšanas saite nosūtīta uz " + epasts + "!", "PopupSuccess");
            Debug.Log("Paroles atiestatīšanas e-pasts nosūtīts uz: " + epasts);
        });
    }
}
