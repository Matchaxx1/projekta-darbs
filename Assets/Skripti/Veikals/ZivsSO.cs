using UnityEngine;

[CreateAssetMenu(fileName = "Jauna zivs", menuName = "Zivs")]
public class ZivsSO : ScriptableObject
{
    public int id;                   // Unikāls zivs identifikators (tiek piešķirts automātiski pēc pozīcijas veikala sarakstā)
    public string zivsNosaukums;     // Zivs nosaukums, kas tiek attēlots veikalā un akvārijā
    public Sprite zivsSpraits;       // Zivs spraits, kas tiek rādīts UI elementos
    public GameObject zivsPrefabs;   // Zivs prefabs, kas tiek izvietots akvārijā
    public float lielums = 1f;       // Zivs izmēra koeficients
    public int maxDaudzums = 3;      // Maksimālais šī tipa zivju skaits, ko spēlētājs var nopirkt
}
