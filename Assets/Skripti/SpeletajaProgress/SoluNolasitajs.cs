using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SoluNolasitajs : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI counterTMP, debugTeksts;  // UI teksti soļu skaita un atkļūdošanas informācijas parādīšanai
    [SerializeField] SpeletajaProgress speletajaProgress;    // Atsauce uz spēlētāja progresa skriptu

    // Sākotnējā pedometra vērtība sesijas sākumā (lai rēķinātu tikai jaunos soļus).
    long soluNobide;
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

        if (soluNobide == 0)
        {
            // Aprēķina bāzi kā sensora pašreizējā vērtība mīnus saglabātie soļi;
            long currentSensor = StepCounter.current.stepCounter.ReadValue();
            ieprieksejieSoli = speletajaProgress != null ? speletajaProgress.soli : 0;
            soluNobide = currentSensor - ieprieksejieSoli;
        }

        int pashreizejieSoli = (int)(StepCounter.current.stepCounter.ReadValue() - soluNobide);

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
                // Atjauno nobīdi uzreiz, lai nākamā rinda rēķinātu no jaunā progresā
                long currentSensor = StepCounter.current.stepCounter.ReadValue();
                soluNobide = currentSensor - kopaSoli;
            }

            ieprieksejieSoli = pashreizejieSoli;
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
    }

    async void RequestPermission()
    {
        #if UNITY_EDITOR
            debugTeksts.text = "Atvērts datorā";
        #endif
        #if UNITY_ANDROID
            AndroidRuntimePermissions.Permission result = await AndroidRuntimePermissions.RequestPermissionAsync("android.permission.ACTIVITY_RECOGNITION");
            if (result == AndroidRuntimePermissions.Permission.Granted)
            {
                debugTeksts.text = "Dota atļauja!";
            }
            else
            {
                debugTeksts.text = "Nav dota atļauja, iziet ārā no spēles!";
                Application.Quit();
            }
        #endif
    }
}