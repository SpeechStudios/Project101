using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class BoardManager : MonoBehaviour
{
    public RectTransform BoardPositionObject;
    public Transform Graveyard;

    private readonly int maxCardsOnBoard = 7;
    private readonly List<CardComponent> CardsOnBoard = new();

    public void AddMinionToBoard(CardComponent card)
    {
        CardsOnBoard.Add(card);
        UpdateBoardPositions();
    }
    public void RemoveCardFromBoard(CardComponent card)
    {
        Destroy(card.placeholder.gameObject);
        CardsOnBoard.Remove(card);
        Invoke(nameof(UpdateBoardPositions), 0.01f);

        //Replace With Destroy Animation Potentially
        LeanTween.move(card.gameObject, Graveyard.position, 0.5f);
        LeanTween.rotate(card.gameObject, Graveyard.rotation.eulerAngles, 0.5f).setOnComplete(() => { if (NetworkManager.Singleton.IsServer) Destroy(card.gameObject); }); // Destroys card when enters graveyard
    }
    public void MovePlaceholderToBoard(CardComponent card, int index)
    {
        card.placeholder.SetParent(BoardPositionObject);
        card.placeholder.SetSiblingIndex(index);
        card.placeholder.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
    }
    public void RearrangeBoard()
    {
        Invoke(nameof(UpdateBoardPositions), 0.01f); // Invokes Function at end of frame because the new positions dont register without the delay, Can be optimised by creating a custom layout group
    }
    public void UpdateBoardPositions()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(BoardPositionObject);
        foreach (CardComponent cardOnBoard in CardsOnBoard)
        {
            ExternalFunctions.CardTween(cardOnBoard, cardOnBoard.placeholder.position, Vector3.zero, 0.1f);
        }
    }
    public bool BoardHasSpace()
    {
        if (CardsOnBoard.Count < maxCardsOnBoard)
        {
            return true;
        }
        return false;
    }
}
