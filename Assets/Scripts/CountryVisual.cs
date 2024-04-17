using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class CountryVisual : MonoBehaviour
{
    // Referencias al país y su sprite
    [SerializeField] private Country _country;
    [SerializeField] private SpriteRenderer _countrySprite;
    [SerializeField] private TMP_Text _troopsAmountIndicator;


    // Al inicio, suscribe la función SetColorToOwners al evento OnGameStart
    private void Awake()
    {
        GameManager.OnGameStart += SetColorToOwners;
        GameManager.OnGameStart += UpdateTroopsAmount;
    }


    // Actualizar indicador de cantidad de tropas
    private void UpdateTroopsAmount(object sender, EventArgs e)
    {
        _troopsAmountIndicator.text = _country.TroopsAmount.ToString();
    }


    // Colorear país según el color de su dueño
    private void SetColorToOwners(object sender, System.EventArgs e)
    {
        _countrySprite.color = _country.Owner.PlayerColor;
    }
}
