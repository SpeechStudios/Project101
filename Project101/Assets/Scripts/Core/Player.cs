using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    //Assigned by Server
    public int Team;
    [HideInInspector] public GameManager gameManager;
    [HideInInspector] public ClientRpcParams playerParams = new() { };

    [Space]
    [HideInInspector] public BoardManager myBoardManager;
    [HideInInspector] public HandManager myHandManager;
    [HideInInspector] public Transform cardDrawTransform;

    [Space]
    public List<Card> Deck;
    public GameObject cardPrefab;
    public override void OnNetworkSpawn()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        SetTeam();
        SetComponents();
    }
    private void SetTeam()
    {
        //Set the teams for both clients and the server and deligates who the active player is, also sets the enemy's team
        if (IsServer)
        {
            if (NetworkManager.Singleton.ConnectedClientsIds.Count == 1)
            {
                gameManager.player1 = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(OwnerClientId).GetComponent<Player>();
                gameManager.activePlayer = gameManager.player1;
                Team = 1;
            }
            else
            {
                gameManager.player2 = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(OwnerClientId).GetComponent<Player>();
                gameManager.InactivePlayer = gameManager.player2;
                Team = 2;
            }
            gameManager.PlayerChange();
        }
        else
        {
            if (IsLocalPlayer)
            {
                if (gameManager.playerCount.Value == 1)
                {
                    gameManager.player1 = this;
                    gameManager.activePlayer = gameManager.player1;
                    Team = 1;
                }
                else
                {
                    gameManager.player2 = this;
                    gameManager.InactivePlayer = gameManager.player2;
                    Team = 2;
                }
            }
            else
            {
                if (gameManager.playerCount.Value == 1)
                {
                    gameManager.player2 = this;
                    gameManager.InactivePlayer = gameManager.player2;
                    Team = 2;
                }
                else
                {
                    gameManager.player1 = this;
                    gameManager.activePlayer = gameManager.player1;
                    Team = 1;
                }
            }
        }
    }
    private void SetComponents()
    {
        //Get The Variables for the Player and Server, Function for Mirror placement: Each player is set as the Main player whilst the opponent is always the EnemyPlayer
        if (IsServer)
        {
            if (NetworkManager.Singleton.ConnectedClientsIds.Count == 1)
            {
                GameObject obj = GameObject.FindGameObjectWithTag("PlayerManager");
                myBoardManager = obj.GetComponent<BoardManager>();
                myHandManager = obj.GetComponent<HandManager>();
            }
            else
            {
                GameObject obj = GameObject.FindGameObjectWithTag("EnemyManager");
                myBoardManager = obj.GetComponent<BoardManager>();
                myHandManager = obj.GetComponent<HandManager>();
            }
        }
        else
        {

            if (IsLocalPlayer)
            {
                GameObject obj = GameObject.FindGameObjectWithTag("PlayerManager");
                myBoardManager = obj.GetComponent<BoardManager>();
                myHandManager = obj.GetComponent<HandManager>();
            }
            else
            {
                GameObject obj = GameObject.FindGameObjectWithTag("EnemyManager");
                myBoardManager = obj.GetComponent<BoardManager>();
                myHandManager = obj.GetComponent<HandManager>();
            }
        }
        cardDrawTransform = myHandManager.cardDrawPosition;
    }
}
