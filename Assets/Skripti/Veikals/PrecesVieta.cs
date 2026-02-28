using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PrecesVieta : MonoBehaviour
{
    // Zivs datu objekts (ScriptableObject), kas satur šīs kartītes zivs informāciju
    public ZivsSO ZivsSO;

    // UI elementi kartītes attēlošanai
    public TMP_Text zivsNosaukums;       // Teksta lauks zivs nosaukuma parādīšanai
    public TMP_Text zivsCena;            // Teksta lauks cenas parādīšanai
    public Image zivsSpraits;            // Attēla komponents zivs spraita parādīšanai

    [Header("Pirkums stavoklis")]
    [SerializeField] private TMP_Text piederTeksts;    // Teksts "Pieder: X/Y" nopirkto skaita parādīšanai
    [SerializeField] private GameObject izpirktsEkrans; // Pārklājums, kas rādās, kad zivs tips ir pilnībā izpirkts
    [SerializeField] private Button pirktPoga;          // Pirkšanas poga (tiek deaktivēta, kad izpirkts)

    // Iekšējie dati
    private int cena;       // Šīs zivs pirkšanas cena monētās
    private int zivsId;     // Zivs tipa identifikators datubāzē

    // Atsauce uz veikala pārvaldnieku, kas izpilda pirkšanas loģiku
    [SerializeField] private VeikalaParvalditajs veikalaParvalditajs;

    /// <summary>
    /// Inicializācijas laikā automātiski meklē VeikalaParvalditajs, ja tas nav norādīts inspektorā.
    /// </summary>
    void Awake()
    {
        if (veikalaParvalditajs == null)
            veikalaParvalditajs = FindFirstObjectByType<VeikalaParvalditajs>();
    }

    /// <summary>
    /// Iestata kartītes datus, aizpilda UI elementus ar zivs informāciju un sākotnējo cenu.
    /// Ja ZivsSO ir tukšs, kartīte tiek paslēpta.
    /// </summary>
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
        // Aizpilda UI elementus ar zivs datiem
        zivsSpraits.sprite = ZivsSO.zivsSpraits;
        zivsSpraits.raycastTarget = false;
        zivsNosaukums.text = ZivsSO.zivsNosaukums;
        zivsCena.text = cena.ToString();

        // Slēpj "izpirkts" pārklājumu pēc noklusējuma
        if (izpirktsEkrans != null) izpirktsEkrans.SetActive(false);

        // Pieprasītīs no DB, cik šī tipa zivis jau ir nopirktas
        AtjaunotNopirktoSkaitu();
    }

    /// <summary>
    /// Ielādē pašreizējo nopirkto skaitu no DB un atjaunina UI.
    /// Ja zivs ir izspirkta, tad parāda izpirkts pārklājumu.
    /// </summary>
    public async void AtjaunotNopirktoSkaitu()
    {
        if (ZivsSO == null || DatuParvaldnieks.Instance == null) return;

        // Iegūst nopirkto skaitu no datubāzes
        int skaits = await DatuParvaldnieks.Instance.IegutNopirktoSkaitu(zivsId);
        if (this == null) return;
        int max = ZivsSO.maxDaudzums;

        // Atjauno "pieder" tekstu ar pašreizējo un maksimālo skaitu

        if (piederTeksts != null)
            piederTeksts.text = "Pieder: " + skaits + "/" + max;

        // Nosaka, vai zivs tips ir pilnībā izpirkts
        bool izpardots = skaits >= max;

        // Parāda vai paslēpj "izpirkts" pārklājumu

        if (izpirktsEkrans != null)
            izpirktsEkrans.SetActive(izpardots);

        // Deaktivē pirkšanas pogu, ja zivs tips ir izpirkts
        if (pirktPoga != null)
            pirktPoga.interactable = !izpardots;
    }

    /// <summary>
    /// Pirkšanas pogas klikšķa apstrādātājs, parāda apstiprinājuma logu pirms pirkšanas.
    /// Ja lietotājs apstiprina, izsauc veikala pārvaldnieka pirkšanas metodi.
    /// </summary>
    public void PiespiezotPirktPogu()
    {
        ApstiprinajumaPopups.RaditPopup(
            "Pirkt zivi?",
            "Vai esi pārliecināts, ka gribi pirkt šo zivi?",
            () => veikalaParvalditajs.meginatPirkt(ZivsSO, cena, zivsId)
        );
    }
}
