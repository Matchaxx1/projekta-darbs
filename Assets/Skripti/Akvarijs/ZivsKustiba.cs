using UnityEngine;

public class ZivsKustiba : MonoBehaviour
{
    [Header("Ātrums")]
    [SerializeField] private float minAtrums = 0.6f;     // Minimālais kustības ātrums
    [SerializeField] private float maxAtrums = 1.4f;     // Maksimālais kustības ātrums
    [SerializeField] private float atrumaMaina = 0.4f;   // Ātruma maiņas koeficients

    [Header("Klīšana")]
    [SerializeField] private float maxPagr = 55f;        // Maksimālais pagrieziena ātrums (grādi sekundē)
    [SerializeField] private float pagrJitter = 180f;    // Nejausinma amp. pagrieziena izmaiņām

    [Header("Šūpošanās")]
    [SerializeField] private float suposanasAmplituda = 0.18f;  // Vertikālās šūpošanās amplitūda
    [SerializeField] private float suposanasAtrums = 1.8f;     // Vertikālās šūpošanās frekvence

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private Vector2 robezaMin;         // Akvārija apakšējā kreisais stūris (kustības robeža)
    private Vector2 robezaMax;         // Akvārija augšējais labais stūris (kustības robeža)
    private bool inicializets = false; // Vai robežas ir ieslēgtas

    private float virziensLenkis;      // Pašreizējais kustības virziena leņķis grādos
    private float pagriezienaAtrums;   // Pašreizējais pagrieziena ātrums
    private float pasreizejaisAtrums;  // Pašreizējais kustības ātrums
    private float merkaAtrums;         // Mērķa ātrums, uz kuru zivs pakāpeniski virzās
    private float suposanasNobide;     // Nejauša šūpošanās nobide, lai visas zivis nešūpotos sinhroni

    /// <summary>
    /// Inicializē fizikas un spraita komponentes, ieslēdz bezgravitācijas režīmu un ģenerē nejaušu šūpošanās nobidi katrai zivij.
    /// </summary>
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
    /// Izsauc AkvārijaPārvaldnieks pēc zivs radīšanas un iestata akvārija robežas, kurās zivs drīkst kustēties. Bez šī izsaukuma zivs nekustēsies.
    /// </summary>
    /// <param name="min">Akvārija apakšējais kreisais stūris</param>
    /// <param name="max">Akvārija augšējais labais stūris</param>
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

    /// <summary>
    /// Galvenais kustības aprēķins, kas tiek izpildīts katru fizikas kadru.
    /// </summary>
    void FixedUpdate()
    {
        if (!inicializets || rb == null) return;

        float dt = Time.fixedDeltaTime;
        Vector2 pozicija = rb.position;

        // 1. Nejauši maina pagrieziena ātrumu, kas pakāpeniski groza virzienu
        pagriezienaAtrums += Random.Range(-pagrJitter, pagrJitter) * dt;
        pagriezienaAtrums = Mathf.Clamp(pagriezienaAtrums, -maxPagr, maxPagr);
        virziensLenkis += pagriezienaAtrums * dt;

        // 2. Ik pa laikam izvēlas jaunu mērķa ātrumu
        if (Random.value < 0.005f)
            merkaAtrums = Random.Range(minAtrums, maxAtrums);
        pasreizejaisAtrums = Mathf.Lerp(pasreizejaisAtrums, merkaAtrums, atrumaMaina * dt);

        // 3. Aprēķina kustības vektoru no leņķa un ātruma
        float lenkisRad = virziensLenkis * Mathf.Deg2Rad;
        float atrX = Mathf.Cos(lenkisRad) * pasreizejaisAtrums;
        float atrY = Mathf.Sin(lenkisRad) * pasreizejaisAtrums;

        // 4. Ātruma komponentu padara negatīvu, kad zivs sasniedz robežu
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

        // Ja zivs atlēca no sienas, pārrēķina virziena leņķi no jaunā ātruma vektora
        if (atsitasPretSienu)
        {
            virziensLenkis = Mathf.Atan2(atrY, atrX) * Mathf.Rad2Deg;
            pagriezienaAtrums = 0f;
            rb.position = pozicija;
        }

        // 5. Pievieno zivs šūpošanos
        float supojums = Mathf.Sin(Time.time * suposanasAtrums + suposanasNobide) * suposanasAmplituda;

        // 6. Pielieto galejīgo ātrumu Rigidbody2D komponentei
        rb.linearVelocity = new Vector2(atrX, atrY + supojums);

        // 7. Apvērš spraitu horizontāli atbilstoši kustības virzienam
        if (sr != null && Mathf.Abs(atrX) > 0.05f)
            sr.flipX = atrX < 0f;
    }
}
