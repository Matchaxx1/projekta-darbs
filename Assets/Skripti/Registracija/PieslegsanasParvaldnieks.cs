using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
using Unity.VisualScripting;
using System.Collections;
using System.Threading.Tasks;

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
            bridinajumsPieslegsanasTeksts.text = kluda;
        }
        else
        {
            lietotajs = PieslegtiesTask.Result.User;
            Debug.LogFormat("Lietotājs veiksmīgi pieslēdzies: {0} ({1})", lietotajs.DisplayName, lietotajs.Email);
            bridinajumsPieslegsanasTeksts.text = "";
            apstiprinatPieslegsanasTeksts.text = "Pieslēdzies";

            // Iestata lomu ka registrets lietotajs
            LietotajaLoma.IestatitKaRegistretu();
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
                        kluda = "Nav ievadīts e-pasts!";
                        break;
                    case AuthError.MissingPassword:
                        kluda = "Nav ievadīta parole!";
                        break;
                    case AuthError.WeakPassword:
                        kluda = "Parolei jābūt vismaz 6 simboliem!";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        kluda = "E-pasta adrese jau tiek izmantota!";
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
                        // Iestata lomu ka registrets lietotajs
                        LietotajaLoma.IestatitKaRegistretu();
                        bridinajumsRegistracijaTeksts.text = "";
                        UnityEngine.SceneManagement.SceneManager.LoadScene("GalvenaisEkrans");
                    }
                }
            }
        }
    }
}
