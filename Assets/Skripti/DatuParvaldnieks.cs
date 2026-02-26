using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;

// Vienotais datu parvaldnieks - izvelas starp SQLite (viesis) un Firestore (registrets)
[DefaultExecutionOrder(-90)]
public class DatuParvaldnieks : MonoBehaviour
{
    public static DatuParvaldnieks Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            LietotajaLoma.IeladetLomu();

            // Sinhronizē lomu ar Firebase Auth stāvokli
            // Ja Firebase jau ir pieslēgts lietotājs (saglabāta sesija),
            // automātiski iestata lomu kā reģistrēts neatkarīgi no PlayerPrefs
            // Sinhronizē tikai ja lietotājs jau bija izvēlējies lomu iepriekš
            // Ja loma ir Nav — lietotājs vēl nav izvēlējies, nedrīkst pārrakstīt
            var auth = FirebaseAuth.DefaultInstance;
            if (auth.CurrentUser != null
                && LietotajaLoma.PasreizejaLoma != LietotajaLoma.Loma.Nav)
            {
                if (!LietotajaLoma.IrRegistrets())
                {
                    LietotajaLoma.IestatitKaRegistretu();
                    Debug.Log("DatuParvaldnieks: Firebase sesija atrasta, loma iestatīta: Registrets");
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Vai lietotajs ir registrets un Firestore ir pieejams
    private bool IzmantotMakoni()
    {
        return LietotajaLoma.IrRegistrets() && MakonaDB.Instance != null;
    }

    // ===== SPĒLĒTĀJA PROGRESS =====

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

    public async Task<(int soli, int monetas, int kopejasMonetas)> IeladetProgresu()
    {
        if (IzmantotMakoni())
        {
            return await MakonaDB.Instance.IeladetProgresu();
        }

        if (DatuBaze.Instance != null)
        {
            var progress = DatuBaze.Instance.IeladetProgresu();
            if (progress != null)
                return (progress.Soli, progress.Monetas, progress.KopejasMonetas);
        }

        return (0, 0, 0);
    }

    // ===== ZIVJU PIRKUMI =====

    // Pievieno jaunu nopirkto zivi
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

    // Cik zivis no konkreta tipa ir nopirktas
    public async Task<int> IegutNopirktoSkaitu(int zivsId)
    {
        if (IzmantotMakoni())
        {
            return await MakonaDB.Instance.IegutNopirktoSkaitu(zivsId);
        }

        if (DatuBaze.Instance != null)
        {
            return DatuBaze.Instance.IegutNopirktoSkaitu(zivsId);
        }

        return 0;
    }

    // Vai var nopirkt vēl
    public async Task<bool> VaiVarPirkt(int zivsId, int maxDaudzums)
    {
        if (IzmantotMakoni())
        {
            return await MakonaDB.Instance.VaiVarPirkt(zivsId, maxDaudzums);
        }

        if (DatuBaze.Instance != null)
        {
            return DatuBaze.Instance.VaiVarPirkt(zivsId, maxDaudzums);
        }

        return false;
    }

    // Iegust visas saglabatas zivis
    public async Task<List<NopirktaZivsDB>> IegutVisasZivis()
    {
        if (IzmantotMakoni())
        {
            return await MakonaDB.Instance.IegutVisasZivis();
        }

        if (DatuBaze.Instance != null)
        {
            return DatuBaze.Instance.IegutVisasZivis();
        }

        return new List<NopirktaZivsDB>();
    }

    // Dzes visas zivis
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

    // Dzes vienu zivi pec tipa (piem. pārdošanas gadījumā)
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

    // Atiestatit visu progresu
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

    // Parnes viesa SQLite datus uz Firestore (izsauc pec registracijas)
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

        // Lasa no SQLite (viesa datubāze), ja tā ir atvērta
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

        // Augšupielādē uz Firestore
        await MakonaDB.Instance.ParnestViesaDatus(soli, monetas, kopejasMonetas, zivis);
    }
}
