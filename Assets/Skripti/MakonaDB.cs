using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

/// <summary>
/// Firebase Firestore mākoņa datubāzes pārvaldnieks, glabā reģistrētu lietotāju datus.
/// Nodrošina spēlētāja progresa un nopirkto zivju saglabāšanu, ielādi un dzēšanu.
/// Papildus sinhronizē datus ar lokālo krātuvi, lai spēle darbotos arī bez interneta.
/// </summary>
[DefaultExecutionOrder(-100)]
public class MakonaDB : MonoBehaviour
{
    public static MakonaDB Instance { get; private set; }

    private FirebaseFirestore db;   // Firestore datubāzes instance
    private FirebaseAuth auth;      // Firebase autentifikācijas instance
    
    // Lokālās krātuves atslēgas datu saglabāšanai PlayerPrefs
    private const string PROGRESS_KEY = "player_progress";  // Progresa datu atslēga
    private const string FISH_KEY = "player_fish";           // Zivju datu atslēga

    /// <summary>
    /// Objekts tiek saglabāts starp ekrāniem ar DontDestroyOnLoad.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            db = FirebaseFirestore.DefaultInstance;
            
            auth = FirebaseAuth.DefaultInstance;
            Debug.Log("=== Firestore datubāze atvērta (offline persistance ieslēgta) ===");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// Kad lietotne tiek pauzēta, saglabā datus lokālajā krātuvē, lai tie būtu pieejami arī bez interneta savienojuma.
    /// </summary>
    private void OnApplicationPause(bool pauseStatus) 
    {
        if (pauseStatus)
        {
            IeladetUnSaglabatProgresuSQLite();
            IeladetUnSaglabatZivisSQLite();
            Debug.Log("Dati saglabāti lokālā datubāzē");
        }
    }
    /// <summary>
    /// Kad lietotne tiek aizvērta, saglabā datus lokālajā krātuvē kā rezerves kopiju.
    /// </summary>
    private void OnApplicationQuit()
    {
        IeladetUnSaglabatProgresuSQLite();
        IeladetUnSaglabatZivisSQLite();
        Debug.Log("Dati saglabāti lokālā datubāzē pirms izslēgšanas");
    }

    /// <summary>
    /// Atgriež pašreizējā pieslēgtā lietotāja Firestore dokumenta atsauci.
    /// Dokuments atrodas kolekcijā "lietotaji" un ir identificēts pēc lietotāja ID.
    /// </summary>
    private DocumentReference LietotajaDokuments()
    {
        if (auth == null || auth.CurrentUser == null)
        {
            Debug.LogError("MakonaDB: Nav pieslēgts lietotājs!");
            return null;
        }
        return db.Collection("lietotaji").Document(auth.CurrentUser.UserId);
    }

    /// <summary>
    /// Saglabā spēlētāja progresu (soļus, monētas) Firestore datubāzē un lokālajā krātuvē kā rezerves kopiju.
    /// </summary>
    public async Task SaglabatProgresu(int soli, int monetas, int kopejasMonetas)
    {
        var doc = LietotajaDokuments();
        if (doc == null) return;

        var dati = new Dictionary<string, object>
        {
            { "soli", soli },
            { "monetas", monetas },
            { "kopejasMonetas", kopejasMonetas }
        };

        // Saglabā datus Firestore, izmantojot MergeAll, lai nepārrakstītu citus laukus
        await doc.SetAsync(dati, SetOptions.MergeAll);
        
        // Saglabā tos pašus datus arī lokāli kā rezerves kopiju
        SaglabatProgresuSQLite(soli, monetas, kopejasMonetas);
    }

    /// <summary>
    /// Ielādē spēlētāja progresu no Firestore. Ja pieslēgšanās neizdodas, izmanto lokāli saglabātos datus kā rezerves variantu.
    /// </summary>
    public async Task<(int soli, int monetas, int kopejasMonetas)> IeladetProgresu()
    {
        var doc = LietotajaDokuments();
        if (doc == null) return IeladetProgresuSQLite();

        try
        {
            // Mēģina nolasīt datus no Firestore
            var snapshot = await doc.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                // Nolasa katru lauku, pārbaudot tā eksistenci, lai izvairītos no kļūdām
                int soli = snapshot.ContainsField("soli") ? snapshot.GetValue<int>("soli") : 0;
                int monetas = snapshot.ContainsField("monetas") ? snapshot.GetValue<int>("monetas") : 0;
                int kopejasMonetas = snapshot.ContainsField("kopejasMonetas") ? snapshot.GetValue<int>("kopejasMonetas") : 0;
                return (soli, monetas, kopejasMonetas);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Firebase nolasīšana neizdevās, izmanto lokālo datubāzi: " + ex.Message);
            return IeladetProgresuSQLite();
        }

        return IeladetProgresuSQLite();
    }

    /// <summary>
    /// Pievieno jaunu nopirkto zivi Firestore apakškolekcijai "zivis" un atjauno lokālo krātuvi.
    /// </summary>
    public async Task PievienotNopirktoZivi(int zivsId)
    {
        var doc = LietotajaDokuments();
        if (doc == null) return;

        await doc.Collection("zivis").Document().SetAsync(new Dictionary<string, object>
        {
            { "zivsId", zivsId }
        });

        // Atjauno lokālo zivju krātuvi
        IeladetUnSaglabatZivisSQLite();
    }

    /// <summary>
    /// Saskaita, cik zivis no konkrētā tipa ir saglabātas Firestore.
    /// </summary>
    public async Task<int> IegutNopirktoSkaitu(int zivsId)
    {
        var doc = LietotajaDokuments();
        if (doc == null) return 0;

        var snapshot = await doc.Collection("zivis")
            .WhereEqualTo("zivsId", zivsId)
            .GetSnapshotAsync();

        return snapshot.Count;
    }

    /// <summary>
    /// Pārbauda, vai spēlētājs vēl var nopirkt konkrētā tipa zivi.
    /// </summary>
    public async Task<bool> VaiVarPirkt(int zivsId, int maxDaudzums)
    {
        return await IegutNopirktoSkaitu(zivsId) < maxDaudzums;
    }

    /// <summary>
    /// Iegūst visu saglabāto zivju sarakstu no Firestore.
    /// Ja Firebase nolasīšana neizdodas, izmanto lokāli saglabātos datus.
    /// Veiksmīgas nolasīšanas gadījumā sinhronizē datus ar lokālo krātuvi.
    /// </summary>
    public async Task<List<NopirktaZivsDB>> IegutVisasZivis()
    {
        var doc = LietotajaDokuments();
        if (doc == null)
        {
            return IeladetZivisSQLite();
        }

        try
        {
            var snapshot = await doc.Collection("zivis").GetSnapshotAsync();
            var saraksts = new List<NopirktaZivsDB>();

            foreach (var zivsDoc in snapshot.Documents)
            {
                int zivsId;
                if (!MeginatIegutZivsId(zivsDoc, out zivsId))
                {
                    Debug.LogWarning("Izlaists zivis dokuments bez derīga zivsId: " + zivsDoc.Id);
                    continue;
                }

                saraksts.Add(new NopirktaZivsDB
                {
                    ZivsId = zivsId,
                    SpeletajaId = 1
                });
            }

            SaglabatZivisSQLite(saraksts);
            return saraksts;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Firebase zivju nolasīšana neizdevās, izmanto lokālo datubāzi: " + ex.Message);
            return IeladetZivisSQLite();
        }
    }

    /// <summary>
    /// Dzēš vienu zivi pēc tipa (pirmo atrasto) no Firestore.
    /// Izmanto pārdošanas gadījumā.
    /// </summary>
    public async Task DzestVienuZiviPecTipa(int zivsId)
    {
        var doc = LietotajaDokuments();
        if (doc == null) return;

        var snapshot = await doc.Collection("zivis")
            .WhereEqualTo("zivsId", zivsId)
            .Limit(1)
            .GetSnapshotAsync();

        foreach (var d in snapshot.Documents)
        {
            await d.Reference.DeleteAsync();
            Debug.Log("Zivs ar tipu " + zivsId + " dzesta no Firestore");
            break;
        }
    }

    /// <summary>
    /// Dzēš visas nopirktās zivis no Firestore, izmantojot paketes (batch) dzēšanu.
    /// </summary>
    public async Task DzestVisasZivis()
    {
        var doc = LietotajaDokuments();
        if (doc == null) return;

        var snapshot = await doc.Collection("zivis").GetSnapshotAsync();
        WriteBatch batch = db.StartBatch();

        foreach (var d in snapshot.Documents)
        {
            batch.Delete(d.Reference);
        }

        await batch.CommitAsync();
        Debug.Log("Visas zivis dzēstas no Firestore");
    }

    /// <summary>
    /// Pārnes viesa lokālos SQLite datus uz Firestore pēc reģistrācijas.
    /// Saglabā soļu un monētu progresu, kā arī visas nopirktās zivis.
    /// </summary>
    public async Task ParnestViesaDatus(int soli, int monetas, int kopejasMonetas, List<NopirktaZivsDB> zivis)
    {
        Debug.Log("Pārnešana: viesa dati -> Firestore...");

        // Saglabā progresu Firestore
        await SaglabatProgresu(soli, monetas, kopejasMonetas);

        // Ja ir nopirktas zivis, pārnes tās uz Firestore, izmantojot paketes operāciju
        if (zivis != null && zivis.Count > 0)
        {
            var doc = LietotajaDokuments();
            if (doc != null)
            {
                WriteBatch batch = db.StartBatch();
                foreach (var zivs in zivis)
                {
                    batch.Set(doc.Collection("zivis").Document(), new Dictionary<string, object>
                    {
                        { "zivsId", zivs.ZivsId }
                    });
                }
                await batch.CommitAsync();
                SaglabatZivisSQLite(zivis);
            }
        }

        Debug.Log("Viesa dati veiksmīgi pārnesti uz Firestore!");
    }

    /// <summary>
    /// Pilnībā progresa atiestatīšana, dzēš visas zivis un atiestata progresu gan Firestore, gan lokālajā krātuvē.
    /// </summary>
    public async Task AtiestatitVisu()
    {
        await DzestVisasZivis();
        await SaglabatProgresu(0, 0, 0);
        PlayerPrefs.DeleteKey(PROGRESS_KEY);
        PlayerPrefs.DeleteKey(FISH_KEY);
        Debug.Log("Viss progress atiestatīts Firestore un lokālā datubāzē!");
    }
    
    /// <summary>
    /// Ielādē progresu no Firestore un saglabā to lokāli PlayerPrefs.
    /// </summary>
    public void IeladetUnSaglabatProgresuSQLite()
    {
        (int soli, int monetas, int kopejasMonetas) = IeladetProgresuSQLite();
        SaglabatProgresuSQLite(soli, monetas, kopejasMonetas);
    }
    
    /// <summary>
    /// Saglabā progresa datus PlayerPrefs JSON formātā.
    /// </summary>
    private void SaglabatProgresuSQLite(int soli, int monetas, int kopejasMonetas)
    {
        var dati = new ProgressData { soli = soli, monetas = monetas, kopejasMonetas = kopejasMonetas };
        string json = JsonUtility.ToJson(dati);
        PlayerPrefs.SetString(PROGRESS_KEY, json);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Nolasa progresa datus no PlayerPrefs.
    /// Ja dati nav saglabāti, atgriež nullēm vērtības.
    /// </summary>
    private (int soli, int monetas, int kopejasMonetas) IeladetProgresuSQLite()
    {
        if (!PlayerPrefs.HasKey(PROGRESS_KEY))
            return (0, 0, 0);
        
        string json = PlayerPrefs.GetString(PROGRESS_KEY);
        ProgressData dati = JsonUtility.FromJson<ProgressData>(json);
        return (dati.soli, dati.monetas, dati.kopejasMonetas);
    }
    
    /// <summary>
    /// Ielādē zivis no Firestore un saglabā tās lokāli PlayerPrefs.
    /// </summary>
    public void IeladetUnSaglabatZivisSQLite()
    {
        SinhronizetZivisNoMakonaUzSQLite();
    }

    /// <summary>
    /// Sinhronizē zivju datus no mākoņa datubāzes uz lokālo krātuvi.
    /// </summary>
    private async void SinhronizetZivisNoMakonaUzSQLite()
    {
        var zivis = await IegutVisasZivis();
        SaglabatZivisSQLite(zivis);
    }
    
    /// <summary>
    /// Saglabā zivju sarakstu PlayerPrefs JSON formātā.
    /// </summary>
    private void SaglabatZivisSQLite(List<NopirktaZivsDB> zivis)
    {
        var wrapper = new ZivuSaraksts { zivis = zivis };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(FISH_KEY, json);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Nolasa zivju sarakstu no PlayerPrefs.
    /// Ja dati nav saglabāti, atgriež tukšu sarakstu.
    /// </summary>
    private List<NopirktaZivsDB> IeladetZivisSQLite()
    {
        if (!PlayerPrefs.HasKey(FISH_KEY))
            return new List<NopirktaZivsDB>();
        
        string json = PlayerPrefs.GetString(FISH_KEY);
        ZivuSaraksts wrapper = JsonUtility.FromJson<ZivuSaraksts>(json);
        return wrapper.zivis ?? new List<NopirktaZivsDB>();
    }

    /// <summary>
    /// Mēģina iegūt zivsId vērtību no Firestore dokumenta.
    /// </summary>
    /// <param name="zivsDoc">Firestore dokumenta momentuzņēmums</param>
    /// <param name="zivsId">Iegūtais zivs ID (out parametrs)</param>
    private bool MeginatIegutZivsId(DocumentSnapshot zivsDoc, out int zivsId)
    {
        zivsId = 0;

        if (zivsDoc == null || !zivsDoc.Exists || !zivsDoc.ContainsField("zivsId"))
            return false;

        try
        {
            zivsId = zivsDoc.GetValue<int>("zivsId");
            return true;
        }
        catch
        {
        }

        try
        {
            long zivsIdLong = zivsDoc.GetValue<long>("zivsId");
            zivsId = (int)zivsIdLong;
            return true;
        }
        catch
        {
        }

        try
        {
            string zivsIdTeksts = zivsDoc.GetValue<string>("zivsId");
            return int.TryParse(zivsIdTeksts, out zivsId);
        }
        catch
        {
        }

        return false;
    }
    
    /// <summary>
    /// Palīgklase progresa datu serializācijai JSON formātā.
    /// </summary>
    [System.Serializable]
    private class ProgressData
    {
        public int soli;
        public int monetas;
        public int kopejasMonetas;
    }
    
    /// <summary>
    /// Palīgklase zivju saraksta serializācijai JSON formātā.
    /// </summary>
    [System.Serializable]
    private class ZivuSaraksts
    {
        public List<NopirktaZivsDB> zivis;
    }
}
