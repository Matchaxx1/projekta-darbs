using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script lai automātiski konfigurētu Canvas Scaler Android ierīcēm.
/// Pievieno šo skriptu tādam pašam GameObject, kurā ir Canvas komponente.
/// </summary>
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
public class CanvasScalerHelper : MonoBehaviour
{
    [Header("Scala iestatījumi")]
    [Tooltip("References izšķirtspēja platumam")]
    [SerializeField] private float referenceWidth = 1080f;
    
    [Tooltip("References izšķirtspēja augstumam")]
    [SerializeField] private float referenceHeight = 1920f;
    
    [Tooltip("Match Mode: 0 = Width, 0.5 = Balance, 1 = Height")]
    [Range(0f, 1f)]
    [SerializeField] private float matchWidthOrHeight = 0.5f;

    void Awake()
    {
        ConfigureCanvasScaler();
    }

    private void ConfigureCanvasScaler()
    {
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            Debug.LogError("CanvasScalerHelper: Nav atrasta CanvasScaler komponente!");
            return;
        }

        // Iestata Scale With Screen Size režīmu
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        
        // Iestata references izšķirtspēju
        scaler.referenceResolution = new Vector2(referenceWidth, referenceHeight);
        
        // Iestata match mode
        scaler.matchWidthOrHeight = matchWidthOrHeight;
        
        // Iestata screen match mode
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        Debug.Log($"Canvas Scaler konfigurēts: {referenceWidth}x{referenceHeight}, match={matchWidthOrHeight}");
        Debug.Log($"Pašreizējā ierīces izšķirtspēja: {Screen.width}x{Screen.height}");
    }

#if UNITY_EDITOR
    // Automātiski konfigurē, kad pievieno skriptu Unity editorā
    void Reset()
    {
        ConfigureCanvasScaler();
    }
#endif
}
