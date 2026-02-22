using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    public async void SaglabatProgresu(int soli, int monetas)
    {
        if (IzmantotMakoni())
        {
            await MakonaDB.Instance.SaglabatProgresu(soli, monetas);
        }
        else if (DatuBaze.Instance != null)
        {
            DatuBaze.Instance.SaglabatProgresu(soli, monetas);
        }
    }

    public async Task<(int soli, int monetas)> IeladetProgresu()
    {
        if (IzmantotMakoni())
        {
            return await MakonaDB.Instance.IeladetProgresu();
        }

        if (DatuBaze.Instance != null)
        {
            var progress = DatuBaze.Instance.IeladetProgresu();
            if (progress != null)
                return (progress.Soli, progress.Monetas);
        }

        return (0, 0);
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

    // Vai var nopirkt vel (max 3)
    public async Task<bool> VaiVarPirkt(int zivsId)
    {
        if (IzmantotMakoni())
        {
            return await MakonaDB.Instance.VaiVarPirkt(zivsId);
        }

        if (DatuBaze.Instance != null)
        {
            return DatuBaze.Instance.VaiVarPirkt(zivsId);
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
    public async void DzestVienuZiviPecTipa(int zivsId)
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
        List<NopirktaZivsDB> zivis = new List<NopirktaZivsDB>();

        // Lasa no SQLite (viesa datubāze), ja tā ir atvērta
        if (DatuBaze.Instance != null)
        {
            var progress = DatuBaze.Instance.IeladetProgresu();
            if (progress != null)
            {
                soli = progress.Soli;
                monetas = progress.Monetas;
            }
            zivis = DatuBaze.Instance.IegutVisasZivis();
        }

        // Augšupielādē uz Firestore
        await MakonaDB.Instance.ParnestViesaDatus(soli, monetas, zivis);
    }
}
