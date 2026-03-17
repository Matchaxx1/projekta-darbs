using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;

public class LideruParvaldnieks : MonoBehaviour
{
    public List<LideruVieta> lideruVieta;
    public TextMeshProUGUI atskaitesTeksts; // Pievieno lauku atskaites logam

    private DateTime nakamaAtjauninasana;
    private bool vaiLaiksIeladets = false;

    private async void Start()
    {
        await IeladetLiderus();
    }

    private void Update()
    {
        // Ja laiks ir ielādēts un pievienots attiecīgais teksta objekts
        if (vaiLaiksIeladets && atskaitesTeksts != null)
        {
            TimeSpan atlikusais = nakamaAtjauninasana - DateTime.UtcNow;

            // Ja laiks ir pagājis, varbūt vajag atjaunot datus
            if (atlikusais.TotalSeconds <= 0)
            {
                atskaitesTeksts.text = "Atjaunina datus...";
                vaiLaiksIeladets = false;
                _ = IeladetLiderus(); // Atjaunojam liderus fonā!
            }
            else
            {
                // Formātējam atlikušo laiku "Stundas:Minūtes:Sekundes"
                atskaitesTeksts.text = $"Līdz līderu tabulas atjaunināšanai {atlikusais.Hours:D2}:{atlikusais.Minutes:D2}:{atlikusais.Seconds:D2}";
            }
        }
    }

    private async Task IeladetLiderus()
    {
        if (MakonaDB.Instance == null)
        {
            Debug.LogWarning("MakonaDB.Instance nav atrasts, nevaru ieladet liderus");
            return;
        }

        var rezultati = await MakonaDB.Instance.IegutLiderus();
        
        var lideri = rezultati.lideri;
        nakamaAtjauninasana = rezultati.nakamaAtjauninasana;
        vaiLaiksIeladets = true; // Sākam laika atskaiti Update metodē
        
        for (int i = 0; i < lideruVieta.Count; i++)
        {
            if (i < lideri.Count)
            {
                lideruVieta[i].gameObject.SetActive(true);
                lideruVieta[i].IestatitLietotajaDatus(lideri[i]);
            }
            else
            {
                lideruVieta[i].gameObject.SetActive(false);
            }
        }
    }
}
