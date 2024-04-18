using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
public class Dice : MonoBehaviour
{
    /*
    - Al principio se tira uno de los dados, el mayor comienza la ronda y asi sigue
    ATAQUE:
        -1 ejército → 1 dado

        -2 ejércitos → 2 dados

        -3 ejércitos → 3 dados

        -4 ejércitos → 3 dados

        -5 ejércitos → 3 dados
    * Atacante y defensor tiran los dados al mismo tiempo
    * Si uno de los jugadores tiene uno o mas dados que el otro esos dados
     no entran en juego
     */

    //Referencias a los objetos del canvas
    public GameObject OnClickDice;
    public GameObject OnClickRoll;
    public GameObject OnclickContinue;
    public TMP_Text diceResult; 
    public void rollDice()

    {   //Se crea un numero random
        System.Random random = new System.Random();
        
        int randomFace = random.Next(1, 7);
        //Al llamar a la funcion hace visibles el texto con el resultado del dado y el boton
        OnClickRoll.SetActive(true);
        OnclickContinue.SetActive(true);
        diceResult.text = randomFace.ToString(); 
        
    }

    public void exit()
    {//Hace invisibles el boton y el texto
        OnClickRoll.SetActive(false);
        OnclickContinue.SetActive(false);
    }

}


