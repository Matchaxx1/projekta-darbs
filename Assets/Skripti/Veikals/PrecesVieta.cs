using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PrecesVieta : MonoBehaviour
{
    public ZivsSO ZivsSO;
    public TMP_Text zivsNosaukums;
    public TMP_Text zivsCena;
    public Image zivsSpraits;

    [Header("Pirkums stavoklis")]
    [SerializeField] private TMP_Text piederTeksts;
    [SerializeField] private GameObject izpirktsEkrans;
    [SerializeField] private Button pirktPoga;

    private int cena;
    private int zivsId;

    [SerializeField] private VeikalaParvalditajs veikalaParvalditajs;

    void Awake()
    {
        if (veikalaParvalditajs == null)
            veikalaParvalditajs = FindFirstObjectByType<VeikalaParvalditajs>();
    }

    public void Uzstadit(ZivsSO newZivsSO, int cena, int zivsId)
    {
        if (newZivsSO == null)
        {
            Debug.LogWarning("PrecesVieta.Uzstadit: ZivsSO ir null, karte tiek slēgta.");
            gameObject.SetActive(false);
            return;
        }

        ZivsSO = newZivsSO;
        this.cena = cena;
        this.zivsId = zivsId;
        zivsSpraits.sprite = ZivsSO.zivsSpraits;
        zivsSpraits.raycastTarget = false;
        //zivsSpraits.preserveAspect = true;
        zivsNosaukums.text = ZivsSO.zivsNosaukums;
        zivsCena.text = cena.ToString();

        // Slēpj sold out pēc noklusējuma, atjaunos async
        if (izpirktsEkrans != null) izpirktsEkrans.SetActive(false);

        AtjaunotNopirktoSkaitu();
    }

    /// <summary>
    /// Ielādē pašreizējo nopirkto skaitu no DB un atjaunina UI.
    /// </summary>
    public async void AtjaunotNopirktoSkaitu()
    {
        if (ZivsSO == null || DatuParvaldnieks.Instance == null) return;

        int skaits = await DatuParvaldnieks.Instance.IegutNopirktoSkaitu(zivsId);
        if (this == null) return; // Objekts iznīcīnāts skatīlas mainīšanas laikā
        int max = ZivsSO.maxDaudzums;

        if (piederTeksts
 != null)
            piederTeksts
    .text = "Pieder: " + skaits + "/" + max;

        bool izpardots = skaits >= max;

        if (izpirktsEkrans != null)
            izpirktsEkrans.SetActive(izpardots);

        if (pirktPoga != null)
            pirktPoga.interactable = !izpardots;
    }

    public void PiespiezotPirktPogu()
    {
        ApstiprinajumaPopups.RaditPopup(
            "Pirkt zivi?",
            "Vai esi pārliecināts, ka gribi pirkt šo zivi?",
            () => veikalaParvalditajs.meginatPirkt(ZivsSO, cena, zivsId)
        );
    }
}
