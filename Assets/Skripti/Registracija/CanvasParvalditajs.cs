using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CanvasParvalditajs : MonoBehaviour
{
    public static CanvasParvalditajs instance;
    public GameObject pieslegsanasCanvas;
    public GameObject registracijasCanvas;

    private void Awake()
    {
        RaditPieslegsanos();

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

    }

    public void RaditPieslegsanos()
    {
        pieslegsanasCanvas.SetActive(true);
        registracijasCanvas.SetActive(false);
    }

    public void RaditRegistraciju()
    {
        pieslegsanasCanvas.SetActive(false);
        registracijasCanvas.SetActive(true);
    }
    
}
