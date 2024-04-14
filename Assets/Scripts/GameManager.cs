using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    public int RoundNo { get; private set; } = 0;
    public List<Player> players;
    public List<Country> countries;
    public Player CurrentPlayer { get; private set; }
    private int currentPlayerIndex;

    public event EventHandler OnRoundStart;
    public event EventHandler OnTurnStart;
    public event EventHandler OnTurnEnd;
    public event EventHandler OnRoundEnd;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        foreach (Country country in countries) {
            country.owner.ownedCountries.Add(country);
        }

        StartRound();
    }

    private void StartRound() {
        RoundNo++;
        currentPlayerIndex = 0;
        CurrentPlayer = players[currentPlayerIndex];
        Debug.Log("Round " + RoundNo + " started.");
        OnRoundStart?.Invoke(this, EventArgs.Empty);

        currentPlayerIndex = -1;
        StartTurn();
    }

    private void StartTurn() {
        currentPlayerIndex++;
        CurrentPlayer = players[currentPlayerIndex];
        Debug.Log(CurrentPlayer.playerName + "'s turn started.");
        OnTurnStart?.Invoke(this, EventArgs.Empty);
    }

    public void EndTurn() {
        Debug.Log(CurrentPlayer.playerName + "'s turn ended.");
        OnTurnEnd?.Invoke(this, EventArgs.Empty);
        
        if (currentPlayerIndex + 1 < players.Count) {
            StartTurn();
        } else {
            EndRound();
        }
    }

    private void EndRound() {
        Debug.Log("Round " + RoundNo + " ended.");
        OnRoundEnd?.Invoke(this, EventArgs.Empty);
        StartRound();
    }
}
