using System.Collections.Generic;
using UnityEngine;

public class AkvarijaParvaldnieks : MonoBehaviour
{
    [Header("Akvārija iestatījumi")]
    [SerializeField] private RectTransform akvarijs;

    [Header("Zivju iestatījumi")]
    [SerializeField] private GameObject zivsPrefabs;

    private List<GameObject> aktivasZivis = new List<GameObject>();

    // Nārsto jaunu zivi akvārijā, pamatojoties uz ZivsSO datiem
    public void IeliktZivi(ZivsSO zivsSO)
    {
        if (zivsSO == null)
        {
            Debug.LogWarning("AkvarijaParvaldnieks: ZivsSO ir null!");
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

        if (akvarijs == null)
        {
            Debug.LogError("AkvarijaParvaldnieks: Nav piesaistīts akvarijs!");
            return;
        }

        // Iegūt akvārija attēla robežas
        Vector3[] pasaulesStūri = new Vector3[4];
        akvarijs.GetWorldCorners(pasaulesStūri);

        // Aprēķināt robežas no RectTransform stūriem
        float minX = pasaulesStūri[0].x;
        float maxX = pasaulesStūri[2].x;
        float minY = pasaulesStūri[0].y;
        float maxY = pasaulesStūri[2].y;

        // Izvēlēties nejaušu vietu akvārijā
        float randX = Random.Range(minX, maxX);
        float randY = Random.Range(minY, maxY);
        Vector3 spawnPozicija = new Vector3(randX, randY, 0f);

        // Izveidot zivi ( varbut janonem quaternion )
        GameObject jaunaZivs = Instantiate(prefabs, spawnPozicija, Quaternion.identity, transform); 
        jaunaZivs.name = zivsSO.zivsNosaukums;

        // Uzstādīt spraitu, ja prefabam ir SpriteRenderer
        SpriteRenderer sr = jaunaZivs.GetComponent<SpriteRenderer>();
        if (sr != null && zivsSO.zivsSpraits != null)
        {
            sr.sprite = zivsSO.zivsSpraits;
        }

        aktivasZivis.Add(jaunaZivs);
        Debug.Log("Zivs '" + zivsSO.zivsNosaukums + "' pievienota akvārijam!");
    }

    // Atgriež aktīvo zivju skaitu akvārijā
    public int IegutZivjuSkaitu()
    {
        // Noņem iznīcinātos objektus
        aktivasZivis.RemoveAll(z => z == null);
        return aktivasZivis.Count;
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
