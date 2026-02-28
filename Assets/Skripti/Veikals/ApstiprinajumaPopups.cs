using UnityEngine;
using System;
using EasyPopupSystem;

public class ApstiprinajumaPopups : MonoBehaviour
{
    /// <summary>
    /// Parāda apstiprinājuma uznirstošo logu ar norādīto virsrakstu un ziņojumu.
    /// Ja lietotājs nospiež "Jā", tiek izpildīta padotā darbība.
    /// Ja lietotājs nospiež "Nē", logs vienkārši aizveras bez papildu darbībām.
    /// </summary>
    public static void RaditPopup(string virsraksts, string zinutne, Action uzJa)
    {
        // Izveido uznirstošo logu ar brīdinājuma stilu un divām pogām
        EasyPopup.Create(
            virsraksts,
            zinutne,
            "PopupWarning",
            uzJa,
            () => { },
            true,
            "Jā",
            "Nē"
        );
    }
}
