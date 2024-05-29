using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class Country : MonoBehaviour
{
    public event EventHandler OnUIShouldUpdate;

    // Datos del país
    [field: SerializeField] public string Id { get; private set; }
    [field: SerializeField] public string CountryName { get; private set; }
    [field: SerializeField] public List<Country> NeighboringCountries { get; private set; }
    public Player owner;
    public int troopsAmount = 5;


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
    public void OnClick()
    {
        GameManager game = GameManager.Instance;
        Player currentPlayer = game.CurrentPlayer;

        if (owner == currentPlayer)
        {
            game.SelectedPrimaryCountry = this;
            Debug.Log("Selected attacking country " + CountryName);
        }
        else
        {
            if (game.SelectedPrimaryCountry != null)
            {
                if (game.SelectedPrimaryCountry.NeighboringCountries.Contains(this))
                {
                    game.Attack(game.SelectedPrimaryCountry, this);
                } else
                    Debug.Log($"You can only attack countries that neighboring countries");
            } else
                Debug.Log("You must first select a country from which to attack another");
        }
    }


    public void UpdateUI()
    {
        OnUIShouldUpdate?.Invoke(this, EventArgs.Empty);
    }
}
