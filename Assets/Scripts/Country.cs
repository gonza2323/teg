using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class Country : MonoBehaviour
{
    // Datos del pa�s
    [field: SerializeField] public string CountryName { get; private set; }
    [field: SerializeField] public List<Country> NeighboringCountries { get; private set; }
    [field: SerializeField] public Player Owner { get; private set; }
    public int ArmiesAmount { get; private set; } = 5;


    // Al hacer click en el pa�s, decir si pertenece al jugador actual
    private void OnMouseDown()
    {
        Player currentPlayer = GameManager.Instance.CurrentPlayer;
        if (currentPlayer == Owner)
            Debug.Log("You (" + currentPlayer.PlayerName + ") own " + CountryName + ".");
        else
            Debug.Log("You (" + currentPlayer.PlayerName + ") DO NOT own " + CountryName + ", " + Owner.PlayerName + " does.");
    }
}
