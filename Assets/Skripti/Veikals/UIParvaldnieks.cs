using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIParvaldnieks : MonoBehaviour
{
    // Atsauces uz galvenajiem UI paneļiem
    public GameObject veikalaUI;             // Veikala panelis (pirkšana un pārdošana)
    public GameObject kontaUI;               // Profila/konta panelis
    public GameObject pardotCanvas;          // Pārdošanas cilnes Canvas
    public GameObject pirktCanvas;           // Pirkšanas cilnes Canvas
    public VeikalaParvalditajs veikalaParvalditajs;  // Veikala pārvaldnieks precu ielādei
    public GameObject izveleUI;              // Sākotnējās izvēles panelis (viesis/reģistrēties)
    public GameObject piesliegsanasPoga;     // Pieslēgšanās poga (redzama tikai viesiem)
    public GameObject registracijasPoga;     // Reģistrācijas poga (redzama tikai viesiem)
    public ProfilaInformacija profilaInfo;   // Profila informācijas komponents
    public GameObject pieslegtiesUI;         // Pieslēgšanās UI panelis

    /// <summary>
    /// Ja lietotājam nav izvēlēta loma, parāda izvēles ekrānu.
    /// Ja loma jau ir iestatīta, paslēpj izvēles ekrānu un parāda spēles UI.
    /// </summary>
    private void Start()
    {
        // Parāda izvēles UI tikai tad, ja lietotājam vēl nav izvēlēta loma
        if (izveleUI != null)
        {
            if (LietotajaLoma.PasreizejaLoma == LietotajaLoma.Loma.Nav)
            {
                // Rāda izvēles ekrānu ar pilnīgu redzamību un interaktivitāti
                izveleUI.GetComponent<CanvasGroup>().alpha = 1f;
                izveleUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
                izveleUI.GetComponent<CanvasGroup>().interactable = true;

                // Slēpj spēles UI paneļus, lai lietotājs nevarētu piekļūt veikalam vai profilam
                SlepjSpelesUI();
            }
            else
            {
                // Paslēpj izvēles ekrānu, jo loma jau ir iestatīta
                izveleUI.GetComponent<CanvasGroup>().alpha = 0f;
                izveleUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
                izveleUI.GetComponent<CanvasGroup>().interactable = false;
            }
        }

        // Pārvalda pieslēgšanās un reģistrācijas pogu redzamību, balstoties uz lietotāja lomu
        AuthPogasParvalde();
    }

    /// <summary>
    /// Slēpj visus spēles UI elementus, iestatot CanvasGroup parametrus uz neredzamu un neinteraktīvu.
    /// Tiek izmantots, kad lietotājam vēl nav izvēlēta loma.
    /// </summary>
    private void SlepjSpelesUI()
    {
        // Lokāla palīgfunkcija, kas paslēpj vienu GameObject caur tā CanvasGroup
        void Slepj(GameObject objekts)
        {
            if (objekts == null) return;
            CanvasGroup canvasGroup = objekts.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        // Paslēpj veikala, profila un pieslēgšanās paneļus
        Slepj(veikalaUI);
        Slepj(kontaUI);
        Slepj(pieslegtiesUI);
    }
    
    /// <summary>
    /// Parāda vai paslēpj pieslēgšanās un reģistrācijas pogas.
    /// Šīs pogas ir redzamas tikai viesiem, lai tie varētu pieslēgties vai reģistrēties.
    /// </summary>
    public void AuthPogasParvalde()
    {
        bool irViesis = LietotajaLoma.IrViesis();
        
        if (piesliegsanasPoga != null)
        {
            piesliegsanasPoga.SetActive(irViesis);
        }
        
        if (registracijasPoga != null)
        {
            registracijasPoga.SetActive(irViesis);
        }
    }

    /// <summary>
    /// Atver veikala paneli un pēc noklusējuma parāda pirkšanas cilni.
    /// </summary>
    public void AtvertVeikalu()
    {
        if (veikalaUI != null)
        {
            veikalaUI.GetComponent<CanvasGroup>().alpha = 1f;
            veikalaUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
            veikalaUI.GetComponent<CanvasGroup>().interactable = true;
        }

        // Atver pirkšanas cilni pēc noklusējuma, kad tiek atvērts veikals
        AtvertPirkt();

        // Nodrošina, ka Canvas ir aktīvs nākamajā kadrā (gadījumam, ja cits skripts to atiestata)

    }

    /// <summary>
    /// Aizver veikala paneli, padarot to neredzamu un neinteraktīvu.
    /// </summary>
    public void AizvertVeikalu()
    {
        if(veikalaUI != null)
        {
            veikalaUI.GetComponent<CanvasGroup>().alpha = 0f;
            veikalaUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
            veikalaUI.GetComponent<CanvasGroup>().interactable = false;
        }
    }

    /// <summary>
    /// Atver profila paneli un atjauno profila informāciju.
    /// </summary>
    public void AtvertProfilu()
    {
        if(kontaUI != null)
        {
            kontaUI.GetComponent<CanvasGroup>().alpha = 1f;
            kontaUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
            kontaUI.GetComponent<CanvasGroup>().interactable = true;
            
            // Atjauno profila informāciju (lietotājvārdu, ID, lomu)
            if(profilaInfo != null)
            {
                profilaInfo.AtjaunotProfiluInfo();
            }
        }
    }

    /// <summary>
    /// Aizver profila paneli, padarot to neredzamu un neinteraktīvu.
    /// </summary>
    public void AizvertProfilu()
    {
        if(kontaUI != null)
        {
            kontaUI.GetComponent<CanvasGroup>().alpha = 0f;
            kontaUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
            kontaUI.GetComponent<CanvasGroup>().interactable = false;
        }
    }
    /// <summary>
    /// Pāriet uz pieslēgšanās ekrānu (RegistracijasEkrans ekrāns)
    /// </summary>
    public void AtvertPieslegsanos()
    {
        CanvasParvalditajs.atvertRegistraciju = false;
        SceneManager.LoadScene("RegistracijasEkrans");
    }

    /// <summary>
    /// Parāda pārdodamo zivju sarakstu un paslēpj pirkšanas cilni. Ielādē pārdošanas kartītes no datubāzes.
    /// </summary>
    // Atver pārdošanas cilni
    public void AtvertPardot()
    {
        if (pardotCanvas != null && pirktCanvas != null)
        {
            pardotCanvas.GetComponent<CanvasGroup>().alpha = 1f;
            pardotCanvas.GetComponent<CanvasGroup>().blocksRaycasts = true;
            pardotCanvas.GetComponent<CanvasGroup>().interactable = true;
            pirktCanvas.GetComponent<CanvasGroup>().alpha = 0f;
            pirktCanvas.GetComponent<CanvasGroup>().blocksRaycasts = false;
            pirktCanvas.GetComponent<CanvasGroup>().interactable = false;
           
        }
        // Ielādē pārdošanas kartītes no datubāzes caur veikala pārvaldnieku
        if (veikalaParvalditajs != null)
            veikalaParvalditajs.AtvertVeikaluPardot();
    }

    /// <summary>
    /// Parāda pieejamo zivju sarakstu un paslēpj pārdošanas cilni.
    /// </summary>
    // Atver pirkšanas cilni
    public void AtvertPirkt()
    {
        if (pardotCanvas != null && pirktCanvas != null)
        {
            pirktCanvas.GetComponent<CanvasGroup>().alpha = 1f;
            pirktCanvas.GetComponent<CanvasGroup>().blocksRaycasts = true;
            pirktCanvas.GetComponent<CanvasGroup>().interactable = true;
            pardotCanvas.GetComponent<CanvasGroup>().alpha = 0f;
            pardotCanvas.GetComponent<CanvasGroup>().blocksRaycasts = false;
            pardotCanvas.GetComponent<CanvasGroup>().interactable = false;

            
        }
    }

    /// <summary>
    /// "Palikt par viesi" pogas apstrādātājs, iestata lietotāja lomu kā viesu un pāriet uz galveno ekrānu.
    /// </summary>
    public void PaliktParViesu()
    {
        LietotajaLoma.IestatitKaViesu();
        SceneManager.LoadScene("GalvenaisEkrans");
    }

    /// <summary>
    /// "Reģistrēties" pogas apstrādātājs, pāriet uz reģistrācijas ekrānu.
    /// </summary>
    public void DotiesUzRegistraciju()
    {
        CanvasParvalditajs.atvertRegistraciju = true;
        SceneManager.LoadScene("RegistracijasEkrans");
    }
}
