using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PrecesVieta : MonoBehaviour
{
    public ZivsSO ZivsSO;
    public TMP_Text zivsNosaukums;
    public TMP_Text zivsCena;
    public Image zivsSpraits;

    private int cena;
    private int zivsId;

    [SerializeField] private VeikalaParvalditajs veikalaParvalditajs;
    
    public void Uzstadit(ZivsSO newZivsSO, int cena, int zivsId)
    {
        // Aizpilda veikala lodziņu ar visu zivs doto informāciju.
        ZivsSO = newZivsSO;
        this.cena = cena;
        this.zivsId = zivsId;
        zivsSpraits.sprite = ZivsSO.zivsSpraits;
        zivsNosaukums.text = ZivsSO.zivsNosaukums;
        zivsCena.text = cena.ToString();
    }

    public void PiespiezotPirktPogu()
    {
        veikalaParvalditajs.meginatPirkt(ZivsSO, cena, zivsId);
    }
}
