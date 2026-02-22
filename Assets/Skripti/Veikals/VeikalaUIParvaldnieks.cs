using System;
using UnityEngine;

public class VeikalaUIParvaldnieks : MonoBehaviour
{
    public GameObject veikalaUI;
    public GameObject pardotCanvas;
    public GameObject pirktCanvas;
    public VeikalaParvalditajs veikalaParvalditajs;
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

    public void AtvertPardot()
    {
        if(pardotCanvas != null && pirktCanvas != null)
        {
            pardotCanvas.GetComponent<CanvasGroup>().alpha = 1f;
            pirktCanvas.GetComponent<CanvasGroup>().alpha = 0f;

            pardotCanvas.GetComponent<CanvasGroup>().blocksRaycasts = true;
            pirktCanvas.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }

        // Ielade pardosanas kartites
        if (veikalaParvalditajs != null)
            veikalaParvalditajs.AtvertVeikaluPardot();
    }
    public void AtvertPirkt()
    {
        if(pardotCanvas != null && pirktCanvas != null)
        {
            pardotCanvas.GetComponent<CanvasGroup>().alpha = 0f;
            pirktCanvas.GetComponent<CanvasGroup>().alpha = 1f;

            pardotCanvas.GetComponent<CanvasGroup>().blocksRaycasts = false;
            pirktCanvas.GetComponent<CanvasGroup>().blocksRaycasts = true;

        }
    }

}
