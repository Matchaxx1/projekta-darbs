using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;

public class LideruParvaldnieks : MonoBehaviour
{
    public List<LideruVieta> lideruVieta;
    public TextMeshProUGUI atskaitesTeksts; // Pievieno lauku atskaites logam

    [Header("Paneļu vadība atkarībā no lomas")]
    public GameObject tabulaPanelis;        // The actual leaderboard for registered users
    public GameObject tabulaPanelisViesim;  // The warning panel for guests

    private DateTime nakamaAtjauninasana;
    private bool vaiLaiksIeladets = false;

    private async void Start()
    {
        // Vispirms pārbauda, vai ir reģistrēts lietotājs vai viesis
        if (LietotajaLoma.PasreizejaLoma == LietotajaLoma.Loma.Viesis)
        {
            // Ja lietotājs ir viesis, rāda brīdinājumu un slēpj līderu tabulu
            if (tabulaPanelis != null) tabulaPanelis.SetActive(false);
            if (tabulaPanelisViesim != null) tabulaPanelisViesim.SetActive(true);
            return;
        }
        else
        {
            // Ja reģistrēts lietotājs, slēpj viesa paziņojumu un rāda īsto tabulu
            if (tabulaPanelis != null) tabulaPanelis.SetActive(true);
            if (tabulaPanelisViesim != null) tabulaPanelisViesim.SetActive(false);
        }

        // Tikai tad, ja nav viesis, ielādē datus no datubāzes
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
                _ = IeladetLiderus();
            }
            else
            {
                // Formātē atlikušo laiku "Stundas:Minūtes:Sekundes"
                atskaitesTeksts.text = $"Līdz līderu tabulas atjaunināšanai {atlikusais.Hours:D2}:{atlikusais.Minutes:D2}:{atlikusais.Seconds:D2}";
            }
        }
    }

    /// <summary>
    /// Asinhroni ielādē līderu datubāzes ierakstus un UI atspoguļo līderu datus.
    /// Tiek iestatīts arī nākamās atjaunināšanas laiks.
    /// </summary>
    private async Task IeladetLiderus()
    {
        // Pārbauda, vai MakonaDB instance eksistē
        if (MakonaDB.Instance == null)
        {
            Debug.LogWarning("MakonaDB.Instance nav atrasts, nevaru ieladet liderus");
            return;
        }

        // Iegūst līderu sarakstu un laiku, kad jānotiek nākamajai datu atjaunināšanai
        var rezultati = await MakonaDB.Instance.IegutLiderus();
        
        var lideri = rezultati.lideri;
        nakamaAtjauninasana = rezultati.nakamaAtjauninasana;
        vaiLaiksIeladets = true; // Sākam laika atskaiti Update metodē
        
        // Iet cauri visiem līderu UI elementiem sarakstā
        for (int i = 0; i < lideruVieta.Count; i++)
        {
            // Ja ir pieejami dati par konkrēto vietu (indeksu), parāda tos uz UI
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

    /// <summary>
    /// Debug poga, kas piespiež tabulu atjaunot datus mākonī, ignorējot 24 stundu noildzes taimeri.
    /// </summary>
    public async void PiespieduAtjaunotLideruTabulu()
    {
        if (MakonaDB.Instance == null) return;
        
        Debug.Log("Piespiedu kārtā atjauno līderu tabulu. Tas var aizņemt brīdi...");
        if (atskaitesTeksts != null) atskaitesTeksts.text = "Piespiedu atjaunošana...";

        var rezultati = await MakonaDB.Instance.IegutLiderus(true);

        var lideri = rezultati.lideri;
        nakamaAtjauninasana = rezultati.nakamaAtjauninasana;
        vaiLaiksIeladets = true;

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
        Debug.Log("Līderu tabula atjaunota piespiedu kārtā!");
    }
}
