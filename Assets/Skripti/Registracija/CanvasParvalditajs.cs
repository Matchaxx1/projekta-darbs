using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CanvasParvalditajs : MonoBehaviour
{
    public static CanvasParvalditajs instance;
    public static bool atvertRegistraciju = false; // false = pieslegsanas, true = registracija
    public GameObject pieslegsanasCanvas;
    public GameObject registracijasCanvas;

    [Header("Aizmirstā parole")]
    public GameObject aizmirstaParoleCanvas;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
            return;
        }

        // Sākumā slēpj aizmirstās paroles paneli
        SleptAizmirstaParoleCanvas();

        if (atvertRegistraciju)
            RaditRegistraciju();
        else
            RaditPieslegsanos();
    }

    public void RaditPieslegsanos()
    {
        pieslegsanasCanvas.SetActive(true);
        registracijasCanvas.SetActive(false);
        SleptAizmirstaParoleCanvas();
    }

    public void RaditRegistraciju()
    {
        pieslegsanasCanvas.SetActive(false);
        registracijasCanvas.SetActive(true);
        SleptAizmirstaParoleCanvas();
    }

    public void AtvertAizmirstaParole()
    {
        pieslegsanasCanvas.SetActive(false);
        registracijasCanvas.SetActive(false);
        RaditAizmirstaParoleCanvas();
    }

    public void AizvertAizmirstaParole()
    {
        SleptAizmirstaParoleCanvas();
        RaditPieslegsanos();
    }

    private void RaditAizmirstaParoleCanvas()
    {
        if (aizmirstaParoleCanvas == null) return;
        aizmirstaParoleCanvas.SetActive(true);
    }

    private void SleptAizmirstaParoleCanvas()
    {
        if (aizmirstaParoleCanvas == null) return;
        aizmirstaParoleCanvas.SetActive(false);
    }
}
