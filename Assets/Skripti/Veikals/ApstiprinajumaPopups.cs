using UnityEngine;
using System;
using EasyPopupSystem;

public class ApstiprinajumaPopups : MonoBehaviour
{

    public static void RaditPopup(string virsraksts, string zinutne, Action uzJa)
    {
        EasyPopup.Create(
            virsraksts,
            zinutne,
            "PopupWarning",
            uzJa,
            () => { /* cancel - just close */ },
            true,
            "Jā",
            "Nē"
        );
    }
}
