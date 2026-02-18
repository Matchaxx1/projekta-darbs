using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

// Vienotais datu parvaldnieks - pagaidam viss iet caur SQLite
[DefaultExecutionOrder(-90)]
public class DatuParvaldnieks : MonoBehaviour
{
    public static DatuParvaldnieks Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LietotajaLoma.IeladetLomu();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ===== SPĒLĒTĀJA PROGRESS =====

    public void SaglabatProgresu(int soli, int monetas)
    {
        if (DatuBaze.Instance != null)
        {
            DatuBaze.Instance.SaglabatProgresu(soli, monetas);
        }
        else
        {
            Debug.LogError("DatuParvaldnieks: Nav pieejama neviena datubāze!");
        }
    }

    public Task<(int soli, int monetas)> IeladetProgresu()
    {
        if (DatuBaze.Instance != null)
        {
            var progress = DatuBaze.Instance.IeladetProgresu();
            if (progress != null)
                return Task.FromResult((progress.Soli, progress.Monetas));
            return Task.FromResult((0, 0));
        }

        Debug.LogError("DatuParvaldnieks: Nav pieejama neviena datubāze!");
        return Task.FromResult((0, 0));
    }

    // ===== ZIVJU PIRKUMI =====

    public void PievienotNopirktoZivi(int zivsId, float x, float y)
    {
        if (DatuBaze.Instance != null)
        {
            DatuBaze.Instance.PievienotNopirktoZivi(zivsId, x, y);
        }
    }

    public Task<int> IegutNopirktoSkaitu(int zivsId)
    {
        if (DatuBaze.Instance != null)
        {
            return Task.FromResult(DatuBaze.Instance.IegutNopirktoSkaitu(zivsId));
        }
        return Task.FromResult(0);
    }

    public Task<bool> VaiVarPirkt(int zivsId)
    {
        if (DatuBaze.Instance != null)
        {
            return Task.FromResult(DatuBaze.Instance.VaiVarPirkt(zivsId));
        }
        return Task.FromResult(false);
    }

    public Task<List<NopirktaZivsDB>> IegutVisasZivis()
    {
        if (DatuBaze.Instance != null)
        {
            return Task.FromResult(DatuBaze.Instance.IegutVisasZivis());
        }
        return Task.FromResult(new List<NopirktaZivsDB>());
    }

    public void SaglabatZivjuPozicijas(List<NopirktaZivsDB> zivis)
    {
        if (DatuBaze.Instance != null)
        {
            DatuBaze.Instance.SaglabatZivjuPozicijas(zivis);
        }
    }

    public void DzestVisasZivis()
    {
        if (DatuBaze.Instance != null)
        {
            DatuBaze.Instance.DzestVisasZivis();
        }
    }

    public void AtiestatitVisu()
    {
        if (DatuBaze.Instance != null)
        {
            DatuBaze.Instance.AtiestatitVisu();
        }
    }
}
