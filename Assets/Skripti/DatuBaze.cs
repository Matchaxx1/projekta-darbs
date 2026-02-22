using UnityEngine;
using SQLite;
using System.Collections.Generic;
using System.IO;

// ===== 1. TABULA: Spēlētāja progress =====
public class SpeletajsDB
{
    [PrimaryKey]
    public int Id { get; set; }
    public int Soli { get; set; }
    public int Monetas { get; set; }
}

// ===== 2. TABULA: Nopirktās zivis (saistīta ar SpeletajsDB) =====
public class NopirktaZivsDB
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int SpeletajaId { get; set; }  // FK -> SpeletajsDB.Id

    public int ZivsId { get; set; }       // Atbilst ZivsSO.id
}

[DefaultExecutionOrder(-100)]
public class DatuBaze : MonoBehaviour
{
    public static DatuBaze Instance { get; private set; }

    private SQLiteConnection db;
    private string dbCels;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            // SQLite tikai viesiem - registretiem lietotajiem izmanto Firestore
            if(!LietotajaLoma.IrRegistrets())
            {
                AtvertDatuBazi();
            }
            else
            {
                Debug.Log("Registrets lietotajs - SQLite netiek atverta");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void AtvertDatuBazi()
    {
        dbCels = Path.Combine(Application.persistentDataPath, "spele.db");
        db = new SQLiteConnection(dbCels);

        db.CreateTable<SpeletajsDB>();
        db.CreateTable<NopirktaZivsDB>();

        if (db.Table<SpeletajsDB>().Count() == 0)
        {
            db.Insert(new SpeletajsDB { Id = 1, Soli = 0, Monetas = 0 });
        }

        NotiritNederigusZivjuId();

        Debug.Log("=== SQLite datubāze atvērta ===");
        Debug.Log("Datubāzes faila atrašanās vieta: " + dbCels);
    }

    private void NotiritNederigusZivjuId()
    {
        int dzesti = db.Execute("DELETE FROM NopirktaZivsDB WHERE SpeletajaId = 1 AND ZivsId <= 0");
        if (dzesti > 0)
        {
            Debug.Log("Notīrīti nederīgi zivju ieraksti (ZivsId <= 0): " + dzesti);
        }
    }

    // ===== SPĒLĒTĀJA PROGRESS =====

    public void SaglabatProgresu(int soli, int monetas)
    {
        db.Update(new SpeletajsDB { Id = 1, Soli = soli, Monetas = monetas });
    }

    public SpeletajsDB IeladetProgresu()
    {
        return db.Find<SpeletajsDB>(1);
    }

    // ===== ZIVJU PIRKUMI =====

    // Pievieno jaunu nopirkto zivi
    public void PievienotNopirktoZivi(int zivsId)
    {
        db.Insert(new NopirktaZivsDB
        {
            SpeletajaId = 1,
            ZivsId = zivsId
        });
    }

    // Cik zivis no konkrētā tipa ir nopirktas
    public int IegutNopirktoSkaitu(int zivsId)
    {
        return db.Table<NopirktaZivsDB>()
            .Where(z => z.SpeletajaId == 1 && z.ZivsId == zivsId)
            .Count();
    }

    // Vai var nopirkt vēl (max 3)
    public bool VaiVarPirkt(int zivsId)
    {
        return IegutNopirktoSkaitu(zivsId) < 3;
    }

    // Iegūst visas saglabātās zivis
    public List<NopirktaZivsDB> IegutVisasZivis()
    {
        return db.Table<NopirktaZivsDB>()
            .Where(z => z.SpeletajaId == 1)
            .ToList();
    }

    /// <summary>
    /// Dzēš visas zivis no datubāzes
    /// </summary>
    public void DzestVisasZivis()
    {
        db.Execute("DELETE FROM NopirktaZivsDB WHERE SpeletajaId = 1");
        Debug.Log("Visas zivis dzēstas no datubāzes");
    }

    /// <summary>
    /// Dzēš vienu zivi pēc tās tipa (pirmo atrasto)
    /// </summary>
    public void DzestVienuZiviPecTipa(int zivsId)
    {
        var zivs = db.Table<NopirktaZivsDB>()
            .Where(z => z.SpeletajaId == 1 && z.ZivsId == zivsId)
            .FirstOrDefault();
        if (zivs != null)
        {
            db.Delete(zivs);
            Debug.Log("Zivs ar tipu " + zivsId + " dzēsta no datubāzes");
        }
    }

    /// <summary>
    /// Atiestatīt visu progresu (IZVĒLES testēšanai)
    /// </summary>
    public void AtiestatitVisu()
    {
        // Dzēš visas zivis
        db.Execute("DELETE FROM NopirktaZivsDB WHERE SpeletajaId = 1");
        
        // Atiestatīt soļus un monētas uz 0
        db.Update(new SpeletajsDB { Id = 1, Soli = 0, Monetas = 0 });
        
        Debug.Log("Viss progress atiestatīts!");
    }

    void OnDestroy()
    {
        if (db != null)
        {
            db.Close();
        }
    }
}
