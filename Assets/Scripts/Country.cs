using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Country : MonoBehaviour
{
    public string countryName;
    public List<Country> neighboringCountries;
    public Player owner;
    public uint armiesAmount;

    private void OnMouseDown() {
        if (GameManager.Instance.CurrentPlayer == owner) {
            Debug.Log("You, " + owner.playerName + ", are the owner of " + countryName);
        } else {
            Debug.Log("You're NOT the owner of " + countryName + ", " + owner.playerName + " is.");
        }
    }
}
