using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountryVisual : MonoBehaviour
{
    [SerializeField] private Country country;
    [SerializeField] private SpriteRenderer countrySprite;

    private void Awake() {

        GameManager.OnGameStart += OnGameStart;
    }

    private void OnGameStart(object sender, System.EventArgs e) {
        countrySprite.color = country.owner.playerColor;
    }
}
