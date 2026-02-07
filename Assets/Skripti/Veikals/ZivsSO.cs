using UnityEngine;

[CreateAssetMenu(fileName = "Jauna zivs", menuName = "Zivs")]
public class ZivsSO : ScriptableObject
{
    public int id;
    public string zivsNosaukums;
    public Sprite zivsSpraits;
    [Tooltip("Neobligāts: pielāgots prefab šai zivij. Ja nav norādīts, tiks izmantots noklusētais.")]
    public GameObject zivsPrefabs;
}
