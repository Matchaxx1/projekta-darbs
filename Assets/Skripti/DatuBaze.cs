using UnityEngine;
using SQLite;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// SQLite tabulas modelis spēlētāja progresa datu glabāšanai.
/// Satur soļu skaitu, pašreizējo monētu atlikumu un kopējo nopelnīto monētu skaitu.
/// </summary>
public class SpeletajsDB
{
    [PrimaryKey]
    public int Id { get; set; }               // Spēlētāja unikālais identifikators
    public int Soli { get; set; }             // Kopējais noieto soļu skaits
    public int Monetas { get; set; }          // Pašreizējais monētu skaits
    public int KopejasMonetas { get; set; }   // Kopējais visā laikā nopelnītais monētu skaits
}

// ===== 2. TABULA: Nopirktās zivis (saistīta ar SpeletajsDB) =====
/// <summary>
/// SQLite tabulas modelis nopirkto zivju glabāšanai.
/// Katrs ieraksts pārstāv vienu nopirktu zivi, kas pieder konkrētam spēlētājam.
/// Zivs tips tiek identificēts pēc ZivsId, kas atbilst ZivsSO.id vērtībai.
/// </summary>
public class NopirktaZivsDB
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }               // Automātiski ģenerēts unikālais ieraksta identifikators

    [Indexed]
    public int SpeletajaId { get; set; }      // Ārējā atslēga -> SpeletajsDB.Id (spēlētāja identifikators)

    public int ZivsId { get; set; }           // Zivs tipa identifikators (atbilst ZivsSO.id)
}

/// <summary>
/// Lokālā SQLite datubāzes pārvaldnieks, glabā spēlētāja progresu un nopirktās zivis.
/// Šī datubāze tiek izmantota tikai viesiem. 
/// </summary>
[DefaultExecutionOrder(-100)]
public class DatuBaze : MonoBehaviour
{

    public static DatuBaze Instance { get; private set; }

    private SQLiteConnection db;   // SQLite savienojuma objekts
    private string dbCels;          // Pilnais ceļs līdz datubāzes failam ierīcē

    /// <summary>
    /// SQLite datubāze tiek atvērta tikai viesiem, reģistrētiem lietotājiem dati tiek glabāti Firestore.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            // SQLite tiek atvērta tikai viesiem, reģistrēti lietotāji izmanto Firestore
            if(!LietotajaLoma.IrRegistrets())
            {
                AtvertDatuBazi();
            }
            else
            {
                Debug.Log("Registrets lietotajs - SQLite netiek atverts");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Atver SQLite datubāzi, izveido nepieciešamās tabulas un pievieno sākotnējo ierakstu, ja datubāze ir tukša.
    /// </summary>
    private void AtvertDatuBazi()
    {
        // Izveido ceļu uz datubāzes failu ierīces pastavīgo datu mapē
        dbCels = Path.Combine(Application.persistentDataPath, "spele.db");
        // Izveido savienojumu ar SQLite datubāzi
        db = new SQLiteConnection(dbCels);

        // Izveido tabulas, ja tās vēl nepastāv
        db.CreateTable<SpeletajsDB>();
        db.CreateTable<NopirktaZivsDB>();

        // Ja spēlētāja tabula ir tukša, pievieno sākotnējo ierakstu ar nullēm vērtībām
        if (db.Table<SpeletajsDB>().Count() == 0)
        {
            db.Insert(new SpeletajsDB { Id = 1, Soli = 0, Monetas = 0 });
        }

        Debug.Log("SQLite datubāze atvērta");
        Debug.Log("Datubāzes faila atrašanās vieta: " + dbCels);
    }


    /// <summary>
    /// Saglabā spēlētāja progresu (soļus, monētas) SQLite datubāzē.
    /// </summary>
    public void SaglabatProgresu(int soli, int monetas, int kopejasMonetas)
    {
        db.Update(new SpeletajsDB { Id = 1, Soli = soli, Monetas = monetas, KopejasMonetas = kopejasMonetas });
    }

    /// <summary>
    /// Ielādē spēlētāja progresu no SQLite datubāzes.
    /// </summary>

    public SpeletajsDB IeladetProgresu()
    {
        return db.Find<SpeletajsDB>(1);
    }

    /// <summary>
    /// Pievieno jaunu nopirkto zivi datubāzē.
    /// </summary>
    /// <param name="zivsId">Nopirktās zivs tipa identifikators</param>
    public void PievienotNopirktoZivi(int zivsId)
    {
        db.Insert(new NopirktaZivsDB
        {
            SpeletajaId = 1,
            ZivsId = zivsId
        });
    }

    /// <summary>
    /// Saskaita, cik zivis no konkrētā tipa spēlētājs ir nopircis.
    /// </summary>
    /// <param name="zivsId">Zivs tipa identifikators</param>
    /// <returns>Nopirkto zivju skaits</returns>
    public int IegutNopirktoSkaitu(int zivsId)
    {
        return db.Table<NopirktaZivsDB>()
            .Where(z => z.SpeletajaId == 1 && z.ZivsId == zivsId)
            .Count();
    }

    /// <summary>
    /// Pārbauda, vai spēlētājs var nopirkt vēl vienu konkrētā tipa zivi.
    /// </summary>
    /// <param name="zivsId">Zivs tipa identifikators</param>
    /// <param name="maxDaudzums">Maksimālais atļautais daudzums šim tipam</param>
    public bool VaiVarPirkt(int zivsId, int maxDaudzums)
    {
        return IegutNopirktoSkaitu(zivsId) < maxDaudzums;
    }

    /// <summary>
    /// Iegūst visu nopirkto zivju sarakstu no datubāzes.
    /// </summary>
    public List<NopirktaZivsDB> IegutVisasZivis()
    {
        return db.Table<NopirktaZivsDB>()
            .Where(z => z.SpeletajaId == 1)
            .ToList();
    }

    /// <summary>
    /// Dzēš visas nopirktās zivis no datubāzes.
    /// </summary>
    public void DzestVisasZivis()
    {
        db.Execute("DELETE FROM NopirktaZivsDB WHERE SpeletajaId = 1");
        Debug.Log("Visas zivis dzēstas no datubāzes");
    }

    /// <summary>
    /// Dzēš vienu zivi pēc tās tipa (pirmo atrasto ar šo ZivsId).
    /// Izmanto pārdošanas gadījumā, kad jānoņem tikai viena zivs.
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
    /// Pilnībā progresa atiestatīšana, dzēš visas zivis un atiestata soļus un monētas uz nulli. Paredzēts testēšanas vajadzībām.
    /// </summary>
    public void AtiestatitVisu()
    {
        // Dzēš visas nopirktās zivis no zivju tabulas
        db.Execute("DELETE FROM NopirktaZivsDB WHERE SpeletajaId = 1");
        
        // Atiestata soļus un monētas uz nulli spēlētāja tabulā
        db.Update(new SpeletajsDB { Id = 1, Soli = 0, Monetas = 0, KopejasMonetas = 0 });
        
        Debug.Log("Viss progress atiestatīts!");
    }

    /// <summary>
    /// Fiziski dzēš SQLite datubāzes failu un aizver savienojumu.
    /// Izmantojams, ja vēlas pilnībā noņemt lokālos datus.
    /// </summary>
    public void IzdzestDbFailu()
    {
        if (db != null)
        {
            db.Close();
            db = null;
        }

        if (!string.IsNullOrEmpty(dbCels) && File.Exists(dbCels))
        {
            File.Delete(dbCels);
            Debug.Log("SQLite datubāzes fails izdzēsts: " + dbCels);
        }

        // Ja stingri esam joprojām viesis, atveram tukšu datubāzi, lai spēle
        // nezaudētu savienojumu (un šādi iespējamās izmaiņas netiktu izsviestas
        // ārpusāk pēc nākamā izrakstīšanās).
        if (!LietotajaLoma.IrRegistrets())
        {
            AtvertDatuBazi();
        }
    }

    /// <summary>
    /// Pārbauda, vai SQLite datubāze ir atvērta un gatava lietošanai.
    /// </summary>
    public bool IsOpen => db != null;

    /// <summary>
    /// Aizver SQLite datubāzes savienojumu, kad objekts tiek iznīcināts.
    /// </summary>
    void OnDestroy()
    {
        if (db != null)
        {
            db.Close();
        }
    }
}
