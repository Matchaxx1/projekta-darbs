using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PardosanasParvaldnieks : MonoBehaviour
{
    public TMP_Text zivsNosaukums;
    public TMP_Text pardosanasMonetas;
    public Image zivsSpraits;

    private int zivsId;
    private int atmaksaSuma;

    [SerializeField] private VeikalaParvalditajs veikalaParvalditajs;

    private void Start()
    {
        // Visa karte ir poga
        Button pardodamaZivs = GetComponent<Button>();
        pardodamaZivs.onClick.AddListener(PiespiezotPardotPogu);
    }

    // Aizpilda pārdošanas kartīti ar zivs informāciju
    public void Uzstadit(int id, ZivsSO zivsSO, int pirksanaCena, VeikalaParvalditajs manager)
    {
        zivsId = id;
        veikalaParvalditajs = manager;
        atmaksaSuma = pirksanaCena / 2;

        zivsSpraits.sprite = zivsSO.zivsSpraits;
        zivsNosaukums.text = zivsSO.zivsNosaukums;
        pardosanasMonetas.text = "+" + atmaksaSuma.ToString();
    }

    public void PiespiezotPardotPogu()
    {
        veikalaParvalditajs.PardotZivi(zivsId, atmaksaSuma, gameObject);
    }
}
