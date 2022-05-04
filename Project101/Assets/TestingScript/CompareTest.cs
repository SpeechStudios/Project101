using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompareTest : MonoBehaviour
{
    List<Object> objects = new List<Object>();
    private void Start()
    {
        objects.Add(new Object(5, 5));
        objects.Add(new Object(1, 4));
        objects.Add(new Object(4, 1));
        objects.Add(new Object(5, 0));
    }
    public static int CompareHealth(Object object1, Object object2)
    {
        return object1.health.CompareTo(object2.health);
    }
    public static int CompareAttack(Object object1, Object object2)
    {
        return object1.attack.CompareTo(object2.attack);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.H))
        {
            objects.Sort(CompareHealth);
            Debugger();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            objects.Sort(CompareAttack);
            Debugger();
        }

    }
    void Debugger()
    {
        foreach (Object obj in objects)
        {
            Debug.Log("My Health:" + obj.health + "  My Attack: " + obj.attack);
        }
    }
}
public class Object
{
    public int health;
    public int attack;
    public Object(int newhealth, int newattack)
    {
        health = newhealth;
        attack = newattack;
    }
}

