using System.Collections.Generic;
using UnityEngine;

public class AkvarijaParvaldnieks : MonoBehaviour
{
    [Header("Akvārija iestatījumi")]
    [SerializeField] private RectTransform akvarijs;

    [Header("Zivju iestatījumi")]
    [SerializeField] private GameObject zivsPrefabs;

    [Header("Visas pieejamās zivis (jāpievieno inspectorī)")]
    [SerializeField] private ZivsSO[] visasZivis;

    private List<GameObject> aktivasZivis = new List<GameObject>();
    // Saglabā zivsId katram GameObject, lai varētu saglabāt pozīcijas
    private List<int> aktivoZivjuId = new List<int>();

    void Start()
    {
        AtjaunotSaglaboatasZivis();
    }

    // Atjauno visas saglabātās zivis no datubāzes
    private void AtjaunotSaglaboatasZivis()
    {
        var saglaboatas = DatuBaze.Instance.IegutVisasZivis();

        foreach (var zivsDB in saglaboatas)
        {
            ZivsSO zivsSO = AtrastZivsSO(zivsDB.ZivsId);
            if (zivsSO == null)
            {
                Debug.LogWarning("Nav atrasts ZivsSO ar id: " + zivsDB.ZivsId);
                continue;
            }

            Vector3 pozicija = new Vector3(zivsDB.PozicijaX, zivsDB.PozicijaY, 0f);
            IzveidotZivi(zivsSO, pozicija);
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

    // Nārsto jaunu zivi akvārijā, pamatojoties uz ZivsSO datiem
    public void IeliktZivi(ZivsSO zivsSO)
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

        // Izvēlēties nejaušu vietu akvārijā
        Vector3[] pasaulesStūri = new Vector3[4];
        akvarijs.GetWorldCorners(pasaulesStūri);
        float randX = Random.Range(pasaulesStūri[0].x, pasaulesStūri[2].x);
        float randY = Random.Range(pasaulesStūri[0].y, pasaulesStūri[2].y);
        Vector3 spawnPozicija = new Vector3(randX, randY, 0f);

        IzveidotZivi(zivsSO, spawnPozicija);
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

    // Saglabā visu zivju pozīcijas datubāzē
    public void SaglabatPozicijas()
    {
        var saraksts = new List<NopirktaZivsDB>();

        for (int i = 0; i < aktivasZivis.Count; i++)
        {
            if (aktivasZivis[i] == null) continue;

            saraksts.Add(new NopirktaZivsDB
            {
                ZivsId = aktivoZivjuId[i],
                PozicijaX = aktivasZivis[i].transform.position.x,
                PozicijaY = aktivasZivis[i].transform.position.y
            });
        }

        DatuBaze.Instance.SaglabatZivjuPozicijas(saraksts);
        Debug.Log("Saglabātas " + saraksts.Count + " zivju pozīcijas datubāzē");
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
        if (DatuBaze.Instance != null)
        {
            DatuBaze.Instance.DzestVisasZivis();
            Debug.Log("Visas zivis dzēstas no akvārija un datubāzes!");
        }
        else
        {
            Debug.LogError("DatuBaze.Instance ir null!");
        }
    }

    void OnApplicationQuit()
    {
        SaglabatPozicijas();
    }

    void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            SaglabatPozicijas();
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
