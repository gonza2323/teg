using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class RoundNoIndicator : MonoBehaviour
{
    // Referencia al texto que indica el n�mero de ronda
    [SerializeField] private TMP_Text _roundNoIndicatorText;


    // Al inicio, suscribe la funci�n UpdateRoundNoIndicator al evento OnRoundStart
    private void Awake()
    {
        GameManager.OnRoundStart += UpdateRoundNoIndicator;
    }


    // Actualiza el indicador de n�mero de ronda
    private void UpdateRoundNoIndicator(object sender, System.EventArgs e)
    {
        _roundNoIndicatorText.text = "Round No: " + GameManager.Instance.RoundNo;
    }
}
