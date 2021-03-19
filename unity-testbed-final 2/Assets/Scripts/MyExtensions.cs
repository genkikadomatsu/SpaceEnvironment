using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MyExtensions
{

    public static string NumberToWords(int number)
    {
        if (number == 0)
            return "zero";

        if (number < 0)
            return "minus " + NumberToWords(Math.Abs(number));

        string words = "";

        if ((number / 1000000) > 0)
        {
            words += NumberToWords(number / 1000000) + " million ";
            number %= 1000000;
        }

        if ((number / 1000) > 0)
        {
            words += NumberToWords(number / 1000) + " thousand ";
            number %= 1000;
        }

        if ((number / 100) > 0)
        {
            words += NumberToWords(number / 100) + " hundred ";
            number %= 100;
        }

        if (number > 0)
        {
            if (words != "")
                words += "and ";

            var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
            var tensMap = new[] { "zero", "Ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

            if (number < 20)
                words += unitsMap[number];
            else
            {
                words += tensMap[number / 10];
                if ((number % 10) > 0)
                    words += "-" + unitsMap[number % 10];
            }
        }

        return words;
    }
    
    public static GameObject GetGameObjectByNameAndPosition(string name, Vector3 position)
    {
        GameObject[] all = Transform.FindObjectsOfType<GameObject>().Where(obj => obj.name == name).ToArray<GameObject>();
        
        float minDist = 999;
        GameObject closest = null;
        //Debug.Log(name);
        foreach (GameObject g in all)
        {
            
            float dist = Vector3.Distance(position, g.transform.position);
            // Debug.Log(g.name + " " + dist);
            if (dist < minDist)
            {
                minDist = dist;
                closest = g;              
            }
        }

        return closest;
    }

    public static  string Number2String(int number, bool isCaps)
    {
        Char c = (Char)((isCaps ? 65 : 97) + (number - 1));

        return c.ToString();
    }

    public static Vector2 XZ(this Vector3 pos) { return new Vector2(pos.x, pos.z); }
}
