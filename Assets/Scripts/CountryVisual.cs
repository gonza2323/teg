using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CountryVisual : MonoBehaviour
{
    // Referencias al país y su sprite
    [SerializeField] private Country _country;
    [SerializeField] private SpriteRenderer _countrySprite;


    // Al inicio, suscribe la función SetColorToOwners al evento OnGameStart
    private void Awake()
    {
        GameManager.OnGameStart += SetColorToOwners;
    }


    // Colorear país según el color de su dueño
    private void SetColorToOwners(object sender, System.EventArgs e)
    {
        _countrySprite.color = _country.Owner.PlayerColor;
    }
}
