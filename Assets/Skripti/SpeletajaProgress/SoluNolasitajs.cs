using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SoluNolasitajs : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI counterTMP, debugTeksts;
    [SerializeField] SpeletajaProgress speletajaProgress;

    long soluNobide;          // Tas cik soļi bija uzkrāti pirms lietotnes palaišanas
    int ieprieksejieSoli = 0;  // Iepriekšējais soļu skaits, lai izvairītos no liekiem UI atjauninājumiem
    bool uzsakts = false;      // Vai skaitītājs jau uzsākts

    /// <summary>
    /// Pieprasa soļu lasīšanas atļauju un ieslēdz soļu skaitītāja sensoru.
    /// </summary>
    void Start()
    {
        if (Application.isEditor) { return; }

        RequestPermission();
        InputSystem.EnableDevice(StepCounter.current);
    }

    /// <summary>
    /// Katru kadru nolasa sensora vērtību, aprēķina jaunos soļus un atjauno UI un progresu.
    /// Pirmajā kadrā uzsāk nobīdi un pieskaita soļus, kas staigāti fonā vai aizvērtā lietotnē.
    /// </summary>
    void Update()
    {
        if (Application.isEditor) { return; }
        if (StepCounter.current == null) { return; }

        long sensors = StepCounter.current.stepCounter.ReadValue();
        if (sensors <= 0) { return; }

        if (!uzsakts)
        {
            ieprieksejieSoli = speletajaProgress != null ? speletajaProgress.soli : 0;

            // Pieskaita soļus, kas staigāti fonā/aizvērtā lietotnē
            long vecaisSensors = long.Parse(PlayerPrefs.GetString("SensorsVertiba", "0"));
            if (vecaisSensors > 0 && sensors >= vecaisSensors)
            {
                int fonaSoli = (int)(sensors - vecaisSensors);
                if (fonaSoli > 0 && speletajaProgress != null)
                {
                    ieprieksejieSoli += fonaSoli;
                    speletajaProgress.AtjauninatSolusNoSkaitītaja(ieprieksejieSoli);
                }
            }

            soluNobide = sensors - ieprieksejieSoli;
            uzsakts = true;
            SaglabatSensoru(sensors);
        }

        int pasreizejieSoli = (int)(sensors - soluNobide);

        if (pasreizejieSoli != ieprieksejieSoli) // Atjaunina tikai ja solis mainījies
        {
            counterTMP.text = "Soli: " + pasreizejieSoli;

            if (pasreizejieSoli > ieprieksejieSoli && speletajaProgress != null)
            {
                int kopaSoli = speletajaProgress.soli + (pasreizejieSoli - ieprieksejieSoli);
                speletajaProgress.AtjauninatSolusNoSkaitītaja(kopaSoli);

                // Pārrēķina nobīdi pēc progresa atjaunināšanas
                sensors = StepCounter.current.stepCounter.ReadValue();
                soluNobide = sensors - kopaSoli;
            }

            ieprieksejieSoli = pasreizejieSoli;
            SaglabatSensoru(sensors);
        }
    }

    /// <summary>
    /// Aizejot fonā saglabā sensora vērtību. Atgriežoties atiestatās inicializācija, lai Update pārrēķinātu soļus, kas staigāti fona laikā.
    /// </summary>
    void OnApplicationPause(bool pauseStatus)
    {
        if (Application.isEditor || StepCounter.current == null) { return; }

        if (pauseStatus)
        {
            // Aizejot fonā — saglabā sensora vērtību
            long sensors = StepCounter.current.stepCounter.ReadValue();
            if (sensors > 0) SaglabatSensoru(sensors);
        }
        else
        {
            // Atgriežoties no fona — ļauj Update pārrēķināt fona soļus
            InputSystem.EnableDevice(StepCounter.current);
            uzsakts = false;
        }
    }

    /// <summary>
    /// Saglabā pēdējo sensora vērtību pirms lietotnes aizvēršanas.
    /// </summary>
    void OnApplicationQuit()
    {
        if (Application.isEditor || StepCounter.current == null) { return; }

        // Saglabā sensora vērtību pirms lietotnes aizvēršanas
        long sensors = StepCounter.current.stepCounter.ReadValue();
        if (sensors > 0) SaglabatSensoru(sensors);
    }

    /// <summary>
    /// Saglabā sensora vērtību PlayerPrefs, lai nākamajā sesijā varētu aprēķināt fona soļus.
    /// </summary>
    void SaglabatSensoru(long vertiba)
    {
        PlayerPrefs.SetString("SensorsVertiba", vertiba.ToString());
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Pieprasa Android atļauju soļu lasīšanai. Ja atļauja tiek atteikta, aizver lietotni.
    /// </summary>
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