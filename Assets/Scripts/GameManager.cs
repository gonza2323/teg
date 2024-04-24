using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    
    // Instancia ˙nica de GameManager
    public static GameManager Instance;


    // Eventos del juego
    public static event EventHandler OnGameStart;
    public static event EventHandler OnRoundStart;
    public static event EventHandler OnTurnStart;
    public static event EventHandler OnTurnEnd;
    public static event EventHandler OnRoundEnd;


    // Mapa y jugadores
    [SerializeField] private List<Country> countries;
    [SerializeField] private List<Player> players;


    // Estado del juego
    public int RoundNo { get; private set; } = 0;
    private int _currentPlayerIndex;
    public Player CurrentPlayer { get; private set; }


    // Crear instancia de GameManager
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Debug.LogError("No puede haber m·s de una instancia de GameManager!");
    }


    // Antes de empezar juego
    private void Start()
    {
        // Agregar pa˙êes a su dueÒo
        // TODO: Los pa˙êes deben sortearse
        foreach (Country country in countries)
            country.Owner.OwnedCountries.Add(country);



        // Empezar juego
        StartGame();
    }


    // Empezar juego
    private void StartGame()
    {
        Debug.Log("Game started");
        OnGameStart?.Invoke(this, EventArgs.Empty);
        StartRound();
    }


    // Empezar ronda
    private void StartRound()
    {
        RoundNo++;
        _currentPlayerIndex = 0;
        CurrentPlayer = players[_currentPlayerIndex];
        Debug.Log("Round " + RoundNo + " started.");
        OnRoundStart?.Invoke(this, EventArgs.Empty);

        _currentPlayerIndex = -1;
        StartTurn();
    }


    // Empezar turno
    private void StartTurn()
    {
        _currentPlayerIndex++;
        CurrentPlayer = players[_currentPlayerIndex];
        Debug.Log(CurrentPlayer.PlayerName + "'s turn started.");
        OnTurnStart?.Invoke(this, EventArgs.Empty);
    }


    // Terminar turno
    public void EndTurn()
    {
        Debug.Log(CurrentPlayer.PlayerName + "'s turn ended.");
        OnTurnEnd?.Invoke(this, EventArgs.Empty);
        
        if (_currentPlayerIndex + 1 < players.Count)
            StartTurn();
        else
            EndRound();
    }


    // Terminar ronda
    private void EndRound()
    {
        Debug.Log("Round " + RoundNo + " ended.");
        OnRoundEnd?.Invoke(this, EventArgs.Empty);
        StartRound();
    }
}
