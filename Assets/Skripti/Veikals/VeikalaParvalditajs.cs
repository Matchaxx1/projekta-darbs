using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VeikalaParvalditajs : MonoBehaviour
{
    [Header("Pirksanas cilne")]
    [SerializeField] private List<PrecuSaraksts> precuSaraksts;
    [SerializeField] private PrecesVieta[] precesVieta;

    [Header("Pardosanas cilne")]
    [SerializeField] private Transform pardosanasKonteiners;
    [SerializeField] private GameObject pardosanasVietaPrefabs;

    [Header("Kopigi")]
    [SerializeField] private SpeletajaProgress speletajaProgress;
    [SerializeField] private AkvarijaParvaldnieks akvarijaParvaldnieks;

    [Header("Kopējs zivju limits")]
    [Tooltip("Maksimālais kopējais zivju skaits akvārijā.")]
    [SerializeField] private int maxKopejaisZivjuSkaits = 10;
    [Tooltip("Panelis, kas tiek rādīts, kad akvārijs ir pilns.")]
    [SerializeField] private GameObject pardotZivisPanelis;
    public int MaxKopejaisZivjuSkaits => maxKopejaisZivjuSkaits;
    private void Awake()
    {
        // Pieskir ID katrai zivij pec pozicijas saraksta (1, 2, 3...)
        for (int i = 0; i < precuSaraksts.Count; i++)
        {
            if (precuSaraksts[i].ZivsSO != null)
                precuSaraksts[i].ZivsSO.id = i + 1;
        }
    }

    private void Start()
    {
        pievienotVeikalam();
        AtvertVeikaluPirkt();
    }

    // ===== PIRKSANAS CILNE =====

    public void AtvertVeikaluPirkt()
    {
        // Panelu parades kontrole ir VeikalaUIParvaldnieks
    }

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

        bool varPirkt = await DatuParvaldnieks.Instance.VaiVarPirkt(zivsId, zivsSO.maxDaudzums);
        if (this == null) return;
        if (!varPirkt)
        {
            Debug.Log("Nevar nopirkt vairāk no šī tipa zivs! Limits: " + zivsSO.maxDaudzums);
            AtjaunotVeikalaKartes();
            return;
        }

        if (speletajaProgress == null)
        {
            Debug.LogError("speletajaProgress ir null!");
            return;
        }

        if (speletajaProgress.monetas >= cena)
        {
            speletajaProgress.monetas -= cena;

            if (speletajaProgress.monetuSkaitsTMP != null)
                speletajaProgress.monetuSkaitsTMP.text = speletajaProgress.monetas.ToString();

            if (speletajaProgress.monetuSkaitsTMP2 != null)
                speletajaProgress.monetuSkaitsTMP2.text = speletajaProgress.monetas.ToString();

            DatuParvaldnieks.Instance?.SaglabatProgresu(speletajaProgress.soli, speletajaProgress.monetas, speletajaProgress.kopejasMonetas);

            zivsSO.id = zivsId;

            if (akvarijaParvaldnieks != null)
                akvarijaParvaldnieks.IeliktZivi(zivsSO);

            // Parades "pilns akvārijs" paneli, ja tagad ir sasniegts limits
            if (akvarijaParvaldnieks != null && pardotZivisPanelis != null)
            {
                int pasuSkaits = akvarijaParvaldnieks.IegutZivjuSkaitu();
                if (pasuSkaits >= maxKopejaisZivjuSkaits)
                    pardotZivisPanelis.SetActive(true);
            }

            AtjaunotVeikalaKartes();
        }
        else
        {
            Debug.Log("Nav pietiekami daudz monetu! Vajag: " + cena + ", ir: " + speletajaProgress.monetas);
        }
    }

    // ===== PARDOSANAS CILNE =====

    // Izsauc VeikalaUIParvaldnieks (vai tieši) - notira un pielada kartites
    public void AtvertVeikaluPardot()
    {
        TiritPardosanasKartes();
        IeladesPardosanasKartes();
    }

    // Notira visas pardosanas kartites no konteiner
    private void TiritPardosanasKartes()
    {
        if (pardosanasKonteiners == null) return;
        foreach (Transform berns in pardosanasKonteiners)
            Destroy(berns.gameObject);
    }

    // Ielade visas nopirktas zivis no DB un izveido kartites
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

    // Izveido jaunu pardosanas kartiti un pievieno konteinera
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

    // Pardod zivi: pievieno monetas, nonem no akvarija, iznicina kartiti
    public async void PardotZivi(int zivsId, int atmaksaSuma, GameObject karte)
    {
        // heti disable card to prevent double-sell while DB operation runs
        if (karte != null)
        {
            Button b = karte.GetComponent<Button>();
            if (b != null) b.interactable = false;
            karte.SetActive(false);
        }

        // Pievieno monetas
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

        // Nonem zivi no akvarija un DB — await, lai DB dzēšana pabeigtos pirms UI atjaunojuma
        if (akvarijaParvaldnieks != null)
            await akvarijaParvaldnieks.PardotZivi(zivsId);

        // Iznicina kartiti no UI
        if (karte != null)
            Destroy(karte);

        // Paslep "pilns akvārijs" paneli, ja tas vairs nav pilns
        if (akvarijaParvaldnieks != null && pardotZivisPanelis != null)
        {
            int pasuSkaits = akvarijaParvaldnieks.IegutZivjuSkaitu();
            if (pasuSkaits < maxKopejaisZivjuSkaits)
                pardotZivisPanelis.SetActive(false);
        }

        // Atjaunina pirksanas cilnes kartes (owned skaits / sold out stavoklis)
        AtjaunotVeikalaKartes();

        Debug.Log("[Pardosana] Pardota zivs ID=" + zivsId + " | Atmaksa=" + atmaksaSuma);
    }

    // ===== PALIGMETODE =====

    /// <summary>
    /// Atjaunina visas veikala kartes (owned skaitu un sold out stavokli).
    /// </summary>
    public void AtjaunotVeikalaKartes()
    {
        foreach (var vieta in precesVieta)
        {
            if (vieta != null && vieta.gameObject.activeSelf)
                vieta.AtjaunotNopirktoSkaitu();
        }
    }

    // Atrod ZivsSO pec ID — izmanto AkvarijaParvaldnieks
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

    // Atrod preci pec zivs ID
    // DB vienmēr glabā pozīciju + 1 (pieskirts Awake), tāpēc indekss ir primārais veids
    private PrecuSaraksts AtrastPreciPecId(int zivsId)
    {
        // Primārais: indekss (DB vienmēr glabā pozīcija+1)
        int indekss = zivsId - 1;
        if (indekss >= 0 && indekss < precuSaraksts.Count && precuSaraksts[indekss].ZivsSO != null)
            return precuSaraksts[indekss];

        // Rezerves: mekle pec ZivsSO.id (gadijumam, ja ID nesaskan ar poziciju)
        foreach (var p in precuSaraksts)
            if (p.ZivsSO != null && p.ZivsSO.id == zivsId)
                return p;

        // Nekas nav atrasts
        string pieejamieId = "";
        foreach (var p in precuSaraksts)
            if (p.ZivsSO != null) pieejamieId += p.ZivsSO.zivsNosaukums + "(id=" + p.ZivsSO.id + ") ";
        Debug.LogError("[Pardosana] Nav atrasta prece ar ID=" + zivsId + ". Pieejamie: " + pieejamieId);
        return null;
    }
}

[System.Serializable]
public class PrecuSaraksts
{
    public ZivsSO ZivsSO;
    public int cena;
}
