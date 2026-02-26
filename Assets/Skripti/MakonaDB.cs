using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

// Firestore makona datubaze - izmanto registretiem lietotajiem
[DefaultExecutionOrder(-100)]
public class MakonaDB : MonoBehaviour
{
    public static MakonaDB Instance { get; private set; }

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    
    // SQLite lokalā databāze
    private const string PROGRESS_KEY = "player_progress";
    private const string FISH_KEY = "player_fish";

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
    // Dati tiek saglabāti lokālajā datubāzē, lai gadījumā ja nav interneta, tad spēle tik un tā var izmantot datus no sqlite datubāzes.
    private void OnApplicationPause(bool pauseStatus) 
    {
        if (pauseStatus)
        {
            IeladetUnSaglabatProgresuSQLite();
            IeladetUnSaglabatZivisSQLite();
            Debug.Log("Dati saglabāti lokālā datubāzē");
        }
    }
    // Dati tiek saglabāti lokālajā datubāzē, lai gadījumā ja nav interneta, tad spēle tik un tā var izmantot datus no sqlite datubāzes.
    private void OnApplicationQuit()
    {
        IeladetUnSaglabatProgresuSQLite();
        IeladetUnSaglabatZivisSQLite();
        Debug.Log("Dati saglabāti lokālā datubāzē pirms izslēgšanas");
    }

    // Atgriez lietotaja dokumentu Firestore
    private DocumentReference LietotajaDokuments()
    {
        if (auth == null || auth.CurrentUser == null)
        {
            Debug.LogError("MakonaDB: Nav pieslēgts lietotājs!");
            return null;
        }
        return db.Collection("lietotaji").Document(auth.CurrentUser.UserId);
    }

    // ===== SPĒLĒTĀJA PROGRESS =====

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

        await doc.SetAsync(dati, SetOptions.MergeAll);
        
        // Saglabā lokāli
        SaglabatProgresuSQLite(soli, monetas, kopejasMonetas);
    }

    public async Task<(int soli, int monetas, int kopejasMonetas)> IeladetProgresu()
    {
        var doc = LietotajaDokuments();
        if (doc == null) return IeladetProgresuSQLite();

        try
        {
            var snapshot = await doc.GetSnapshotAsync();
            if (snapshot.Exists)
            {
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

    // ===== ZIVJU PIRKUMI =====

    // Pievieno jaunu nopirkto zivi
    public async Task PievienotNopirktoZivi(int zivsId)
    {
        var doc = LietotajaDokuments();
        if (doc == null) return;

        await doc.Collection("zivis").Document().SetAsync(new Dictionary<string, object>
        {
            { "zivsId", zivsId }
        });

        // Atjaunina lokālo datubāzi
        IeladetUnSaglabatZivisSQLite();
    }

    // Cik zivis no konkreta tipa ir nopirktas
    public async Task<int> IegutNopirktoSkaitu(int zivsId)
    {
        var doc = LietotajaDokuments();
        if (doc == null) return 0;

        var snapshot = await doc.Collection("zivis")
            .WhereEqualTo("zivsId", zivsId)
            .GetSnapshotAsync();

        return snapshot.Count;
    }

    // Vai var nopirkt vēl
    public async Task<bool> VaiVarPirkt(int zivsId, int maxDaudzums)
    {
        return await IegutNopirktoSkaitu(zivsId) < maxDaudzums;
    }

    // Iegust visas saglabatas zivis
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

    // Dzes vienu zivi pec tipa (pirmo atrasto)
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

    // Dzes visas zivis no datubāzes
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

    // Parnes viesa SQLite datus uz Firestore pec registracijas
    public async Task ParnestViesaDatus(int soli, int monetas, int kopejasMonetas, List<NopirktaZivsDB> zivis)
    {
        Debug.Log("Pārnešana: viesa dati -> Firestore...");

        await SaglabatProgresu(soli, monetas, kopejasMonetas);

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

    // Atiestatit visu progresu
    public async Task AtiestatitVisu()
    {
        await DzestVisasZivis();
        await SaglabatProgresu(0, 0, 0);
        PlayerPrefs.DeleteKey(PROGRESS_KEY);
        PlayerPrefs.DeleteKey(FISH_KEY);
        Debug.Log("Viss progress atiestatīts Firestore un lokālā datubāzē!");
    }
    
    // ===== LOKĀLĀ DATUBĀZE (SQLite) METODESS =====
    
    // Saglabā progresu lokālā datubāzē
    public void IeladetUnSaglabatProgresuSQLite()
    {
        (int soli, int monetas, int kopejasMonetas) = IeladetProgresuSQLite();
        SaglabatProgresuSQLite(soli, monetas, kopejasMonetas);
    }
    
    private void SaglabatProgresuSQLite(int soli, int monetas, int kopejasMonetas)
    {
        var dati = new ProgressData { soli = soli, monetas = monetas, kopejasMonetas = kopejasMonetas };
        string json = JsonUtility.ToJson(dati);
        PlayerPrefs.SetString(PROGRESS_KEY, json);
        PlayerPrefs.Save();
    }
    
    private (int soli, int monetas, int kopejasMonetas) IeladetProgresuSQLite()
    {
        if (!PlayerPrefs.HasKey(PROGRESS_KEY))
            return (0, 0, 0);
        
        string json = PlayerPrefs.GetString(PROGRESS_KEY);
        ProgressData dati = JsonUtility.FromJson<ProgressData>(json);
        return (dati.soli, dati.monetas, dati.kopejasMonetas);
    }
    
    // Saglabā zivis lokālā datubāzē
    public void IeladetUnSaglabatZivisSQLite()
    {
        SinhronizetZivisNoMakonaUzSQLite();
    }

    private async void SinhronizetZivisNoMakonaUzSQLite()
    {
        var zivis = await IegutVisasZivis();
        SaglabatZivisSQLite(zivis);
    }
    
    private void SaglabatZivisSQLite(List<NopirktaZivsDB> zivis)
    {
        var wrapper = new ZivuSaraksts { zivis = zivis };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(FISH_KEY, json);
        PlayerPrefs.Save();
    }
    
    private List<NopirktaZivsDB> IeladetZivisSQLite()
    {
        if (!PlayerPrefs.HasKey(FISH_KEY))
            return new List<NopirktaZivsDB>();
        
        string json = PlayerPrefs.GetString(FISH_KEY);
        ZivuSaraksts wrapper = JsonUtility.FromJson<ZivuSaraksts>(json);
        return wrapper.zivis ?? new List<NopirktaZivsDB>();
    }

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
    
    [System.Serializable]
    private class ProgressData
    {
        public int soli;
        public int monetas;
        public int kopejasMonetas;
    }
    
    [System.Serializable]
    private class ZivuSaraksts
    {
        public List<NopirktaZivsDB> zivis;
    }
}
