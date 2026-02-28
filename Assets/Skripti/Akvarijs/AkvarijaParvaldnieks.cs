using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Auth;
using TMPro;

public class AkvarijaParvaldnieks : MonoBehaviour
{
    [Header("Akvārija iestatījumi")]
    [SerializeField] private RectTransform akvarijs;          // Akvārija UI robežu taisnstūris.
    [SerializeField] [Range(0f, 1f)] private float smilsuZonaProcenti = 0.15f; // Smilšu zonas augstums procentos no visa akvārija.

    [Header("Zivju iestatījumi")]
    [SerializeField] private GameObject zivsPrefabs;

    [Tooltip("Veikala parvaldnieks — zivju uzmeklesanai pec ID.")]
    [SerializeField] private VeikalaParvalditajs veikalaParvalditajs; // Atsauce uz veikala pārvaldnieku
    [SerializeField] private TMP_Text zivjuSkaitaTeksts;      // Primārais zivju skaita teksts
    [SerializeField] private TMP_Text zivjuSkaitaTeksts2;     // Sekundārais zivju skaita teksts

    private List<GameObject> aktivasZivis = new List<GameObject>();  // Aktīvo zivju objektu saraksts
    private List<int> aktivoZivjuId = new List<int>();               // Aktīvo zivju ID saraksts (atbilst indeksam)

    /// <summary>
    /// Ielādē saglabātās zivis.
    /// Reģistrētiem lietotājiem gaida Firebase Auth pirms Firestore pieprasījuma.
    /// Viesiem izmanto SQLite tieši.
    /// </summary>
    void Start()
    {
        // Ja reģistrēts lietotājs, tad gaida Firebase Auth pirms Firestore pieprasījuma
        if (LietotajaLoma.IrRegistrets())
        {
            var auth = FirebaseAuth.DefaultInstance;
            if (auth.CurrentUser != null)
            {
                AtjaunotSaglabatasZivis();
            }
            else
            {
                auth.StateChanged += OnAuthStateChanged;
            }
        }
        else
        {
            // Viesis, izmanto SQLite tieši
            AtjaunotSaglabatasZivis();
        }
    }

    /// <summary>
    /// Firebase autentifikācijas stāvokļa maiņas apstrādātājs.
    /// Kad lietotājs veikmīgi pieslēdzas, ielādē saglabātās zivis.
    /// </summary>
    private void OnAuthStateChanged(object sender, System.EventArgs e)
    {
        var auth = FirebaseAuth.DefaultInstance;
        if (auth.CurrentUser != null)
        {
            auth.StateChanged -= OnAuthStateChanged;
            AtjaunotSaglabatasZivis();
        }
    }

    /// <summary>
    /// Atreģistrē notikuma apstrādātāju, kad objekts tiek iznīcināts.
    /// </summary>
    private void OnDestroy()
    {
        FirebaseAuth.DefaultInstance.StateChanged -= OnAuthStateChanged;
    }

    /// <summary>
    /// Ielādē visas saglabātās zivis no datubāzes un izveido atbilstošus GameObjects akvārijā.
    /// </summary>
    private async void AtjaunotSaglabatasZivis()
    {
        if (DatuParvaldnieks.Instance == null)
        {
            Debug.LogError("DatuParvaldnieks.Instance ir null! Nevar ielādēt zivis.");
            return;
        }

        List<NopirktaZivsDB> saglabatas;
        try
        {
            saglabatas = await DatuParvaldnieks.Instance.IegutVisasZivis();
        }
        catch (System.Exception e)
        {
            Debug.LogError("AkvarijaParvaldnieks: Neizdevās ielādēt zivis - " + e.Message);
            return;
        }

        // Ja objekts tika iznīcināts ainas mainīšanas laikā, pārtrauc
        if (this == null) return;

        // Izveido zivi katram datubāzes ierakstam
        foreach (var zivsDB in saglabatas)
        {
            ZivsSO zivsSO = AtrastZivsSO(zivsDB.ZivsId);
            if (zivsSO == null)
            {
                Debug.LogWarning("Nav atrasts ZivsSO ar id: " + zivsDB.ZivsId);
                continue;
            }

            // Ieliek zivi nejaušā vietā
            IeladetZivis(zivsSO);
        }

        Debug.Log("Atjaunotas " + saglabatas.Count + " zivis no datubāzes");
    }

    /// <summary>
    /// Meklē ZivsSO datu objektu pēc ID, izmantojot VeikalaPārvaldtāju.
    /// Ja VeikalaPārvaldtājs nav piesaistīts, mēģina to atrast ekrānā.
    /// </summary>
    private ZivsSO AtrastZivsSO(int zivsId)
    {
        if (veikalaParvalditajs == null)
        {
            veikalaParvalditajs = FindFirstObjectByType<VeikalaParvalditajs>();
            if (veikalaParvalditajs == null)
            {
                Debug.LogError("AkvarijaParvaldnieks: Nav atrasts VeikalaParvalditajs!");
                return null;
            }
        }
        return veikalaParvalditajs.IegutZivsSO(zivsId);
    }

    /// <summary>
    /// Ievieto jaunu zivi akvārijā un saglabā datubāzē.
    /// Izmanto ZivsSO.id kā zivs identifikatoru.
    /// </summary>
    public void IeliktZivi(ZivsSO zivsSO)
    {
        // Nolasa zivs ID no ScriptableObject
        int zivsId = (zivsSO != null) ? zivsSO.id : 0;
        IeliktZivi(zivsSO, zivsId);
    }

    /// <summary>
    /// Ievieto jaunu zivi akvārijā ar norādītu ID, saglabā datubāzē un izveido vizualo zivs objektu.
    /// </summary>
    public void IeliktZivi(ZivsSO zivsSO, int zivsId)
    {
        if (zivsSO == null)
        {
            Debug.LogWarning("AkvarijaParvaldnieks: ZivsSO ir null!");
            return;
        }

        if (akvarijs == null)
        {
            Debug.LogError("AkvarijaParvaldnieks: Nav piesaistīts akvarijs!");
            return;
        }

        // Saglabā nopirkto zivi datubāzē
        if (DatuParvaldnieks.Instance != null)
        {
            DatuParvaldnieks.Instance.PievienotNopirktoZivi(zivsId);
        }

        
        zivsSO.id = zivsId;

        // Izveido zivs vizualo objektu akvārijā
        IeladetZivis(zivsSO);
    }

    /// <summary>
    /// Ieliek zivi nejaušā vietā akvārijā.
    /// Aprēķina akvārija robežas un smilšu zonu, lai noteiktu atļautu peldēšanas apgabalu.
    /// </summary>
    private void IeladetZivis(ZivsSO zivsSO)
    {
        if (akvarijs == null) return;

        // Iegūst akvārija robežu koordinātes pasaules telpā
        Vector3[] pasaulesStūri = new Vector3[4];
        akvarijs.GetWorldCorners(pasaulesStūri);

        // Aprēķina minimālo Y pozīciju, ņemot vērā smilšu zonu
        float akvarijsAugstums = pasaulesStūri[2].y - pasaulesStūri[0].y;
        float minY = pasaulesStūri[0].y + akvarijsAugstums * smilsuZonaProcenti;

        // Ģenerē nejaušu pozīciju, kur likt zivi
        float randX = Random.Range(pasaulesStūri[0].x, pasaulesStūri[2].x);
        float randY = Random.Range(minY, pasaulesStūri[2].y);
        Vector3 spawnPozicija = new Vector3(randX, randY, 0f);

        // Nosaka kustības robežas (minimālās un maksimālās koordinātes)
        Vector2 kustibaMin = new Vector2(pasaulesStūri[0].x, minY);
        Vector2 kustibaMax = new Vector2(pasaulesStūri[2].x, pasaulesStūri[2].y);

        // Izveido zivs vizualo objektu
        IzveidotZivi(zivsSO, spawnPozicija, kustibaMin, kustibaMax);
    }

    /// <summary>
    /// Pārdod vienu zivi pēc tipa, noņem no akvārija, iznīcina objektu un dzēš no datubāzes.
    /// </summary>
    public async Task PardotZivi(int zivsId)
    {
        // Meklē pirmo atbilstošo zivs objektu pēc ID
        int indekss = -1;
        for (int i = 0; i < aktivoZivjuId.Count; i++)
        {
            if (aktivoZivjuId[i] == zivsId)
            {
                indekss = i;
                break;
            }
        }

        if (indekss >= 0)
        {
            // Iznīcina zivs objektu un noņem no sarakstiem
            if (aktivasZivis[indekss] != null)
                Destroy(aktivasZivis[indekss]);

            aktivasZivis.RemoveAt(indekss);
            aktivoZivjuId.RemoveAt(indekss);
            AtjaunotZivjuSkaituUI();
        }

        // Dzēš no datubāzes un gaida, lai pārliecinātos par veiksmīgu dzēšanu
        if (DatuParvaldnieks.Instance != null)
        {
            await DatuParvaldnieks.Instance.DzestVienuZiviPecTipa(zivsId);
        }

        Debug.Log("Zivs ar id " + zivsId + " pārdota un noņemta no akvārija");
    }

    /// <summary>
    /// Kopīgā metode zivs vizuālā objekta izveidošanai.
    /// Iestata attēlu, izmēru, fizikas komponentus un kustības robežas.
    /// Pievieno jauno zivi aktīvo zivju sarakstam.
    /// </summary>
    private void IzveidotZivi(ZivsSO zivsSO, Vector3 pozicija, Vector2 kustibsMin, Vector2 kustibsMax)
    {
        if (zivsSO == null)
        {
            Debug.LogWarning("AkvarijaParvaldnieks.IzveidotZivi: zivsSO ir null!");
            return;
        }

        // Izvēlas prefabu
        GameObject prefabs;
        if (zivsSO.zivsPrefabs != null)
            prefabs = zivsSO.zivsPrefabs;
        else
            prefabs = zivsPrefabs;

        if (prefabs == null)
        {
            Debug.LogError("AkvarijaParvaldnieks: Nav norādīts zivs prefab!");
            return;
        }

        // Izveido jauno zivs objektu norādītajā pozīcijā
        GameObject jaunaZivs = Instantiate(prefabs, pozicija, Quaternion.identity, transform);
        jaunaZivs.name = zivsSO.zivsNosaukums;

        // Piemēro izmēru no ZivsSO datiem
        float lielums = zivsSO.lielums > 0f ? zivsSO.lielums : 1f;
        jaunaZivs.transform.localScale = Vector3.one * lielums;

        // SVARĪGI: Iestata Rigidbody2D, lai zivs nekrīt un nepieaug pie gravitācijas
        Rigidbody2D rb = jaunaZivs.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            Debug.LogWarning("Zivij nav Rigidbody2D komponentes!");
        }

        // Uzstāda zivs attēlu no ScriptableObject
        SpriteRenderer sr = jaunaZivs.GetComponent<SpriteRenderer>();
        if (sr != null && zivsSO.zivsSpraits != null)
        {
            sr.sprite = zivsSO.zivsSpraits;
        }

        // Uzstāda zivs kustību ar akvārija robežām
        ZivsKustiba kustiba = jaunaZivs.GetComponent<ZivsKustiba>();
        if (kustiba != null)
            kustiba.Inicializet(kustibsMin, kustibsMax);

        // Pievieno jauno zivi aktīvo zivju sarakstam un atjauno UI
        aktivasZivis.Add(jaunaZivs);
        aktivoZivjuId.Add(zivsSO.id);
        Debug.Log("Zivs '" + zivsSO.zivsNosaukums + "' pievienota akvārijam!");
        AtjaunotZivjuSkaituUI();
    }

    /// <summary>
    /// Atgriež aktīvo zivju skaitu akvārijā.
    /// Automātiski notīra sarakstu no iznīcinātiem objektiem.
    /// </summary>
    public int IegutZivjuSkaitu()
    {
        aktivasZivis.RemoveAll(z => z == null);
        return aktivasZivis.Count;
    }

    /// <summary>
    /// Atjauno zivju skaita attēlojumu UI tekstos (formatā “Akvārijā X / Y zivis”).
    /// </summary>
    private void AtjaunotZivjuSkaituUI()
    {
        int max = veikalaParvalditajs != null ? veikalaParvalditajs.MaxKopejaisZivjuSkaits : 0;
        int skaits = IegutZivjuSkaitu();
        string teksts = "Akvārijā " + skaits + " / " + max + " zivis";
        if (zivjuSkaitaTeksts != null)  zivjuSkaitaTeksts.text  = teksts;
        if (zivjuSkaitaTeksts2 != null) zivjuSkaitaTeksts2.text = teksts;
    }

    /// <summary>
    /// Dzēš visas zivis no akvārija un datubāzes.
    /// Iznīcina visus zivju GameObjects un attīra sarakstus.
    /// </summary>
    public void DzestVisasZivis()
    {
        // Iznīcina visus zivju GameObjects
        foreach (var zivs in aktivasZivis)
        {
            if (zivs != null)
            {
                Destroy(zivs);
            }
        }

        // Notīra sarakstus
        aktivasZivis.Clear();
        aktivoZivjuId.Clear();
        AtjaunotZivjuSkaituUI();

        // Dzēš no datubāzes
        if (DatuParvaldnieks.Instance != null)
        {
            DatuParvaldnieks.Instance.DzestVisasZivis();
            Debug.Log("Visas zivis dzēstas no akvārija un datubāzes!");
        }
        else
        {
            Debug.LogError("DatuParvaldnieks.Instance ir null!");
        }
    }

    
}
