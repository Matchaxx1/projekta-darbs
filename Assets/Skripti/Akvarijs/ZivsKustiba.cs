using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ZivsKustiba : MonoBehaviour
{
    [Header("Ātrums")]
    [SerializeField] private float minAtrums = 0.6f;
    [SerializeField] private float maxAtrums = 1.4f;
    [SerializeField] private float atrumaMaina = 0.4f;

    [Header("Klīšana")]
    [SerializeField] private float maxPagr = 55f;
    [SerializeField] private float pagrJitter = 180f;

    [Header("Šūpošanās")]
    [SerializeField] private float suposanosAplituda = 0.18f;
    [SerializeField] private float suposanosAtrums = 1.8f;

    // --- privātie lauki ---
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private Vector2 robezaMin;
    private Vector2 robezaMax;
    private bool inicializets = false;

    private float virziensLenkis;
    private float pagriezienaAtrums;
    private float pasreizejaisAtrums;
    private float merkaAtrums;
    private float suposanasNobide;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        suposanasNobide = Random.Range(0f, Mathf.PI * 2f);
    }

    /// <summary>
    /// Izsauc AkvarijaParvaldnieks pēc spawna — iestata akvārija robežas.
    /// </summary>
    public void Inicializet(Vector2 min, Vector2 max)
    {
        robezaMin = min;
        robezaMax = max;
        inicializets = true;

        virziensLenkis = Random.Range(0f, 360f);
        pagriezienaAtrums = 0f;
        pasreizejaisAtrums = Random.Range(minAtrums, maxAtrums);
        merkaAtrums = pasreizejaisAtrums;
    }

    void FixedUpdate()
    {
        if (!inicializets || rb == null) return;

        float dt = Time.fixedDeltaTime;
        Vector2 pozicija = rb.position;

        // ── 1. Wander ─────────────────────────────────────────────────────────
        pagriezienaAtrums += Random.Range(-pagrJitter, pagrJitter) * dt;
        pagriezienaAtrums = Mathf.Clamp(pagriezienaAtrums, -maxPagr, maxPagr);
        virziensLenkis += pagriezienaAtrums * dt;

        // ── 2. Ātruma maiga maiņa ─────────────────────────────────────────────
        if (Random.value < 0.005f)
            merkaAtrums = Random.Range(minAtrums, maxAtrums);
        pasreizejaisAtrums = Mathf.Lerp(pasreizejaisAtrums, merkaAtrums, atrumaMaina * dt);

        // ── 3. Aprēķina kustības vektoru ─────────────────────────────────────
        float lenkisRad = virziensLenkis * Mathf.Deg2Rad;
        float atrX = Mathf.Cos(lenkisRad) * pasreizejaisAtrums;
        float atrY = Mathf.Sin(lenkisRad) * pasreizejaisAtrums;

        // ── 4. Sienas atbangošana — apvērš konkrēto ass komponentu ───────────
        bool atsitasPretSienu = false;

        if (pozicija.x <= robezaMin.x && atrX < 0f)
        {
            atrX = -atrX;
            pozicija.x = robezaMin.x;
            atsitasPretSienu = true;
        }
        else if (pozicija.x >= robezaMax.x && atrX > 0f)
        {
            atrX = -atrX;
            pozicija.x = robezaMax.x;
            atsitasPretSienu = true;
        }

        if (pozicija.y <= robezaMin.y && atrY < 0f)
        {
            atrY = -atrY;
            pozicija.y = robezaMin.y;
            atsitasPretSienu = true;
        }
        else if (pozicija.y >= robezaMax.y && atrY > 0f)
        {
            atrY = -atrY;
            pozicija.y = robezaMax.y;
            atsitasPretSienu = true;
        }

        // Ja atbangoja — atjaunina leņķi no jaunā vektora un novieto uz robežas
        if (atsitasPretSienu)
        {
            virziensLenkis = Mathf.Atan2(atrY, atrX) * Mathf.Rad2Deg;
            pagriezienaAtrums = 0f;
            rb.position = pozicija;
        }

        // ── 5. Šūpošanās ─────────────────────────────────────────────────────
        float supojums = Mathf.Sin(Time.time * suposanosAtrums + suposanasNobide) * suposanosAplituda;

        // ── 6. Pielieto ātrumu ────────────────────────────────────────────────
        rb.linearVelocity = new Vector2(atrX, atrY + supojums);

        // ── 7. Sprīta apvēršana ───────────────────────────────────────────────
        if (sr != null && Mathf.Abs(atrX) > 0.05f)
            sr.flipX = atrX < 0f;
    }
}
