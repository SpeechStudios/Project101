using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class GameManager : NetworkBehaviour
{
    [HideInInspector] public Player activePlayer,InactivePlayer;
    [HideInInspector] public Player player1, player2;
    [HideInInspector] public NetworkVariable<int> playerCount = new();
    bool gameStarted;

    public void PlayerChange()
    {
        playerCount.Value = NetworkManager.Singleton.ConnectedClientsIds.Count;
    }
    private void LateUpdate()
    {
        if (playerCount.Value == 2 && !gameStarted && IsServer)
        {
            gameStarted = true;
            player1.playerParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { player1.GetComponent<NetworkObject>().OwnerClientId } } };
            player2.playerParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { player2.GetComponent<NetworkObject>().OwnerClientId } } };
            DrawCard(activePlayer, InactivePlayer);
            StartGameClientRpc();
        }
    }
    [ClientRpc]
    private void StartGameClientRpc(ClientRpcParams rpcParams = default)
    {
        player1.GetComponent<DragCards>().enabled = true;
        player2.GetComponent<DragCards>().enabled = true;
    }

    //Handles End Turn Functionality
    [ServerRpc(RequireOwnership = false)]
    public void EndTurnRequestServerRpc()
    {
        activePlayer = activePlayer == player1 ? player2 : player1;
        InactivePlayer = InactivePlayer == player2 ? player1 : player2;
        EntdTurnClientRpc();
        DrawCard(activePlayer, InactivePlayer);
    }
    [ClientRpc]
    void EntdTurnClientRpc(ClientRpcParams rpcParams = default)
    {
        activePlayer = activePlayer == player1 ? player2 : player1;
        InactivePlayer = InactivePlayer == player2 ? player1 : player2;
    }

    
    //Handles All the Draw Card Functionality
    [ServerRpc(RequireOwnership = false)]
    public void DrawCardRequestServerRpc(ServerRpcParams serverRpcParams = default)
    {
        DrawCard(activePlayer, InactivePlayer);
    }
    public void DrawCard(Player player, Player enemyPlayer)
    {
        if (IsServer)
        {
            if (player.myHandManager.HandHasSpace() && player.Deck.Count > 0)
            {

                GameObject cardObject = Instantiate(player.cardPrefab, player.cardDrawTransform.position, player.cardDrawTransform.rotation);
                cardObject.GetComponent<NetworkObject>().Spawn();
                CardComponent card = cardObject.GetComponent<CardComponent>();
                card.myCard = player.Deck[0];
                card.player = player;
                card.Team = player.Team;
                player.myHandManager.AddCardToHand(card);
                player.Deck.RemoveAt(0);
                DrawCardForPlayerClientRPC(cardObject.GetComponent<NetworkObject>(), player.GetComponent<NetworkObject>(), player.playerParams);
                DrawCardForEnemyClientRPC(cardObject.GetComponent<NetworkObject>(), player.GetComponent<NetworkObject>(), enemyPlayer.playerParams);
            }
        }
    }
    [ClientRpc]
    public void DrawCardForPlayerClientRPC(NetworkObjectReference card, NetworkObjectReference player, ClientRpcParams clientRpcParams)
    {
        if (card.TryGet(out NetworkObject cardTarget) && player.TryGet(out NetworkObject playerTarget))
        {
            if(playerTarget.GetComponent<Player>() == player2)
            {
                cardTarget.transform.position = player2.cardDrawTransform.position;
            }
            Player playerComponent = playerTarget.GetComponent<Player>();
            CardComponent cardComponent = cardTarget.GetComponent<CardComponent>();
            cardComponent.myCard = playerComponent.Deck[0];
            cardComponent.player = playerComponent;
            cardComponent.Team = playerComponent.Team;
            playerComponent.myHandManager.AddCardToHand(cardComponent);
            playerComponent.Deck.RemoveAt(0);
        }
    }
    [ClientRpc]
    public void DrawCardForEnemyClientRPC(NetworkObjectReference card, NetworkObjectReference player, ClientRpcParams clientRpcParams)
    {
        if (card.TryGet(out NetworkObject cardTarget) && player.TryGet(out NetworkObject playerTarget))
        {
            if (playerTarget.GetComponent<Player>() == player1)
            {
                cardTarget.transform.position = player1.cardDrawTransform.position;
            }
            Player playerComponent = playerTarget.GetComponent<Player>();
            CardComponent cardComponent = cardTarget.GetComponent<CardComponent>();
            cardComponent.player = playerComponent;
            cardComponent.Team = playerComponent.Team;
            playerComponent.myHandManager.AddCardToHand(cardComponent);
            playerComponent.Deck.RemoveAt(0);
        }
    }


    //Handles All the Play Card Functionallity
    [ServerRpc(RequireOwnership = false)]
    public void PlayCardRequestServerRpc(NetworkObjectReference card, int index, ServerRpcParams serverRpcParams = default)
    {
        if (card.TryGet(out NetworkObject cardTarget))
        {
            CardComponent cardComponent = cardTarget.GetComponent<CardComponent>();
            activePlayer.myHandManager.RemoveCardFromHand(cardComponent);
            activePlayer.myBoardManager.MovePlaceholderToBoard(cardComponent, index);
            activePlayer.myBoardManager.AddMinionToBoard(cardComponent);
            cardComponent.cardCanBePickedUp = false;
            cardComponent.cardHasBeenPlayed = true;
            PlayCardClientRPC(cardTarget, index);
        }
    }
    [ClientRpc]
    private void PlayCardClientRPC(NetworkObjectReference card, int index, ClientRpcParams clientRpcParams = default)
    {
        if (card.TryGet(out NetworkObject cardTarget))
        {
            CardComponent cardComponent = cardTarget.GetComponent<CardComponent>();
            if(activePlayer.myHandManager.HasCardInHand(cardComponent))
            {
                activePlayer.myHandManager.RemoveCardFromHand(cardComponent);
            }
            activePlayer.myBoardManager.MovePlaceholderToBoard(cardComponent, index);
            activePlayer.myBoardManager.AddMinionToBoard(cardComponent);
            cardComponent.cardCanBePickedUp = false;
            cardComponent.cardHasBeenPlayed = true;
        }
    }


    //Handlles Remove Card Functionallity
    [ServerRpc(RequireOwnership = false)]
    public void RemoveCardRequestServerRpc(NetworkObjectReference card, ServerRpcParams serverRpcParams = default)
    {
        if (card.TryGet(out NetworkObject cardTarget))
        {
            RemoveCardClientRPC(cardTarget.GetComponent<NetworkObject>());
            InactivePlayer.myBoardManager.RemoveCardFromBoard(cardTarget.GetComponent<CardComponent>());
        }
    }
    [ClientRpc]
    private void RemoveCardClientRPC(NetworkObjectReference card, ClientRpcParams clientRpcParams = default)
    {
        if (card.TryGet(out NetworkObject cardTarget))
        {
            InactivePlayer.myBoardManager.RemoveCardFromBoard(cardTarget.GetComponent<CardComponent>());
        }
    }


    public bool IsItMyTurn(Player player)
    {
        return player == activePlayer;
    }
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        GUILayout.EndArea();
    }
    static void StartButtons()
    {
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    }
}
