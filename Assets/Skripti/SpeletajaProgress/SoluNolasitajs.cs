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

        // Pagaida, kamēr no datubāzes tiek ielādēti dati, lai var tiem droši pieskaitīt
        if (speletajaProgress != null && !speletajaProgress.datiIeladeti) {
            return;
        }

        if (soluNobide == 0)
        {
            long currentSensor = StepCounter.current.stepCounter.ReadValue();
            
            // Aprēķina soļus, kas veikti, kamēr spēle bija aizvērta
            long savedSensor = System.Convert.ToInt64(PlayerPrefs.GetString("LastSensorValue", "0"));
            long offlineSteps = 0;
            
            if (savedSensor > 0 && currentSensor > savedSensor)
            {
                offlineSteps = currentSensor - savedSensor;
            }
            
            ieprieksejieSoli = speletajaProgress != null ? speletajaProgress.soli : 0;
            
            // Pievieno oflainā noietos soļus esošajiem
            if (offlineSteps > 0 && speletajaProgress != null)
            {
                speletajaProgress.AtjauninatSolusNoSkaitītaja(speletajaProgress.soli + (int)offlineSteps);
                ieprieksejieSoli = speletajaProgress.soli;
            }

            // Aprēķina bāzi kā sensora pašreizējā vērtība mīnus saglabātie soļi;
            soluNobide = currentSensor - ieprieksejieSoli;
            
            SaglabatSensoraStavokli();
        }

        int pashreizejieSoli = (int)(StepCounter.current.stepCounter.ReadValue() - soluNobide);

        // Atjaunina tekstu tikai tad, ja ir mainījies solis
        if (pashreizejieSoli != ieprieksejieSoli)
        {
            // Atjaunina SpeletajaProgress, ja ir jauni soļi
            if (pashreizejieSoli > ieprieksejieSoli && speletajaProgress != null)
            {
                int jaunieSoli = pashreizejieSoli - ieprieksejieSoli;
                int kopaSoli = speletajaProgress.soli + jaunieSoli;

                speletajaProgress.AtjauninatSolusNoSkaitītaja(kopaSoli);
                // Atjauno nobīdi uzreiz, lai nākamā rinda rēķinātu no jaunā progresā
                long currentSensor = StepCounter.current.stepCounter.ReadValue();
                soluNobide = currentSensor - kopaSoli;
                
                pashreizejieSoli = kopaSoli; // Novērš bezgalīgo soļu skaitīšanas kļūdu
                
                SaglabatSensoraStavokli(); // Saglabā jauno sensoru vērtību fonam
            }

            counterTMP.text = "Soli: " + pashreizejieSoli.ToString();
            ieprieksejieSoli = pashreizejieSoli;
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaglabatSensoraStavokli();
        }
    }

    void OnApplicationQuit()
    {
        SaglabatSensoraStavokli();
    }

    void SaglabatSensoraStavokli()
    {
        if (Application.isEditor || StepCounter.current == null) { return; }
        long currentSensor = StepCounter.current.stepCounter.ReadValue();
        PlayerPrefs.SetString("LastSensorValue", currentSensor.ToString());
        PlayerPrefs.Save();
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