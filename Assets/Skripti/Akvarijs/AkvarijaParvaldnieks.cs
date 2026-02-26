using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Auth;
using TMPro;

public class AkvarijaParvaldnieks : MonoBehaviour
{
    [Header("Akvārija iestatījumi")]
    [SerializeField] private RectTransform akvarijs;
    [Tooltip("Cik liela daļa no akvārija apakšas ir smiltis (0–1). Zivis neparādīsies zemāk par šo robežu.")]
    [SerializeField] [Range(0f, 1f)] private float smilsuZonaProcenti = 0.15f;

    [Header("Zivju iestatījumi")]
    [SerializeField] private GameObject zivsPrefabs;

    [Tooltip("Veikala parvaldnieks — zivju uzmeklesanai pec ID.")]
    [SerializeField] private VeikalaParvalditajs veikalaParvalditajs;
    [SerializeField] private TMP_Text zivjuSkaitaTeksts;
    [SerializeField] private TMP_Text zivjuSkaitaTeksts2;

    private List<GameObject> aktivasZivis = new List<GameObject>();
    private List<int> aktivoZivjuId = new List<int>();

    void Start()
    {
        // Ja reģistrēts lietotājs — gaida Firebase Auth pirms Firestore pieprasījuma
        if (LietotajaLoma.IrRegistrets())
        {
            var auth = FirebaseAuth.DefaultInstance;
            if (auth.CurrentUser != null)
            {
                AtjaunotSaglaboatasZivis();
            }
            else
            {
                auth.StateChanged += OnAuthStateChanged;
            }
        }
        else
        {
            // Viesis — izmanto SQLite tieši
            AtjaunotSaglaboatasZivis();
        }
    }

    private void OnAuthStateChanged(object sender, System.EventArgs e)
    {
        var auth = FirebaseAuth.DefaultInstance;
        if (auth.CurrentUser != null)
        {
            auth.StateChanged -= OnAuthStateChanged;
            AtjaunotSaglaboatasZivis();
        }
    }

    private void OnDestroy()
    {
        FirebaseAuth.DefaultInstance.StateChanged -= OnAuthStateChanged;
    }

    // Atjauno visas saglabātās zivis no datubāzes (SQLite vai Firestore)
    private async void AtjaunotSaglaboatasZivis()
    {
        if (DatuParvaldnieks.Instance == null)
        {
            Debug.LogError("DatuParvaldnieks.Instance ir null!");
            return;
        }

        List<NopirktaZivsDB> saglaboatas;
        try
        {
            saglaboatas = await DatuParvaldnieks.Instance.IegutVisasZivis();
        }
        catch (System.Exception e)
        {
            Debug.LogError("AkvarijaParvaldnieks: Neizdevās ielādēt zivis - " + e.Message);
            return;
        }

        // Ja objekts tika iznīcīnāts skatīlas mainīšanas laikā — pārtrauc
        if (this == null) return;

        foreach (var zivsDB in saglaboatas)
        {
            ZivsSO zivsSO = AtrastZivsSO(zivsDB.ZivsId);
            if (zivsSO == null)
            {
                Debug.LogWarning("Nav atrasts ZivsSO ar id: " + zivsDB.ZivsId);
                continue;
            }

            // Nārsto zivi nejaušā vietā (nepieciešama saglabātā pozīcija)
            IeladetZivis(zivsSO);
        }

        Debug.Log("Atjaunotas " + saglaboatas.Count + " zivis no datubāzes");
    }

    // Meklē ZivsSO pēc id caur VeikalaParvalditajs
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

    // Nārsto jaunu zivi akvārijā, pamatojoties uz ZivsSO datiem (arī saglabā DB)
    public void IeliktZivi(ZivsSO zivsSO)
    {
        int zivsId = (zivsSO != null) ? zivsSO.id : 0;
        IeliktZivi(zivsSO, zivsId);
    }

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

        // Saglabā DB (pozīcija netiek glabāta)
        if (DatuParvaldnieks.Instance != null)
        {
            DatuParvaldnieks.Instance.PievienotNopirktoZivi(zivsId);
        }

        // Uztur saskanīgu id arī runtime objektam
        zivsSO.id = zivsId;

        IeladetZivis(zivsSO);
    }

    // Nārsto zivi nejaušā vietā akvārijā (bez DB saglabāšanas)
    private void IeladetZivis(ZivsSO zivsSO)
    {
        if (akvarijs == null) return;

        Vector3[] pasaulesStūri = new Vector3[4];
        akvarijs.GetWorldCorners(pasaulesStūri);
        float akvarijsAugstums = pasaulesStūri[2].y - pasaulesStūri[0].y;
        float minY = pasaulesStūri[0].y + akvarijsAugstums * smilsuZonaProcenti;

        float randX = Random.Range(pasaulesStūri[0].x, pasaulesStūri[2].x);
        float randY = Random.Range(minY, pasaulesStūri[2].y);
        Vector3 spawnPozicija = new Vector3(randX, randY, 0f);

        Vector2 kustibaMin = new Vector2(pasaulesStūri[0].x, minY);
        Vector2 kustibaMax = new Vector2(pasaulesStūri[2].x, pasaulesStūri[2].y);

        IzveidotZivi(zivsSO, spawnPozicija, kustibaMin, kustibaMax);
    }

    // Pārdod vienu zivi pēc tipa: noņem no akvārija un dzēš no DB
    public async Task PardotZivi(int zivsId)
    {
        // Meklē pirmo atbilstošo GameObject
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
            if (aktivasZivis[indekss] != null)
                Destroy(aktivasZivis[indekss]);

            aktivasZivis.RemoveAt(indekss);
            aktivoZivjuId.RemoveAt(indekss);
            AtjaunotZivjuSkaituUI();
        }

        // Dzēš no DB — await, lai UI atjaunojums notiktu tikai pēc dzēšanas
        if (DatuParvaldnieks.Instance != null)
        {
            await DatuParvaldnieks.Instance.DzestVienuZiviPecTipa(zivsId);
        }

        Debug.Log("Zivs ar id " + zivsId + " pārdota un noņemta no akvārija");
    }

    // Kopīga metode zivs izveidošanai
    private void IzveidotZivi(ZivsSO zivsSO, Vector3 pozicija, Vector2 kustibsMin, Vector2 kustibsMax)
    {
        if (zivsSO == null)
        {
            Debug.LogWarning("AkvarijaParvaldnieks.IzveidotZivi: zivsSO ir null!");
            return;
        }

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

        GameObject jaunaZivs = Instantiate(prefabs, pozicija, Quaternion.identity, transform);
        jaunaZivs.name = zivsSO.zivsNosaukums;

        // Piemēro izmēru
        float lielums = zivsSO.lielums > 0f ? zivsSO.lielums : 1f;
        jaunaZivs.transform.localScale = Vector3.one * lielums;

        // SVARĪGI: Uzreiz iestatīt Rigidbody2D, lai zivs nekrīt!
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

        SpriteRenderer sr = jaunaZivs.GetComponent<SpriteRenderer>();
        if (sr != null && zivsSO.zivsSpraits != null)
        {
            sr.sprite = zivsSO.zivsSpraits;
        }

        // Inicializē kustību ar akvārija robežām
        ZivsKustiba kustiba = jaunaZivs.GetComponent<ZivsKustiba>();
        if (kustiba != null)
            kustiba.Inicializet(kustibsMin, kustibsMax);

        aktivasZivis.Add(jaunaZivs);
        aktivoZivjuId.Add(zivsSO.id);
        Debug.Log("Zivs '" + zivsSO.zivsNosaukums + "' pievienota akvārijam!");
        AtjaunotZivjuSkaituUI();
    }

    // Atgriež aktīvo zivju skaitu akvārijā
    public int IegutZivjuSkaitu()
    {
        aktivasZivis.RemoveAll(z => z == null);
        return aktivasZivis.Count;
    }

    // Atjaunina zivju skaita tekstu UI
    private void AtjaunotZivjuSkaituUI()
    {
        int max = veikalaParvalditajs != null ? veikalaParvalditajs.MaxKopejaisZivjuSkaits : 0;
        int skaits = IegutZivjuSkaitu();
        string teksts = "Akvārijā " + skaits + " / " + max + " zivis";
        if (zivjuSkaitaTeksts != null)  zivjuSkaitaTeksts.text  = teksts;
        if (zivjuSkaitaTeksts2 != null) zivjuSkaitaTeksts2.text = teksts;
    }

    /// <summary>
    /// Dzēš visas zivis no akvārija un datubāzes
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

    // VELAK JANONEM!!!!
    private void OnDrawGizmosSelected()
    {
        if (akvarijs == null) return;

        Vector3[] pasaulesStūri = new Vector3[4];
        akvarijs.GetWorldCorners(pasaulesStūri);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pasaulesStūri[0], pasaulesStūri[1]);
        Gizmos.DrawLine(pasaulesStūri[1], pasaulesStūri[2]);
        Gizmos.DrawLine(pasaulesStūri[2], pasaulesStūri[3]);
        Gizmos.DrawLine(pasaulesStūri[3], pasaulesStūri[0]);
    }
}
