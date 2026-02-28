using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;

/// <summary>
/// Vienotais datu pārvaldnieks, nodrošina vienotu interfeisu datu operācijām neatkarīgi no datu avota.
/// </summary>
[DefaultExecutionOrder(-90)]
public class DatuParvaldnieks : MonoBehaviour
{
    public static DatuParvaldnieks Instance { get; private set; }

    /// <summary>
    /// Ielādē lietotāja lomu un sinhronizē lomu ar Firebase Auth stāvokli.
    /// Ja Firebase sesija jau pastāv un lietotājs iepriekš bija izvēlējies lomu, automātiski iestata lomu kā reģistrētu.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            LietotajaLoma.IeladetLomu();

            // Sinhronizē lomu ar Firebase Auth stāvokli:
            // Ja Firebase jau ir pieslēgts lietotājs (saglabāta sesija), automātiski iestata lomu kā reģistrēts.
            // Sinhronizē tikai, ja lietotājs jau bija izvēlējies lomu iepriekš, ja loma nav, lietotājs vēl nav izvēlējies un nedrīkst pārrakstīt.
            var auth = FirebaseAuth.DefaultInstance;
            if (auth.CurrentUser != null
                && LietotajaLoma.PasreizejaLoma != LietotajaLoma.Loma.Nav)
            {
                if (!LietotajaLoma.IrRegistrets())
                {
                    LietotajaLoma.IestatitKaRegistretu();
                    Debug.Log("DatuParvaldnieks: Firebase sesija atrasta, loma mainīta uz Reģistrēts");
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    /// <summary>
    /// Pārbauda, vai jāizmanto mākoņa datubāze (Firestore).
    /// Atgriež true, ja lietotājs ir reģistrēts un MakonaDB instance ir pieejama.
    /// </summary>
    private bool IzmantotMakoni()
    {
        return LietotajaLoma.IrRegistrets() && MakonaDB.Instance != null;
    }

    // filtrē null‑ref izsaukumus, kas rodas AndroidPlayer logā
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Exception && logString.Contains("NullReferenceException")
            && logString.Contains("AndroidPlayer"))
        {
            Debug.Log( logString + "\n" + stackTrace);
        }
    }

    /// <summary>
    /// Saglabā spēlētāja progresu izvēlētajā datubāzē (Firestore vai SQLite).
    /// </summary>
    public async Task SaglabatProgresu(int soli, int monetas, int kopejasMonetas)
    {
        if (IzmantotMakoni())
        {
            await MakonaDB.Instance.SaglabatProgresu(soli, monetas, kopejasMonetas);
        }
        else if (DatuBaze.Instance != null)
        {
            DatuBaze.Instance.SaglabatProgresu(soli, monetas, kopejasMonetas);
        }
    }

    /// <summary>
    /// Ielādē spēlētāja progresu no izvēlētās datubāzes.
    /// Atgriež soļu skaitu, monētās un kopējās monētās.
    /// </summary>
    public async Task<(int soli, int monetas, int kopejasMonetas)> IeladetProgresu()
    {
        try
        {
            if (IzmantotMakoni())
            {
                return await MakonaDB.Instance.IeladetProgresu();
            }

            if (DatuBaze.Instance != null && DatuBaze.Instance.IsOpen)
            {
                var progress = DatuBaze.Instance.IeladetProgresu();
                if (progress != null)
                    return (progress.Soli, progress.Monetas, progress.KopejasMonetas);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }

        return (0, 0, 0);
    }

    /// <summary>
    /// Pievieno jaunu nopirkto zivi izvēlētajā datubāzē.
    /// </summary>
    /// <param name="zivsId">Nopirktās zivs tipa identifikators</param>
    public async void PievienotNopirktoZivi(int zivsId)
    {
        if (IzmantotMakoni())
        {
            await MakonaDB.Instance.PievienotNopirktoZivi(zivsId);
        }
        else if (DatuBaze.Instance != null)
        {
            DatuBaze.Instance.PievienotNopirktoZivi(zivsId);
        }
    }

    /// <summary>
    /// Saskaita, cik zivis no konkrētā tipa ir nopirktas izvēlētajā datubāzē.
    /// </summary>
    public async Task<int> IegutNopirktoSkaitu(int zivsId)
    {
        if (IzmantotMakoni())
        {
            return await MakonaDB.Instance.IegutNopirktoSkaitu(zivsId);
        }

        if (DatuBaze.Instance != null && DatuBaze.Instance.IsOpen)
        {
            try
            {
                return DatuBaze.Instance.IegutNopirktoSkaitu(zivsId);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning(ex.Message);
                return 0;
            }
        }

        return 0;
    }

    /// <summary>
    /// Pārbauda, vai spēlētājs var nopirkt vēl vienu konkrētā tipa zivi.
    /// </summary>
    public async Task<bool> VaiVarPirkt(int zivsId, int maxDaudzums)
    {
        if (IzmantotMakoni())
        {
            return await MakonaDB.Instance.VaiVarPirkt(zivsId, maxDaudzums);
        }

        if (DatuBaze.Instance != null && DatuBaze.Instance.IsOpen)
        {
            try
            {
                return DatuBaze.Instance.VaiVarPirkt(zivsId, maxDaudzums);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning(ex.Message);
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Iegūst visu nopirkto zivju sarakstu no izvēlētās datubāzes.
    /// </summary>
    public async Task<List<NopirktaZivsDB>> IegutVisasZivis()
    {
        if (IzmantotMakoni())
        {
            return await MakonaDB.Instance.IegutVisasZivis();
        }

        if (DatuBaze.Instance != null && DatuBaze.Instance.IsOpen)
        {
            try
            {
                return DatuBaze.Instance.IegutVisasZivis();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning(ex.Message);
                return new List<NopirktaZivsDB>();
            }
        }

        return new List<NopirktaZivsDB>();
    }

    /// <summary>
    /// Dzēš visas nopirktās zivis no izvēlētās datubāzes.
    /// </summary>
    public async void DzestVisasZivis()
    {
        if (IzmantotMakoni())
        {
            await MakonaDB.Instance.DzestVisasZivis();
        }
        else if (DatuBaze.Instance != null)
        {
            DatuBaze.Instance.DzestVisasZivis();
        }
    }

    /// <summary>
    /// Dzēš vienu zivi pēc tipa (pārdošanas gadījumā).
    /// </summary>
    public async Task DzestVienuZiviPecTipa(int zivsId)
    {
        if (IzmantotMakoni())
        {
            await MakonaDB.Instance.DzestVienuZiviPecTipa(zivsId);
        }
        else if (DatuBaze.Instance != null)
        {
            DatuBaze.Instance.DzestVienuZiviPecTipa(zivsId);
        }
    }

    /// <summary>
    /// Pilnībā progresa atiestatīšana, dzēš visas zivis un atiestata soļus un monētas.
    /// </summary>
    public async void AtiestatitVisu()
    {
        if (IzmantotMakoni())
        {
            await MakonaDB.Instance.AtiestatitVisu();
        }
        else if (DatuBaze.Instance != null)
        {
            DatuBaze.Instance.AtiestatitVisu();
        }
    }

    /// <summary>
    /// Pārnes viesa lokālos SQLite datus uz Firestore mākoņa datubāzi.
    /// Tiek izsaukta pēc tam, kad viesis veiksmīgi reģistrējas vai pieslēdzas, lai viņa līdzšinējie sasniegumi tiktu saglabāti mākonī.
    /// </summary>
    public async Task ParnestViesaDatusUzMakoni()
    {
        if (MakonaDB.Instance == null)
        {
            Debug.LogWarning("ParnestViesaDatusUzMakoni: MakonaDB nav pieejams!");
            return;
        }

        int soli = 0;
        int monetas = 0;
        int kopejasMonetas = 0;
        List<NopirktaZivsDB> zivis = new List<NopirktaZivsDB>();

        // Nolasa spēlētāja progresu no lokālās SQLite datubāzes
        if (DatuBaze.Instance != null)
        {
            var progress = DatuBaze.Instance.IeladetProgresu();
            if (progress != null)
            {
                soli = progress.Soli;
                monetas = progress.Monetas;
                kopejasMonetas = progress.KopejasMonetas;
            }
            zivis = DatuBaze.Instance.IegutVisasZivis();
        }

        // Augšuplādē nolasītos datus uz Firestore mākoņa datubāzi
        await MakonaDB.Instance.ParnestViesaDatus(soli, monetas, kopejasMonetas, zivis);
    }

    /// <summary>
    /// (Utility) copy cloud progress/zivis back into local SQLite – used when a
    /// registered user logs out and we briefly want the guest DB to reflect those
    /// same values. The feature was removed from the regular logout flow, but the
    /// method is available if you ever want to invoke it manually for testing.
    /// </summary>
    public async Task SinhronizetMakonaUzSQLite()
    {
        if (MakonaDB.Instance == null)
        {
            Debug.LogWarning("SinhronizetMakonaUzSQLite: MakonaDB nav pieejams!");
            return;
        }
        if (DatuBaze.Instance == null)
        {
            Debug.LogWarning("SinhronizetMakonaUzSQLite: DatuBaze nav pieejams!");
            return;
        }

        // Nolasām progresu no mākoņa un pārrakstām lokālajā datubāzē
        var prog = await MakonaDB.Instance.IeladetProgresu();
        DatuBaze.Instance.SaglabatProgresu(prog.soli, prog.monetas, prog.kopejasMonetas);

        // Iztīrām esošo zivju tabulu un aizstājam ar mākoņa saturu
        DatuBaze.Instance.DzestVisasZivis();
        var zivis = await MakonaDB.Instance.IegutVisasZivis();
        foreach (var z in zivis)
        {
            DatuBaze.Instance.PievienotNopirktoZivi(z.ZivsId);
        }

        Debug.Log("DatuParvaldnieks: sinhronizēts mākoņa saturs uz SQLite");
    }
}
