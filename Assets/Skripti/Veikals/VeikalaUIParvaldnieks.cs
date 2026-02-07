using System;
using UnityEngine;

public class VeikalaUIParvaldnieks : MonoBehaviour
{
    public GameObject veikalaUI;
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
}
