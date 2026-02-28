using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Reģistrācijas un pieslēgšanās ekrāna Canvas pārvaldnieks.
/// </summary>
public class CanvasParvalditajs : MonoBehaviour
{
    public static CanvasParvalditajs instance;
    public static bool atvertRegistraciju = false;

    // Atsauces uz UI paneļiem (Canvas objektiem)
    public GameObject pieslegsanasCanvas;    // Pieslēgšanās formas panelis
    public GameObject registracijasCanvas;   // Reģistrācijas formas panelis

    public GameObject aizmirstaParoleCanvas;  // Aizmirstās paroles atjaunošanas panelis

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

        // Parāda attiecīgo paneli
        if (atvertRegistraciju)
            RaditRegistraciju();
        else
            RaditPieslegsanos();
    }

    /// <summary>
    /// Parāda pieslēgšanās formu un paslēpj pārējos paneļus.
    /// </summary>
    public void RaditPieslegsanos()
    {
        pieslegsanasCanvas.SetActive(true);
        registracijasCanvas.SetActive(false);
        SleptAizmirstaParoleCanvas();
    }

    /// <summary>
    /// Parāda reģistrācijas formu un paslēpj pārējos paneļus.
    /// </summary>
    public void RaditRegistraciju()
    {
        pieslegsanasCanvas.SetActive(false);
        registracijasCanvas.SetActive(true);
        SleptAizmirstaParoleCanvas();
    }

    /// <summary>
    /// Atver aizmirstās paroles atjaunošanas formu, paslēpjot citas formas.
    /// </summary>
    public void AtvertAizmirstaParole()
    {
        pieslegsanasCanvas.SetActive(false);
        registracijasCanvas.SetActive(false);
        RaditAizmirstaParoleCanvas();
    }

    /// <summary>
    /// Aizver aizmirstās paroles formu un atgriežas pie pieslēgšanās formas.
    /// </summary>
    public void AizvertAizmirstaParole()
    {
        SleptAizmirstaParoleCanvas();
        RaditPieslegsanos();
    }

    /// <summary>
    /// Parāda aizmirstās paroles Canvas paneli, ja tas ir pieejams.
    /// </summary>
    private void RaditAizmirstaParoleCanvas()
    {
        if (aizmirstaParoleCanvas == null) return;
        aizmirstaParoleCanvas.SetActive(true);
    }

    /// <summary>
    /// Paslēpj aizmirstās paroles Canvas paneli, ja tas ir pieejams.
    /// </summary>
    private void SleptAizmirstaParoleCanvas()
    {
        if (aizmirstaParoleCanvas == null) return;
        aizmirstaParoleCanvas.SetActive(false);
    }
}

