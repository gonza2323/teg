using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;


public class Player : MonoBehaviour
{
    // Datos del jugador
    [field: SerializeField] public string PlayerName { get; private set; } = "DefaultName";
    [field: SerializeField] public Color PlayerColor { get; private set; } = Color.magenta;


    // Países que le pertenecen
    public List<Country> OwnedCountries { get; private set; } = new List<Country>();
}
