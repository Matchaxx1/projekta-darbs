using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Auth;

public class AkvarijaParvaldnieks : MonoBehaviour
{
    [Header("Akvārija iestatījumi")]
    [SerializeField] private RectTransform akvarijs;

    [Header("Zivju iestatījumi")]
    [SerializeField] private GameObject zivsPrefabs;

    [Header("Visas pieejamās zivis (jāpievieno inspectorī)")]
    [SerializeField] private ZivsSO[] visasZivis;

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

    // Meklē ZivsSO pēc id
    private ZivsSO AtrastZivsSO(int zivsId)
    {
        if (visasZivis == null) return null;
        foreach (var z in visasZivis)
        {
            if (z != null && z.id == zivsId)
                return z;
        }
        return null;
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
        float randX = Random.Range(pasaulesStūri[0].x, pasaulesStūri[2].x);
        float randY = Random.Range(pasaulesStūri[0].y, pasaulesStūri[2].y);
        Vector3 spawnPozicija = new Vector3(randX, randY, 0f);

        IzveidotZivi(zivsSO, spawnPozicija);
    }

    // Pārdod vienu zivi pēc tipa: noņem no akvārija un dzēš no DB
    public void PardotZivi(int zivsId)
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
        }

        // Dzēš no DB
        if (DatuParvaldnieks.Instance != null)
        {
            DatuParvaldnieks.Instance.DzestVienuZiviPecTipa(zivsId);
        }

        Debug.Log("Zivs ar id " + zivsId + " pārdota un noņemta no akvārija");
    }

    // Kopīga metode zivs izveidošanai
    private void IzveidotZivi(ZivsSO zivsSO, Vector3 pozicija)
    {
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

        aktivasZivis.Add(jaunaZivs);
        aktivoZivjuId.Add(zivsSO.id);
        Debug.Log("Zivs '" + zivsSO.zivsNosaukums + "' pievienota akvārijam!");
    }

    // Atgriež aktīvo zivju skaitu akvārijā
    public int IegutZivjuSkaitu()
    {
        aktivasZivis.RemoveAll(z => z == null);
        return aktivasZivis.Count;
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
