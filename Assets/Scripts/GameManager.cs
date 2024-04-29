using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    
    // Instancia única de GameManager
    public static GameManager Instance;


    // Eventos del juego
    public static event EventHandler OnGameStart;
    public static event EventHandler OnRoundStart;
    public static event EventHandler OnTurnStart;
    public static event EventHandler OnTurnEnd;
    public static event EventHandler OnRoundEnd;


    // Mapa y jugadores
    [field: SerializeField] public List<Country> Countries { get; private set; }
    [field: SerializeField] public List<Player> Players { get; private set; }


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
            Debug.LogError("No puede haber más de una instancia de GameManager!");
    }


    // Antes de empezar juego
    private void Start()
    {
        // Repartir países y agergarlos a su dueño
        for (int i = 0; i < Countries.Count; i++)
        {
            Player player = Players[i % Players.Count];
            Countries[i].owner = player;
            player.OwnedCountries.Add(Countries[i]);
        }

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
        CurrentPlayer = Players[_currentPlayerIndex];
        Debug.Log("Round " + RoundNo + " started.");
        OnRoundStart?.Invoke(this, EventArgs.Empty);

        _currentPlayerIndex = -1;
        StartTurn();
    }


    // Empezar turno
    private void StartTurn()
    {
        _currentPlayerIndex++;
        CurrentPlayer = Players[_currentPlayerIndex];
        Debug.Log(CurrentPlayer.PlayerName + "'s turn started.");
        OnTurnStart?.Invoke(this, EventArgs.Empty);
    }


    // Terminar turno
    public void EndTurn()
    {
        Debug.Log(CurrentPlayer.PlayerName + "'s turn ended.");
        OnTurnEnd?.Invoke(this, EventArgs.Empty);
        
        if (_currentPlayerIndex + 1 < Players.Count)
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
