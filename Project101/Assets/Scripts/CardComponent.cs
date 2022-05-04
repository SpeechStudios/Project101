using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CardComponent : NetworkBehaviour
{
    [HideInInspector] public bool cardIsHeldOverBoard;
    [HideInInspector] public bool cardCanBePickedUp = true;
    [HideInInspector] public bool cardHasBeenPlayed;
    public Transform placeholder;
    public int Team;
    public Player player;
    public Card myCard;

    private void OnTriggerStay(Collider other)
    {
        //Check If the Card is within the parameters of the board
        if (other.CompareTag("BoardPosition") && placeholder.parent != player.myBoardManager.BoardPositionObject && player.myBoardManager.BoardHasSpace())
        {
            player.myBoardManager.MovePlaceholderToBoard(this, placeholder.GetSiblingIndex());
            cardIsHeldOverBoard = true;
            player.myHandManager.UpdateHandPositions();
            player.myBoardManager.UpdateBoardPositions();
        }
        if (other.CompareTag("HandPosition") && placeholder.parent != player.myHandManager.HandPositionObject && !cardIsHeldOverBoard)
        {
            player.myHandManager.MovePlaceholderToHand(this);
            player.myHandManager.UpdateHandPositions();
            player.myBoardManager.UpdateBoardPositions();
        }
    }
    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("BoardPosition"))
        {
            placeholder.SetParent(null);
            cardIsHeldOverBoard = false;
            player.myHandManager.UpdateHandPositions();
            player.myBoardManager.UpdateBoardPositions();
        }
        if (other.CompareTag("HandPosition"))
        {
            placeholder.SetParent(null);
            player.myHandManager.UpdateHandPositions();
            player.myBoardManager.UpdateBoardPositions();
        }
    } 
}
