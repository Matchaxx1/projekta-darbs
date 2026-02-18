using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIParvaldnieks : MonoBehaviour
{
    public GameObject veikalaUI;
    public GameObject kontaUI;
    public GameObject izveleUI;

    private void Start()
    {
        // Parbaudit vai lietotajs jau ir izvelejies (viesis vai registrets)
        if (LietotajaLoma.PasreizejaLoma != LietotajaLoma.Loma.Nav && izveleUI != null)
        {
            izveleUI.GetComponent<CanvasGroup>().alpha = 0f;
            izveleUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
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
    public void AtvērtPieslēgšanos()
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
