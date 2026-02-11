using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SoluNolasitajs : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI counterTMP, DEBUGTEXT;
    [SerializeField] SpeletajaProgress speletajaProgress;

    long stepOffset;
    int ieprieksejieSoli = 0;

    void Start()
    {
        if (Application.isEditor) { return; }

        RequestPermission();
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
        counterTMP.text = "Soli: " + pashreizejieSoli.ToString();

        // Atjaunina SpeletajaProgress, ja ir jauni soļi
        if (pashreizejieSoli > ieprieksejieSoli && speletajaProgress != null)
        {
            int jaunieSoli = pashreizejieSoli - ieprieksejieSoli;
            int kopaSoli = speletajaProgress.soli + jaunieSoli;
            
            speletajaProgress.AtjauninatSolusNoSkaitītaja(kopaSoli);
            ieprieksejieSoli = pashreizejieSoli;
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // Saglabā progresu, kad app iet fonā vai atgriežas
        if (pauseStatus && speletajaProgress != null)
        {
            if (DatuBaze.Instance != null)
            {
                DatuBaze.Instance.SaglabatProgresu(speletajaProgress.soli, speletajaProgress.monetas);
            }
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
                DEBUGTEXT.text = "Permissions granted";
            else
                DEBUGTEXT.text = "Permission state: " + result;
        #endif
    }
}