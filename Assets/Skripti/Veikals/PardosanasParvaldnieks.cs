using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PardosanasParvaldnieks : MonoBehaviour
{
    // UI elementi pārdošanas kartītes attēlošanai
    public TMP_Text zivsNosaukums;       // Teksta lauks zivs nosaukuma parādīšanai
    public TMP_Text pardosanasMonetas;   // Teksta lauks atmaksas summas parādīšanai
    public Image zivsSpraits;            // Attēla komponents zivs spraita parādīšanai

    // Iekšējie dati par šo konkrēto pārdodamo zivi
    private int zivsId;                  // Zivs tipa identifikators datubāzē
    private int atmaksaSuma;             // Monētu daudzums, ko spēlētājs saņem par pārdošanu

    // Atsauce uz veikala pārvaldnieku, kas izpilda pārdošanas loģiku
    [SerializeField] private VeikalaParvalditajs veikalaParvalditajs;

    /// <summary>
    /// Pievieno klikšķa notikuma apstrādātāju šī objekta Button komponentei, lai visa kartīte darbojas kā poga.
    /// </summary>
    private void Start()
    {
        // Visa kartīte darbojas kā poga, pievieno klikšķa apstrādātāju
        Button pardodamaZivs = GetComponent<Button>();
        pardodamaZivs.onClick.AddListener(PiespiezotPardotPogu);
    }

    /// <summary>
    /// Aizpilda pārdošanas kartīti ar konkrētas zivs datiem.
    /// Atmaksas summa ir puse no pirkšanas cenas.
    /// </summary>
    public void Uzstadit(int id, ZivsSO zivsSO, int pirksanaCena, VeikalaParvalditajs manager)
    {
        zivsId = id;
        veikalaParvalditajs = manager;

        // Atmaksas summa ir puse no pirkšanas cenas (noapaļošana uz leju)
        atmaksaSuma = pirksanaCena / 2;

        // Aizpilda UI elementus ar zivs datiem
        zivsSpraits.sprite = zivsSO.zivsSpraits;
        zivsSpraits.preserveAspect = true;
        zivsNosaukums.text = zivsSO.zivsNosaukums;
        pardosanasMonetas.text = "+" + atmaksaSuma.ToString();
    }

    /// <summary>
    /// Klikšķa apstrādātājs, parāda apstiprinājuma logu pirms zivs pārdošanas.
    /// Ja lietotājs apstiprina, izsauc veikala pārvaldnieka pārdošanas metodi.
    /// </summary>
    public void PiespiezotPardotPogu()
    {
        ApstiprinajumaPopups.RaditPopup(
            "Pārdot zivi?",
            "Vai esi pārliecināts, ka gribi pārdot šo zivi?",
            () => veikalaParvalditajs.PardotZivi(zivsId, atmaksaSuma, gameObject)
        );
    }
}
