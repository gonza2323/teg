using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class Country : MonoBehaviour
{
    // Datos del país
    [field: SerializeField] public string Id { get; private set; }
    [field: SerializeField] public string CountryName { get; private set; }
    [field: SerializeField] public List<Country> NeighboringCountries { get; private set; }
    public Player owner;
    public int troopsAmount = 1;


    // Método para instanciar un país (usado solamente por Map Editor)
    public void Init(string id, string countryName)
    {
        Id = id;
        CountryName = countryName;
    }


    // Solo usado por Map Editor
    public void SetNeighbors(List<Country> neighboringCountries)
    {
        NeighboringCountries = neighboringCountries;
    }


    // Al hacer click en el país, decir si pertenece al jugador actual
    private void OnMouseDown()
    {
        Player currentPlayer = GameManager.Instance.CurrentPlayer;
        if (currentPlayer == owner)
            Debug.Log("You (" + currentPlayer.PlayerName + ") own " + CountryName + ".");
        else
            Debug.Log("You (" + currentPlayer.PlayerName + ") DO NOT own " + CountryName + ", " + owner.PlayerName + " does.");
    }
}
