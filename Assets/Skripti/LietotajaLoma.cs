using UnityEngine;

// Glabā lietotāja lomu (viesis vai reģistrēts)
public static class LietotajaLoma
{
    /// <summary>
    /// Iespējamās lietotāja lomas.
    /// </summary>
    public enum Loma
    {
        Nav,          // Vēl nav izvēlēts, lietotājs tikko atvēris spēli pirmo reizi
        Viesis,       // Spēlē kā viesis, dati tiek glabāti lokāli SQLite datubāzē
        Registrets    // Reģistrēts vai pieslēdzies lietotājs, dati tiek glabāti Firestore mākonī
    }

    // Atslēga, ar kādu loma tiek saglabāta PlayerPrefs krātuvē
    private const string LOMA_KEY = "lietotaja_loma";
    // Pašreizējā loma atmiņā
    private static Loma _pasreizejaLoma = Loma.Nav;
    // Kārodziņš, kas norāda, vai loma jau ir ielādēta no PlayerPrefs
    private static bool _irIeladeta = false;

    /// <summary>
    /// Pašreizējā lietotāja loma. Automātiski ielādē lomu no PlayerPrefs, ja tā vēl nav tikusi ielādēta.
    /// Iestatīšanas (set) brīdī automātiski saglabā jauno vērtību PlayerPrefs.
    /// </summary>
    public static Loma PasreizejaLoma
    {
        get
        {
            // Automātiski ielādē lomu no PlayerPrefs, ja vēl nav ielādēta
            if (!_irIeladeta)
            {
                IeladetLomu();
            }
            return _pasreizejaLoma;
        }
        private set
        {
            // Iestata jauno lomu un saglabā to PlayerPrefs
            _pasreizejaLoma = value;
            _irIeladeta = true;
            PlayerPrefs.SetInt(LOMA_KEY, (int)value);
            PlayerPrefs.Save();
            Debug.Log("Lietotāja loma iestatīta: " + value);
        }
    }

    /// <summary>
    /// Ielādē iepriekš saglabāto lomu no PlayerPrefs.
    /// Ja nav atrastas saglabātas vērtības, iestata lomu kā "Nav".
    /// </summary>
    public static void IeladetLomu()
    {
        if (PlayerPrefs.HasKey(LOMA_KEY))
        {
            // Nolasa saglabāto lomas vērtību un pārveido par enum tipu
            _pasreizejaLoma = (Loma)PlayerPrefs.GetInt(LOMA_KEY, 0);
            _irIeladeta = true;
            Debug.Log("Lietotāja loma ielādēta: " + _pasreizejaLoma);
        }
        else
        {
            // Nav saglabātas lomas, iestata noklusējuma vērtību
            _pasreizejaLoma = Loma.Nav;
            _irIeladeta = true;
            Debug.Log("Nav saglabātas lomas, iestatīta: Nav");
        }
    }

    /// <summary>
    /// Iestata lietotāja lomu kā viesu, dati tiks glabāti lokāli.
    /// </summary>
    public static void IestatitKaViesu()
    {
        PasreizejaLoma = Loma.Viesis;
    }

    /// <summary>
    /// Iestata lietotāja lomu kā reģistrētu, dati tiks glabāti Firestore mākonī.
    /// </summary>
    public static void IestatitKaRegistretu()
    {
        PasreizejaLoma = Loma.Registrets;
    }

    /// <summary>
    /// Pārbauda, vai pašreizējais lietotājs ir viesis.
    /// </summary>
    public static bool IrViesis()
    {
        return _pasreizejaLoma == Loma.Viesis;
    }

    /// <summary>
    /// Pārbauda, vai pašreizējais lietotājs ir reģistrēts.
    /// </summary>
    public static bool IrRegistrets()
    {
        return _pasreizejaLoma == Loma.Registrets;
    }

    /// <summary>
    /// Atiestata lietotāja lomu uz noklusējuma vērtību (Nav).
    /// Tiek izsaukta, piemēram, izrakstīšanās brīdī vai progresa atiestatīšanas laikā.
    /// </summary>
    public static void AtiestatitLomu()
    {
        PasreizejaLoma = Loma.Nav;
    }
}
