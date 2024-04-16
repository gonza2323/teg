using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CountryVisual : MonoBehaviour
{
    // Referencias al pa�s y su sprite
    [SerializeField] private Country _country;
    [SerializeField] private SpriteRenderer _countrySprite;


    // Al inicio, suscribe la funci�n SetColorToOwners al evento OnGameStart
    private void Awake()
    {
        GameManager.OnGameStart += SetColorToOwners;
    }


    // Colorear pa�s seg�n el color de su due�o
    private void SetColorToOwners(object sender, System.EventArgs e)
    {
        _countrySprite.color = _country.Owner.PlayerColor;
    }
}
