using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DragCards : NetworkBehaviour
{
    private GameObject hoveredOverObject;
    private CardComponent selectedCard;
    private BoardManager boardManager;
    private HandManager handManager;
    private Player player;
    private GameManager gameManager;
    private Camera myCam;


    private readonly float PlayableAreaYPos = -9f; //Make sure this value syncs up with Y Value of the Playablearea transform position in game Hierarchy

    // Update is called once per frame
    private void Start()
    {
        player = GetComponent<Player>();
        boardManager = player.myBoardManager;
        handManager = player.myHandManager;
        gameManager = player.gameManager;
        myCam = Camera.main;
    }
    void Update()
    {
        if (IsOwner)
        {
            TargetRay();
            if (gameManager.IsItMyTurn(player))
            {
                MyControls();
            }
        }
    }
    private void MyControls()
    {
        if (Input.GetButtonDown("Fire1") && hoveredOverObject != null)
        {
            EndTurn(); // Check For end turn
            DrawACard(); // Check For Draw //Testing
            PickUpCard(); //Check For Card
            RemoveACard(); //Check For Minion //Testing
        }
        if (selectedCard != null)
        {
            DraggingCard(); // If Card Was Picked Up, Start Dragging Function
            if (Input.GetButtonUp("Fire1"))
            {
                ReleaseCard(); //Release Card To Zone Based On What Its Colliding With
            }
        }
    }
    private void TargetRay()
    {
        //Casts ray at mouse position
        Ray ray = myCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            hoveredOverObject = hit.transform.gameObject;
        }
        else
        {
            hoveredOverObject = null;
        }
    }
    private void PickUpCard()
    {
        //Checks if the card can be picked up and picks it up if returned true
        if (CardCanBePlayed(hoveredOverObject.GetComponent<CardComponent>()))
        {
            selectedCard = hoveredOverObject.GetComponent<CardComponent>();
            selectedCard.GetComponent<CardRotateInDrag>().Reset();
            selectedCard.GetComponent<CardRotateInDrag>().enabled = true;
            Cursor.visible = false;
            handManager.RemoveCardFromHand(selectedCard);
        }
    }
    private void DraggingCard()
    {
        //Moves the card to the position of the mouse based on the card's y axis
        Cursor.visible = false;
        Vector3 mousePosition = new(Input.mousePosition.x, Input.mousePosition.y, myCam.WorldToScreenPoint(selectedCard.transform.position).z);
        Vector3 worldPosition = myCam.ScreenToWorldPoint(mousePosition);
        selectedCard.transform.position = new Vector3(worldPosition.x, PlayableAreaYPos, worldPosition.z);
        if (selectedCard.placeholder.parent)
        {
            AdjustPlaceHolderPosition();
        }
    }
    private void AdjustPlaceHolderPosition()
    {
        int newSiblingIndex = selectedCard.placeholder.parent.childCount - 1;
        for (int i = 0; i < selectedCard.placeholder.parent.childCount; i++)
        {
            if (selectedCard.transform.position.x > selectedCard.placeholder.parent.GetChild(i).position.x) // Checks the x position of the card in relation to the other cards
            {
                newSiblingIndex = i;
                if (selectedCard.placeholder.transform.GetSiblingIndex() < newSiblingIndex)
                {
                    newSiblingIndex -= 1;
                }

                break;
            }
        }
        if (newSiblingIndex != selectedCard.placeholder.transform.GetSiblingIndex()) //Update Hand/BoardPositions when a change of position has been made
        {
            if (selectedCard.placeholder.parent == handManager.HandPositionObject)
            {
                handManager.RearrangeHand();
            }
            if (selectedCard.placeholder.parent == boardManager.BoardPositionObject)
            {
                boardManager.RearrangeBoard();
            }
            selectedCard.placeholder.transform.SetSiblingIndex(newSiblingIndex);
        }
    }
    private void ReleaseCard()
    {
        //Drops the card based on its position
        if (selectedCard.cardIsHeldOverBoard && boardManager.BoardHasSpace()) //If the card is on the board drop it like a minion
        { 
            gameManager.PlayCardRequestServerRpc(selectedCard.GetComponent<NetworkObject>(), selectedCard.placeholder.transform.GetSiblingIndex());
            //Trigger PlayCard Animation
        }
        else
        {
            if (selectedCard.placeholder.parent != handManager.HandPositionObject) // If card is not in the hand, return it to the hand
            {
                handManager.MovePlaceholderToHand(selectedCard);
            }
            handManager.EndArrangement(selectedCard);
        }
        selectedCard.GetComponent<CardRotateInDrag>().enabled = false;
        selectedCard = null;
        Cursor.visible = true;
    }
    private bool CardCanBePlayed(CardComponent card)
    {
        //Set Dependecies for card
        if(card != null)
        {
            if (card.GetComponent<CardComponent>().cardCanBePickedUp && card.Team == player.Team)
            {
                return true;
            }
            return false;
        }
        return false;
    }
    private void EndTurn()
    {
        if(hoveredOverObject.CompareTag("Finish"))
        {
            gameManager.EndTurnRequestServerRpc();
            return;
        }
    }

    //Functions For Testing
    private void DrawACard()
    {
        //For Testing Purposes
        if (hoveredOverObject.CompareTag("Player") && handManager.HandHasSpace()) // If we click to draw then deck needs a team component
        {
            gameManager.DrawCardRequestServerRpc();
            return;
        }
    }
    private void RemoveACard()
    {
        //For Testing Purposes
        if (hoveredOverObject.GetComponent<CardComponent>() !=null)
        {
            if (hoveredOverObject.GetComponent<CardComponent>().cardHasBeenPlayed && hoveredOverObject.GetComponent<CardComponent>().Team !=player.Team)
            {
                gameManager.RemoveCardRequestServerRpc(hoveredOverObject.GetComponent<NetworkObject>());
                return;
            }
        }
    }

}
