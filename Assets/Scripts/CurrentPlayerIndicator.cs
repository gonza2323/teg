using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CurrentPlayerIndicator : MonoBehaviour
{
    [SerializeField] private TMP_Text playerIndicatorText;

    private void Start() {
        GameManager.Instance.OnTurnStart += OnTurnStart;
    }

    private void OnTurnStart(object sender, System.EventArgs e) {
        playerIndicatorText.text = "Playing: " + GameManager.Instance.CurrentPlayer.playerName;
    }
}
