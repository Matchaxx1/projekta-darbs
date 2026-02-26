using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIParvaldnieks : MonoBehaviour
{
    public GameObject veikalaUI;
    public GameObject kontaUI;
    public GameObject pardotCanvas;
    public GameObject pirktCanvas;
    public VeikalaParvalditajs veikalaParvalditajs;
    public GameObject izveleUI;
    public GameObject piesliegsanasPoga;
    public GameObject registracijasPoga;
    public ProfilaInformacija profilaInfo;
    public GameObject pieslegtiesUI;

    private void Start()
    {

        // Radam izveleUI tikai tad, ja lietotajam nav lomas (nav izvelejies)
        if(izveleUI != null)
        {
            if(LietotajaLoma.PasreizejaLoma == LietotajaLoma.Loma.Nav)
            {
                izveleUI.GetComponent<CanvasGroup>().alpha = 1f;
                izveleUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
                izveleUI.GetComponent<CanvasGroup>().interactable = true;
            }
            else
            {
                izveleUI.GetComponent<CanvasGroup>().alpha = 0f;
                izveleUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
                izveleUI.GetComponent<CanvasGroup>().interactable = false;
            }
        }

        // Parvaldi piesliegsanas un registracijas pogu redzesamibu
        AuthPogasParvalde();
    }
    
    // Parāda/paslēpj pieslēgšanās un reģistrācijas pogas atkarībā no lomas

    
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

    public void AtvertVeikalu()
    {
        if (veikalaUI != null)
        {
            veikalaUI.GetComponent<CanvasGroup>().alpha = 1f;
            veikalaUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
            veikalaUI.GetComponent<CanvasGroup>().interactable = true;
        }

        // Atver pirksanas cilni pec noklusejuma
        AtvertPirkt();

        // Force-ensure after a frame in case something else is resetting
        StartCoroutine(EnsureCanvasActiveNextFrame(pirktCanvas));
    }

    private System.Collections.IEnumerator EnsureCanvasActiveNextFrame(GameObject canvas)
    {
        yield return null;
        if (canvas != null)
        {
            CanvasGroup cg = canvas.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.blocksRaycasts = true;
                cg.interactable = true;
            }
            foreach (CanvasGroup childCG in canvas.GetComponentsInChildren<CanvasGroup>())
            {
                childCG.interactable = true;
                childCG.blocksRaycasts = true;
            }
        }
    }

    public void AizvertVeikalu()
    {
        if(veikalaUI != null)
        {
            veikalaUI.GetComponent<CanvasGroup>().alpha = 0f;
            veikalaUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
            veikalaUI.GetComponent<CanvasGroup>().interactable = false;
        }
    }

    public void AtvertProfilu()
    {
        if(kontaUI != null)
        {
            kontaUI.GetComponent<CanvasGroup>().alpha = 1f;
            kontaUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
            kontaUI.GetComponent<CanvasGroup>().interactable = true;
            
            // Atjauno profila informāciju
            if(profilaInfo != null)
            {
                profilaInfo.AtjaunotProfiluInfo();
            }
        }
    }

    public void AizvertProfilu()
    {
        if(kontaUI != null)
        {
            kontaUI.GetComponent<CanvasGroup>().alpha = 0f;
            kontaUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
            kontaUI.GetComponent<CanvasGroup>().interactable = false;
        }
    }
    public void AtvertPieslegsanos()
    {
        CanvasParvalditajs.atvertRegistraciju = false;
        SceneManager.LoadScene("RegistracijasEkrans");
    }

    // Atver pardosanas cilni
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

            // Ensure all child CanvasGroups in pardotCanvas are active
            foreach (CanvasGroup childCG in pardotCanvas.GetComponentsInChildren<CanvasGroup>())
            {
                childCG.interactable = true;
                childCG.blocksRaycasts = true;
            }

            StartCoroutine(EnsureCanvasActiveNextFrame(pardotCanvas));
        }
        if (veikalaParvalditajs != null)
            veikalaParvalditajs.AtvertVeikaluPardot();
    }

    // Atver pirksanas cilni
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

            // Ensure all child CanvasGroups in pirktCanvas are active
            foreach (CanvasGroup childCG in pirktCanvas.GetComponentsInChildren<CanvasGroup>())
            {
                childCG.interactable = true;
                childCG.blocksRaycasts = true;
            }

            StartCoroutine(EnsureCanvasActiveNextFrame(pirktCanvas));
        }
    }

    // "Palikt par viesi" poga - iestata lomu ka viesis un iet uz galveno ekranu
    public void PaliktParViesu()
    {
        LietotajaLoma.IestatitKaViesu();
        SceneManager.LoadScene("GalvenaisEkrans");
    }

    // "Registreties" poga - aizved uz registracijas ekranu
    public void DotiesUzRegistraciju()
    {
        CanvasParvalditajs.atvertRegistraciju = true;
        SceneManager.LoadScene("RegistracijasEkrans");
    }
}
