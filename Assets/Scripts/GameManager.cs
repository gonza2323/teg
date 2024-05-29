using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Schema;
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
    public Country SelectedPrimaryCountry;
    public Country SelectedSecondaryCountry;


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


    public void Attack(Country attackerCountry, Country defenderCountry)
    {
        if (attackerCountry == null || defenderCountry == null ||
            !attackerCountry.NeighboringCountries.Contains(defenderCountry) ||
            attackerCountry.owner != CurrentPlayer ||
            defenderCountry.owner == CurrentPlayer)
            return;

        int attackerDiceNo = Math.Min(attackerCountry.troopsAmount - 1, 3);
        int defenderDiceNo = Math.Min(defenderCountry.troopsAmount, 3);

        if (attackerDiceNo == 0)
            return;

        List<int> attackerDice = ThrowDice(attackerDiceNo).OrderByDescending(n => n).ToList();
        List<int> defenderDice = ThrowDice(defenderDiceNo).OrderByDescending(n => n).ToList();

        for (int i = 0; i < Math.Min(attackerDiceNo, defenderDiceNo); i++)
        {
            Debug.Log($"{attackerDice[i]} vs {defenderDice[i]}");
            if (attackerDice[i] > defenderDice[i])
                defenderCountry.troopsAmount--;
            else
                attackerCountry.troopsAmount--;
        }

        if (defenderCountry.troopsAmount == 0)
        {
            Debug.Log($"Conquistaste {defenderCountry.CountryName}!");
            attackerCountry.troopsAmount--;
            defenderCountry.troopsAmount = 1;
            defenderCountry.owner = attackerCountry.owner;
        }

        attackerCountry.UpdateUI();
        defenderCountry.UpdateUI();
    }


    private List<int> ThrowDice(int numberOfDice)
    {
        var random = new System.Random();
        List<int> dice = new List<int>();

        for (int i = 0; i < numberOfDice; i++)
            dice.Add(random.Next(1, 7));

        return dice;
    }
}


