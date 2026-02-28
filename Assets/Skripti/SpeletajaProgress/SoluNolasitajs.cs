using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SoluNolasitajs : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI counterTMP, DEBUGTEXT;  // UI teksti soļu skaita un atkļūdošanas informācijas parādīšanai
    [SerializeField] SpeletajaProgress speletajaProgress;    // Atsauce uz spēlētāja progresa skriptu

    long stepOffset;             // Sākotnējā pedometra vērtība sesijas sākumā (lai rēķinātu tikai jaunos soļus)
    int ieprieksejieSoli = 0;    // Iepriekšējais soļu skaits, novērš nevajadzīgus UI atjaunojumus

    /// <summary>
    /// Inicializē pedometru, pieprasa atļauju un ieslēdz soļu skaitītāju.
    /// </summary>
    void Start()
    {
        if (Application.isEditor) { return; }

        // Pieprasa Android atļauju soļu nolasīšanai
        RequestPermission();
        // Ieslēdz soļu skaitītāja ierīci
        InputSystem.EnableDevice(StepCounter.current);
    }

    void Update()
    {
        if (Application.isEditor) { return; }

        if (stepOffset == 0)
        {
            stepOffset = StepCounter.current.stepCounter.ReadValue();
            ieprieksejieSoli = speletajaProgress != null ? speletajaProgress.soli : 0;
        }

        int pashreizejieSoli = (int)(StepCounter.current.stepCounter.ReadValue() - stepOffset);

        // Atjaunina tekstu tikai tad, ja ir mainījies solis
        if (pashreizejieSoli != ieprieksejieSoli)
        {
            counterTMP.text = "Soli: " + pashreizejieSoli.ToString();

            // Atjaunina SpeletajaProgress, ja ir jauni soļi
            if (pashreizejieSoli > ieprieksejieSoli && speletajaProgress != null)
            {
                int jaunieSoli = pashreizejieSoli - ieprieksejieSoli;
                int kopaSoli = speletajaProgress.soli + jaunieSoli;

                speletajaProgress.AtjauninatSolusNoSkaitītaja(kopaSoli);
            }

            ieprieksejieSoli = pashreizejieSoli;
        }
    }

    /// <summary>
    /// Apstrādā lietotnes pauzēšanu un atsākšanu.
    /// Kad lietotne atgriežas no fona režīma, atiestata pedometra nobīdi, lai nākamajā Update() tā tiktu pārrēķināta no jaunās bāzvērtības.
    /// Progresa saglabāšana notiek SpeletajaProgress skriptā.
    /// </summary>
    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            stepOffset = 0;
        }
    }

    async void RequestPermission()
    {
        #if UNITY_EDITOR
            DEBUGTEXT.text = "Editor Platform";
        #endif
        #if UNITY_ANDROID
            AndroidRuntimePermissions.Permission result = await AndroidRuntimePermissions.RequestPermissionAsync("android.permission.ACTIVITY_RECOGNITION");
            if (result == AndroidRuntimePermissions.Permission.Granted)
            {
                DEBUGTEXT.text = "Permissions granted";
            }
            else
            {
                DEBUGTEXT.text = "Permission denied — closing app";
                Application.Quit();
            }
        #endif
    }
}