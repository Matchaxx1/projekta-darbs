using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
using Unity.VisualScripting;
using System.Collections;
using System.Threading.Tasks;
using EasyUI.Popup;
using Firebase.Extensions;

public class PieslegsanasParvaldnieks : MonoBehaviour
{
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser lietotajs;

    [Header("Pieslegsanas")]
    public TMP_InputField epastsPieslegsanasIevade;
    public TMP_InputField parolePieslegsanasIevade;
    public TMP_Text bridinajumsPieslegsanasTeksts;
    public TMP_Text apstiprinatPieslegsanasTeksts;

    [Header("Reģistrācija")]
    public TMP_InputField lietotājvārdsRegistracijaIevade;
    public TMP_InputField epastsRegistracijaIevade;
    public TMP_InputField paroleRegistracijaIevade;
    public TMP_InputField paroleApstiprinatRegistracijaIevade;
    public TMP_Text bridinajumsRegistracijaTeksts;

    [Header("Aizmirstā parole")]
    public TMP_InputField aizmirstaParoleEpastsIevade;
    public TMP_Text aizmirstaParoleBridinajums;
    public TMP_Text aizmirstaParoleApstiprinajums;

    private void Awake()
    {
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

    private void UzsaktFirebase()
    {
        Debug.Log("Setting up firebase auth");
        auth = FirebaseAuth.DefaultInstance;
    }

    public void PieslegtiesPoga()
    {
        StartCoroutine(Pieslegties(epastsPieslegsanasIevade.text, parolePieslegsanasIevade.text));
    }

    public void RegistracijasPoga()
    {
        StartCoroutine(Registreties(epastsRegistracijaIevade.text, paroleRegistracijaIevade.text, lietotājvārdsRegistracijaIevade.text));
    }

    private IEnumerator Pieslegties(string _epasts, string _parole)
    {
        Task<AuthResult> PieslegtiesTask = auth.SignInWithEmailAndPasswordAsync(_epasts, _parole);
        yield return new WaitUntil(predicate: () => PieslegtiesTask.IsCompleted);

        if (PieslegtiesTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {PieslegtiesTask.Exception}");
            FirebaseException firebaseEx = PieslegtiesTask.Exception.GetBaseException() as FirebaseException;
            AuthError kludasKods = (AuthError)firebaseEx.ErrorCode;

            string kluda = "Nesanāca pieslēgties!";
            switch (kludasKods)
            {
                case AuthError.MissingEmail:
                    Popup.Show ("<color=red>Kļūda </color>", "Nav ievadīts e-pasts!");
                    break;
                case AuthError.MissingPassword:
                    Popup.Show ("<color=red>Kļūda </color>", "Nav ievadīta parole!");
                    break;
                case AuthError.WrongPassword:
                    Popup.Show ("<color=red>Kļūda </color>", "Nepareiza parole!");
                    break;
                case AuthError.InvalidEmail:
                    Popup.Show ("<color=red>Kļūda </color>", "Nepareiza e-pasta adrese!");
                    break;
                case AuthError.UserNotFound:
                    Popup.Show ("<color=red>Kļūda </color>", "Konts neeksistē!");
                    break;
            }
            bridinajumsPieslegsanasTeksts.text = kluda;
        }
        else
        {
            lietotajs = PieslegtiesTask.Result.User;
            Debug.LogFormat("Lietotājs veiksmīgi pieslēdzies: {0} ({1})", lietotajs.DisplayName, lietotajs.Email);
            bridinajumsPieslegsanasTeksts.text = "";
            apstiprinatPieslegsanasTeksts.text = "Pieslēdzies";

            // Iestata lomu ka registrets lietotajs (jo jau ir Firebase lietotājs)
            LietotajaLoma.IestatitKaRegistretu();
            Debug.Log("Loma iestatīta: Registrets (pēc pieslēgšanās)");
            UnityEngine.SceneManagement.SceneManager.LoadScene("GalvenaisEkrans");
        }
    }

    private IEnumerator Registreties(string _epasts, string _parole, string _lietotajvards)
    {
        if (_lietotajvards == "")
        {
            bridinajumsRegistracijaTeksts.text = "Nav ievadīts lietotājvārds!";
        }
        else if (paroleRegistracijaIevade.text != paroleApstiprinatRegistracijaIevade.text)
        {
            bridinajumsRegistracijaTeksts.text = "Paroles nav vienādas!";
        }
        else
        {
            Task<AuthResult> RegistracijaTask = auth.CreateUserWithEmailAndPasswordAsync(_epasts, _parole);
            yield return new WaitUntil(predicate: () => RegistracijaTask.IsCompleted);

            if (RegistracijaTask.Exception != null)
            {
                Debug.LogWarning(message: $"Reģistrācija nesanāca ar {RegistracijaTask.Exception}");
                FirebaseException firebaseEx = RegistracijaTask.Exception.GetBaseException() as FirebaseException;
                AuthError kludasKods = (AuthError)firebaseEx.ErrorCode;

                string kluda = "Nesanāca reģistrēties!";
                switch (kludasKods)
                {
                    case AuthError.MissingEmail:
                        Popup.Show ("<color=red>Kļūda </color>", "Nav ievadīts e-pasts!");
                        break;
                    case AuthError.MissingPassword:
                        Popup.Show ("<color=red>Kļūda </color>", "Nav ievadīta parole!");
                        break;
                    case AuthError.WeakPassword:
                        Popup.Show ("<color=red>Kļūda </color>", "Parolei jābūt vismaz 6 simbolu garai!");
                        break;
                    case AuthError.EmailAlreadyInUse:
                        Popup.Show ("<color=red>Kļūda </color>", "E-pastu jau kāds izmanto!");
                        break;
                }
                bridinajumsRegistracijaTeksts.text = kluda;
            }
            else
            {
                lietotajs = RegistracijaTask.Result.User;

                if (lietotajs != null)
                {
                    UserProfile profils = new UserProfile{DisplayName = _lietotajvards};

                    Task ProfilsTask = lietotajs.UpdateUserProfileAsync(profils);
                    yield return new WaitUntil(predicate: () => ProfilsTask.IsCompleted);

                    if (ProfilsTask.Exception != null)
                    {
                        Debug.LogWarning(message: $"Nesanāca reģistrēt task ar {ProfilsTask.Exception}");
                        FirebaseException firebaseEx = ProfilsTask.Exception.GetBaseException() as FirebaseException;
                        AuthError kludasKods = (AuthError)firebaseEx.ErrorCode;
                        bridinajumsRegistracijaTeksts.text = "Username Set Failed!";
                    }
                    else
                    {
                        // Saglaba vai bijis viesis PIRMS lomas mainisanas
                        bool bijViesis = LietotajaLoma.IrViesis();

                        // Iestata lomu ka registrets lietotajs (jo tikai reģistrētie var registrēties)
                        LietotajaLoma.IestatitKaRegistretu();
                        Debug.Log("Loma iestatīta: Registrets (pēc reģistrācijas)");

                        // Ja bijis viesis - parnes SQLite datus uz Firestore
                        if (bijViesis && DatuParvaldnieks.Instance != null)
                        {
                            Debug.Log("Viesis reģistrējas - pārnesam lokālos datus uz Firestore...");
                            Task migracijasTask = DatuParvaldnieks.Instance.ParnestViesaDatusUzMakoni();
                            yield return new WaitUntil(() => migracijasTask.IsCompleted);

                            if (migracijasTask.Exception != null)
                                Debug.LogWarning("Datu pārnešana neizdevās: " + migracijasTask.Exception);
                            else
                                Debug.Log("Viesa dati veiksmīgi pārnesti uz Firestore!");
                        }

                        bridinajumsRegistracijaTeksts.text = "";
                        UnityEngine.SceneManagement.SceneManager.LoadScene("GalvenaisEkrans");
                    }
                }
            }
        }
    }



    // === AIZMIRSATA PAROLE ===

    public void AizmirstaParolePoga()
    {
        string epasts = aizmirstaParoleEpastsIevade != null ? aizmirstaParoleEpastsIevade.text.Trim() : "";

        if (string.IsNullOrEmpty(epasts))
        {
            if (aizmirstaParoleBridinajums != null)
                aizmirstaParoleBridinajums.text = "Lūdzu ievadiet e-pasta adresi!";
            return;
        }

        if (aizmirstaParoleBridinajums != null)   aizmirstaParoleBridinajums.text = "";
        if (aizmirstaParoleApstiprinajums != null) aizmirstaParoleApstiprinajums.text = "";

        aizmirstaParole(epasts);
    }

    private void aizmirstaParole(string epasts)
    {
        auth.SendPasswordResetEmailAsync(epasts).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                string kluda = "K\u013c\u016bda nos\u016bt\u012bšan\u0101!";

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

                if (aizmirstaParoleBridinajums != null)
                    aizmirstaParoleBridinajums.text = kluda;

                Debug.LogWarning("SendPasswordResetEmailAsync kļūda: " + task.Exception);
                return;
            }

            if (aizmirstaParoleApstiprinajums != null)
                aizmirstaParoleApstiprinajums.text = "Saite nosūtīta uz " + epasts + "!";

            Debug.Log("Paroles atiestatīšanas e-pasts nosūtīts uz: " + epasts);
        });
    }
}
