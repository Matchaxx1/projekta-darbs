using UnityEngine;

// Glabā lietotāja lomu (viesis vai reģistrēts)
public static class LietotajaLoma
{
    public enum Loma
    {
        Nav,          // Vēl nav izvēlēts
        Viesis,       // Spēlē kā viesis
        Registrets    // Reģistrēts / pieslēdzies lietotājs
    }

    private const string LOMA_KEY = "lietotaja_loma";
    private static Loma _pasreizejaLoma = Loma.Nav;

    public static Loma PasreizejaLoma
    {
        get => _pasreizejaLoma;
        private set
        {
            _pasreizejaLoma = value;
            PlayerPrefs.SetInt(LOMA_KEY, (int)value);
            PlayerPrefs.Save();
            Debug.Log("Lietotāja loma iestatīta: " + value);
        }
    }

    // Ielādē saglabāto lomu no PlayerPrefs
    public static void IeladetLomu()
    {
        if (PlayerPrefs.HasKey(LOMA_KEY))
        {
            _pasreizejaLoma = (Loma)PlayerPrefs.GetInt(LOMA_KEY, 0);
            Debug.Log("Lietotāja loma ielādēta: " + _pasreizejaLoma);
        }
        else
        {
            _pasreizejaLoma = Loma.Nav;
        }
    }

    public static void IestatitKaViesu()
    {
        PasreizejaLoma = Loma.Viesis;
    }

    public static void IestatitKaRegistretu()
    {
        PasreizejaLoma = Loma.Registrets;
    }

    public static bool IrViesis()
    {
        return _pasreizejaLoma == Loma.Viesis;
    }

    public static bool IrRegistrets()
    {
        return _pasreizejaLoma == Loma.Registrets;
    }

    // Atiestatīt lomu (piemēram, izlogojoties)
    public static void AtiestatitLomu()
    {
        PasreizejaLoma = Loma.Nav;
    }
}
