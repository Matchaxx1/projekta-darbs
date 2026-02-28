using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VeikalaParvalditajs : MonoBehaviour
{
    [Header("Pirksanas cilne")]
    [SerializeField] private List<PrecuSaraksts> precuSaraksts;  // Visu pieejamo zivju precu saraksts
    [SerializeField] private PrecesVieta[] precesVieta;           // UI kartīšu vietas pirkšanas cilnē

    [Header("Pardosanas cilne")]
    [SerializeField] private Transform pardosanasKonteiners;      // Konteiners, kurā dinamiski tiek pievienotas pārdošanas kartītes
    [SerializeField] private GameObject pardosanasVietaPrefabs;    // Pārdošanas kartītes prefabs

    [Header("Kopigi")]
    [SerializeField] private SpeletajaProgress speletajaProgress;      // Spēlētāja progresa atsauce monētu atskaitīšanai
    [SerializeField] private AkvarijaParvaldnieks akvarijaParvaldnieks; // Akvārija pārvaldnieks zivju pievienošanai/noņemšanai

    [Header("Kopējs zivju limits")]
    [Tooltip("Maksimālais kopējais zivju skaits akvārijā.")]
    [SerializeField] private int maxKopejaisZivjuSkaits = 10;  // Maksimālais zivju skaits akvārijā
    [Tooltip("Panelis, kas tiek rādīts, kad akvārijs ir pilns.")]
    [SerializeField] private GameObject pardotZivisPanelis;      // UI panelis, kas tiek parādīts, kad akvārijs ir pilns
    // Publiskais īpašums maksimālā zivju skaita nolasīšanai no citiem skriptiem
    public int MaxKopejaisZivjuSkaits => maxKopejaisZivjuSkaits;
    /// <summary>
    /// Inicializācijas laikā piešķir unikālu ID katrai zivij.
    /// </summary>
    private void Awake()
    {
        // Piešķir ID katrai zivij pēc pozīcijas sarakstā
        for (int i = 0; i < precuSaraksts.Count; i++)
        {
            if (precuSaraksts[i].ZivsSO != null)
                precuSaraksts[i].ZivsSO.id = i + 1;
        }
    }

    /// <summary>
    /// Aizpilda precu kartītes un atver pirkšanas cilni.
    /// </summary>
    private void Start()
    {
        pievienotVeikalam();
        AtvertVeikaluPirkt();
    }


    /// <summary>
    /// Atver pirkšanas cilni, paneļu pārslēgšanu veic UIPārvaldnieks.
    /// </summary>
    public void AtvertVeikaluPirkt()
    {
        // Paneļu parādīšanas kontrole notiek UIPārvaldnieks skriptā
    }

    /// <summary>
    /// Aizpilda veikala pirkšanas kartītes ar datiem no precu saraksta.
    /// Katrai precei iestata attēlu, nosaukumu, cenu un zivs ID.
    /// </summary>
    public void pievienotVeikalam()
    {
        for (int i = 0; i < precuSaraksts.Count && i < precesVieta.Length; i++)
        {
            PrecuSaraksts veikalaPrece = precuSaraksts[i];
            if (veikalaPrece == null || veikalaPrece.ZivsSO == null)
            {
                Debug.LogWarning("[Veikals] precuSaraksts[" + i + "] vai tā ZivsSO ir null — karte slēgta.");
                if (precesVieta[i] != null)
                    precesVieta[i].gameObject.SetActive(false);
                continue;
            }
            int zivsId = i + 1;
            veikalaPrece.ZivsSO.id = zivsId;
            precesVieta[i].Uzstadit(veikalaPrece.ZivsSO, veikalaPrece.cena, zivsId);
            precesVieta[i].gameObject.SetActive(true);
        }

        for (int i = precuSaraksts.Count; i < precesVieta.Length; i++)
        {
            if (precesVieta[i] != null)
                precesVieta[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Mēģina nopirkt zivi, pārbauda pieejamību, monētu pietiekamību un izpilda pirkšanas darbību ar datubāzes saglabāšanu un akvārija atjaunošanu.
    /// </summary>
    public async void meginatPirkt(ZivsSO zivsSO, int cena, int zivsId)
    {
        if (zivsSO == null) return;
        if (zivsId <= 0)
        {
            Debug.LogError("Nederigs zivs ID: " + zivsSO.zivsNosaukums);
            return;
        }

        if (DatuParvaldnieks.Instance == null)
        {
            Debug.LogError("DatuParvaldnieks.Instance ir null!");
            return;
        }

        // Pārbauda, vai spēlētājs vēl var pirkt šī tipa zivi (nav sasniegts limits)
        bool varPirkt = await DatuParvaldnieks.Instance.VaiVarPirkt(zivsId, zivsSO.maxDaudzums);
        if (this == null) return;
        if (!varPirkt)
        {
            Debug.Log("Nevar nopirkt vairāk no šī tipa zivs! Limits: " + zivsSO.maxDaudzums);
            // Ja limits sasniegts, atjauno kartītes, lai attēlo "izpirkts" stāvokli
            AtjaunotVeikalaKartes();
            return;
        }

        if (speletajaProgress == null)
        {
            Debug.LogError("speletajaProgress ir null!");
            return;
        }

        // Pārbauda, vai spēlētājam ir pietiekami daudz monētu pirkšanai
        if (speletajaProgress.monetas >= cena)
        {
            // Atskaita monētas no spēlētāja atlikuma
            speletajaProgress.monetas -= cena;

            if (speletajaProgress.monetuSkaitsTMP != null)
                speletajaProgress.monetuSkaitsTMP.text = speletajaProgress.monetas.ToString();

            if (speletajaProgress.monetuSkaitsTMP2 != null)
                speletajaProgress.monetuSkaitsTMP2.text = speletajaProgress.monetas.ToString();

            // Saglabā atjaunoto progresu datubāzē
            DatuParvaldnieks.Instance?.SaglabatProgresu(speletajaProgress.soli, speletajaProgress.monetas, speletajaProgress.kopejasMonetas);

            // Iestata zivs ID un pievieno to akvārijam
            zivsSO.id = zivsId;

            if (akvarijaParvaldnieks != null)
                akvarijaParvaldnieks.IeliktZivi(zivsSO);

            // Parāda "pilns akvārijs" paneli, ja tagad ir sasniegts zivju limits
            if (akvarijaParvaldnieks != null && pardotZivisPanelis != null)
            {
                int pasuSkaits = akvarijaParvaldnieks.IegutZivjuSkaitu();
                if (pasuSkaits >= maxKopejaisZivjuSkaits)
                    pardotZivisPanelis.SetActive(true);
            }

            // Atjauno veikala kartītes, lai atspoguļotu jauno nopirkto skaitu
            AtjaunotVeikalaKartes();
        }
        else
        {
            // Ja monētu nepietiek, izvada paziņojumu konsolē
            Debug.Log("Nav pietiekami daudz monetu! Vajag: " + cena + ", ir: " + speletajaProgress.monetas);
        }
    }


    /// <summary>
    /// Notīra vecas kartītes un ielādē aktuālās no datubāzes.
    /// </summary>
    public void AtvertVeikaluPardot()
    {
        TiritPardosanasKartes();
        IeladesPardosanasKartes();
    }

    /// <summary>
    /// Notīra visas iepriekšējās pārdošanas kartītes no konteinerā.
    /// </summary>
    private void TiritPardosanasKartes()
    {
        if (pardosanasKonteiners == null) return;
        foreach (Transform berns in pardosanasKonteiners)
            Destroy(berns.gameObject);
    }

    /// <summary>
    /// Ielādē visas nopirktās zivis no datubāzes un izveido pārdošanas kartītes katrai zivij.
    /// </summary>
    private async void IeladesPardosanasKartes()
    {
        if (DatuParvaldnieks.Instance == null)
        {
            Debug.LogError("[Pardosana] DatuParvaldnieks.Instance ir null!");
            return;
        }
        if (pardosanasVietaPrefabs == null)
        {
            Debug.LogError("[Pardosana] pardosanasVietaPrefabs nav iestatits!");
            return;
        }
        if (pardosanasKonteiners == null)
        {
            Debug.LogError("[Pardosana] pardosanasKonteiners nav iestatits!");
            return;
        }

        List<NopirktaZivsDB> zivis;
        try
        {
            zivis = await DatuParvaldnieks.Instance.IegutVisasZivis();
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Pardosana] DB kluda: " + e.Message);
            return;
        }

        if (this == null) return;

        if (zivis == null || zivis.Count == 0)
        {
            Debug.Log("[Pardosana] Nav nevienas zivs pardosanai.");
            return;
        }

        Debug.Log("[Pardosana] Atrasti " + zivis.Count + " ieraksti DB. precuSaraksts.Count=" + precuSaraksts.Count);

        foreach (var zivsDB in zivis)
        {
            PrecuSaraksts prece = AtrastPreciPecId(zivsDB.ZivsId);
            if (prece == null || prece.ZivsSO == null)
            {
                Debug.LogWarning("[Pardosana] Nav atrasta prece ar ID=" + zivsDB.ZivsId);
                continue;
            }

            Debug.Log("[Pardosana] Veido karti: " + prece.ZivsSO.zivsNosaukums
                + " | ID=" + zivsDB.ZivsId + " | Cena=" + prece.cena);
            PievienotPardosanasKarti(zivsDB.ZivsId, prece.ZivsSO, prece.cena);
        }
    }

    /// <summary>
    /// Izveido jaunu pārdošanas kartīti un pievieno to konteineram.
    /// </summary>
    private void PievienotPardosanasKarti(int zivsId, ZivsSO zivsSO, int pirksanaCena)
    {
        if (pardosanasVietaPrefabs == null || pardosanasKonteiners == null) return;

        GameObject karte = Instantiate(pardosanasVietaPrefabs, pardosanasKonteiners);
        PardosanasParvaldnieks komp = karte.GetComponent<PardosanasParvaldnieks>();
        if (komp != null)
        {
            komp.Uzstadit(zivsId, zivsSO, pirksanaCena, this);
        }
        else
        {
            Debug.LogError("[Pardosana] Prefabam '" + pardosanasVietaPrefabs.name
                + "' nav PardosanasParvaldnieks komponente!");
        }
    }

    /// <summary>
    /// Pievieno monētas spēlētājam, noņem zivi no akvārija, dzēš no datubāzes un iznīcina pārdošanas kartīti.
    /// </summary>
    public async void PardotZivi(int zivsId, int atmaksaSuma, GameObject karte)
    {
        // Uzreiz deaktivē kartīti, lai novērstu dubultu pārdošanu, kamēr DB operācija darbojas
        if (karte != null)
        {
            Button b = karte.GetComponent<Button>();
            if (b != null) b.interactable = false;
            karte.SetActive(false);
        }

        // Pievieno pārdošanas monētas spēlētāja atlikumam un kopējai statistikai
        if (speletajaProgress != null)
        {
            speletajaProgress.monetas += atmaksaSuma;
            speletajaProgress.kopejasMonetas += atmaksaSuma;

            if (speletajaProgress.monetuSkaitsTMP != null)
                speletajaProgress.monetuSkaitsTMP.text = speletajaProgress.monetas.ToString();

            if (speletajaProgress.monetuSkaitsTMP2 != null)
                speletajaProgress.monetuSkaitsTMP2.text = speletajaProgress.monetas.ToString();

            DatuParvaldnieks.Instance?.SaglabatProgresu(speletajaProgress.soli, speletajaProgress.monetas, speletajaProgress.kopejasMonetas);
        }

        // Noņem zivi no akvārija un dzēš no datubāzes un gaida, līdz operācija beidzas
        if (akvarijaParvaldnieks != null)
            await akvarijaParvaldnieks.PardotZivi(zivsId);

        // Iznīcina pārdošanas kartīti no UI
        if (karte != null)
            Destroy(karte);

        // Paslēpj "pilns akvārijs" paneli, ja akvārijs vairs nav pilns
        if (akvarijaParvaldnieks != null && pardotZivisPanelis != null)
        {
            int pasuSkaits = akvarijaParvaldnieks.IegutZivjuSkaitu();
            if (pasuSkaits < maxKopejaisZivjuSkaits)
                pardotZivisPanelis.SetActive(false);
        }

        // Atjauno pirkšanas cilnes kartītes (nopirkto skaitu un izpirkts stāvokli)
        AtjaunotVeikalaKartes();

        Debug.Log("[Pardosana] Pardota zivs ID=" + zivsId + " | Atmaksa=" + atmaksaSuma);
    }


    /// <summary>
    /// Atjauno visas veikala pirkšanas kartītes, nopirkto skaitu un izpirkts stāvokli.
    /// </summary>
    public void AtjaunotVeikalaKartes()
    {
        foreach (var vieta in precesVieta)
        {
            if (vieta != null && vieta.gameObject.activeSelf)
                vieta.AtjaunotNopirktoSkaitu();
        }
    }

    /// <summary>
    /// Meklē ZivsSO datu objektu pēc zivs ID.
    /// Vispirms meklē pēc indeksa (primāri), tad pēc ZivsSO.id lauka (rezervē).
    /// </summary>
    public ZivsSO IegutZivsSO(int zivsId)
    {
        int indekss = zivsId - 1;
        if (indekss >= 0 && indekss < precuSaraksts.Count && precuSaraksts[indekss].ZivsSO != null)
            return precuSaraksts[indekss].ZivsSO;
        foreach (var p in precuSaraksts)
            if (p.ZivsSO != null && p.ZivsSO.id == zivsId)
                return p.ZivsSO;
        Debug.LogWarning("[IegutZivsSO] Nav atrasts ZivsSO ar ID=" + zivsId);
        return null;
    }

    /// <summary>
    /// Meklē preci pēc zivs ID. Primāri izmanto indeksu (ID - 1), jo ID tiek piešķirti Awake() metodē sākot no 1.
    /// Ja indeksa meklēšana nespēj atrast, pārbauda ZivsSO.id lauku.
    /// </summary>
    private PrecuSaraksts AtrastPreciPecId(int zivsId)
    {
        // Primārais meklēšanas veids, pēc indeksa (DB glabā pozīcija + 1)
        int indekss = zivsId - 1;
        if (indekss >= 0 && indekss < precuSaraksts.Count && precuSaraksts[indekss].ZivsSO != null)
            return precuSaraksts[indekss];

        // Rezerves meklēšanas veids, pārbauda ZivsSO.id lauku (gadījumam, ja ID nesakrīt ar pozīciju)
        foreach (var p in precuSaraksts)
            if (p.ZivsSO != null && p.ZivsSO.id == zivsId)
                return p;

        // Nekas nav atrasts, tadizdrukā pieejamos ID diagnostikai
        string pieejamieId = "";
        foreach (var p in precuSaraksts)
            if (p.ZivsSO != null) pieejamieId += p.ZivsSO.zivsNosaukums + "(id=" + p.ZivsSO.id + ") ";
        Debug.LogError("[Pardosana] Nav atrasta prece ar ID=" + zivsId + ". Pieejamie: " + pieejamieId);
        return null;
    }
}

/// <summary>
/// Preces datu klase, satur atsauci uz zivs ScriptableObject un tās cenu.
/// Tiek izmantota veikala precu sarakstā.
/// </summary>
[System.Serializable]
public class PrecuSaraksts
{
    public ZivsSO ZivsSO; // Atsauce uz zivs datu objektu
    public int cena;      // Zivs pirkšanas cena monētās
}
