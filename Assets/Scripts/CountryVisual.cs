using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class CountryVisual : MonoBehaviour
{
    // Referencias al pa�s y su sprite
    [SerializeField] private Country _country;
    [SerializeField] private SpriteRenderer _countrySprite;
    [SerializeField] private TMP_Text _troopsAmountIndicator;


    // Al inicio, suscribe la funci�n SetColorToOwners al evento OnGameStart
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


    // Colorear pa�s seg�n el color de su due�o
    private void SetColorToOwners(object sender, System.EventArgs e)
    {
        _countrySprite.color = _country.Owner.PlayerColor;
    }
}
