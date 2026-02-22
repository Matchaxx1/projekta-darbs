using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIParvaldnieks : MonoBehaviour
{
    public GameObject veikalaUI;
    public GameObject kontaUI;
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
        if(veikalaUI != null)
        {
            veikalaUI.GetComponent<CanvasGroup>().alpha = 1f;
            veikalaUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
    }

    public void AizvertVeikalu()
    {
        if(veikalaUI != null)
        {
            veikalaUI.GetComponent<CanvasGroup>().alpha = 0f;
            veikalaUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }

    public void AtvertProfilu()
    {
        if(kontaUI != null)
        {
            kontaUI.GetComponent<CanvasGroup>().alpha = 1f;
            kontaUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
            
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
        }
    }
    public void AtvertPieslegsanos()
    {
        SceneManager.LoadScene("RegistracijasEkrans");
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
        SceneManager.LoadScene("RegistracijasEkrans");
    }
}
