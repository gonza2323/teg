using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoundNoIndicator : MonoBehaviour
{
    [SerializeField] private TMP_Text roundNoIndicatorText;

    private void Start() {
        GameManager.Instance.OnTurnStart += OnRoundStart;
    }

    private void OnRoundStart(object sender, System.EventArgs e) {
        roundNoIndicatorText.text = "Round No: " + GameManager.Instance.RoundNo;
    }
}
