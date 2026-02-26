using UnityEngine;

[CreateAssetMenu(fileName = "Jauna zivs", menuName = "Zivs")]
public class ZivsSO : ScriptableObject
{
    public int id;
    public string zivsNosaukums;
    public Sprite zivsSpraits;
    public GameObject zivsPrefabs;
    public float lielums = 1f;
    public int maxDaudzums = 3;
}
