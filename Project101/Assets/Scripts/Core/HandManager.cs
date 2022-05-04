using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandManager : MonoBehaviour
{
    public RectTransform HandPositionObject;
    public Transform cardDrawPosition;

    private readonly int maxCardsInHand = 7;
    private readonly List<CardComponent> CardsInHand = new();
    
    public void AddCardToHand(CardComponent card)
    {
        CreatePlaceHolder(card);
        CardsInHand.Add(card);
        UpdateHandPositions();
    }
    private void CreatePlaceHolder(CardComponent card)
    {
        GameObject placeholder = new();
        placeholder.AddComponent<RectTransform>();
        placeholder.transform.SetParent(HandPositionObject);
        placeholder.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        card.placeholder = placeholder.transform;
    }
    public void RemoveCardFromHand(CardComponent card)
    {
        if (HasCardInHand(card))
        {
            CardsInHand.Remove(card);
            Invoke(nameof(UpdateHandPositions), 0.01f);
        }
    }
    public void MovePlaceholderToHand(CardComponent card)
    {
        card.placeholder.SetParent(HandPositionObject);
        card.placeholder.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
    }
    public void RearrangeHand()
    {
        Invoke(nameof(UpdateHandPositions), 0.01f); // Invokes Function at end of frame because the new positions dont register without the delay, Can be optimised by creating a custom layout group
    }
    public void EndArrangement(CardComponent card)
    {
        CardsInHand.Add(card);
        UpdateHandPositions();
    }
    public void UpdateHandPositions()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(HandPositionObject);
        foreach (CardComponent cardInHand in CardsInHand)
        {
            ExternalFunctions.CardTween(cardInHand, cardInHand.placeholder.position, Vector3.zero, 0.1f);
        }
    }
    public bool HandHasSpace()
    {
        if (CardsInHand.Count < maxCardsInHand)
        {
            return true;
        }
        return false;
    }
    public bool HasCardInHand( CardComponent card)
    {
        if(CardsInHand.Contains(card))
        {
            return true;
        }
        return false;
    }
}
