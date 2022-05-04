using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExternalFunctions
{
    // Start is called before the first frame update
    public static void CardTween(CardComponent card, Vector3 EndPosition, Vector3 EndRotation, float Duration)
    {
        if(card.GetComponent<Collider>())
        {
            card.GetComponent<Collider>().enabled = false;
        }
        LeanTween.move(card.gameObject, EndPosition, Duration);
        LeanTween.rotate(card.gameObject, EndRotation, Duration).setOnComplete(() => card.GetComponent<Collider>().enabled = true);
    }
}
