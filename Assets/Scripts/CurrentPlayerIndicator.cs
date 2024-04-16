using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class CurrentPlayerIndicator : MonoBehaviour
{
    // Referencia al texto que indica el jugador actual
    [SerializeField] private TMP_Text _playerIndicatorText;


    // Al inicio, suscribe la función UpdatePlayerIndicator al evento OnTurnStart
    private void Awake()
    {
        GameManager.OnTurnStart += UpdatePlayerIndicator;
    }


    // Actualiza el indicador de jugador actual
    private void UpdatePlayerIndicator(object sender, System.EventArgs e)
    {
        _playerIndicatorText.text = "Playing: " + GameManager.Instance.CurrentPlayer.PlayerName;
        _playerIndicatorText.color = GameManager.Instance.CurrentPlayer.PlayerColor;
    }
}
