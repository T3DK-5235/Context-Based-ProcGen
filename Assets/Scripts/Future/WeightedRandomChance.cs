using System.Linq;
using UnityEngine;
using System;
using Unity.VisualScripting;

//TODO remove monobehaviour after testing
public class WeightedRandomChance : MonoBehaviour
{
    //TODO figure out why the hell this breaks half way through.
    // void Start()
    // {
    //     int[] chances = new int[4] { 15, 20, 20, 25 };
    //     //GetWeightedRandomValue(chances);

    //     GetWeightedRandomOrdering(chances);

    //     // int[] outcomes = new int[20];
    //     // for (int i = 0; i < 20; i++)
    //     // {
    //     //     outcomes[i] = GetWeightedRandomOrdering(chances);
    //     // }

    //     // string outcomeString = "Results from 20 runs: ";
    //     // for (int j = 0; j < 20; j++) { outcomeString += outcomes[j] + ", "; }
    //     // Debug.Log(outcomeString);
    // }

    // public int GetWeightedRandomOrdering(int[] weightedChances)
    // {
    //     // int[] weightedOutput = new int[weightedChances.Length];

    //     // for (int i = 0; i < weightedChances.Length; i++)
    //     // {
    //     //     int chosenValue = GetWeightedRandomValue(weightedChances);
    //     //     Debug.Log("chosen value: " + chosenValue);
    //     //     weightedOutput[i] = chosenValue;
    //     //     weightedChances[chosenValue] = 0;
    //     // }

    //     int[] weightedOutput = new int[weightedChances.Length];
    //     int initialChanceNum = weightedChances.Length;
    //     for (int i = 0; i < initialChanceNum; i++)
    //     {
    //         int chosenValue = GetWeightedRandomValue(weightedChances);
    //         Debug.Log("chosen value: " + chosenValue);
    //         weightedOutput[i] = chosenValue;
    //         // weightedChances = weightedChances.Except(new int[] { chosenValue });
    //         weightedChances = weightedChances.Where((val, idx) => idx != chosenValue).ToArray();
    //     }

    //     string outcomeString = "Result order: ";
    //     for (int i = 0; i < weightedOutput.Length; i++)
    //     {
    //         outcomeString += weightedOutput[i] + ", ";
    //     }
    //     Debug.Log(outcomeString);

    //     return -1;
    // }

    // public int GetWeightedRandomValue(int[] weightedChances)
    // {

    //     string outcomeString = "weightedChances: ";
    //     for (int i = 0; i < weightedChances.Length; i++)
    //     {
    //         outcomeString += weightedChances[i] + ", ";
    //     }
    //     Debug.Log(outcomeString);

    //     int[] adjustedChances = new int[weightedChances.Length];
    //     Array.Copy(weightedChances, adjustedChances, weightedChances.Length);

    //     Array.Sort(adjustedChances);
    //     int randomValue = UnityEngine.Random.Range(0, adjustedChances.Sum());
    //     Debug.Log("adjustedChances sum: " + adjustedChances.Sum() + " --- random value: " + randomValue);

    //     Debug.Log("adjustedChances length: " + adjustedChances.Length);
    //     for (int i = 1; i < adjustedChances.Length; i++)
    //     {
    //         Debug.Log("adjusted chance: " + adjustedChances[i]);
    //         adjustedChances[i] = adjustedChances[i] + adjustedChances[i - 1];
    //         Debug.Log("adjusted chance: " + adjustedChances[i]);
    //     }

    //     for (int j = 0; j < adjustedChances.Length; j++)
    //     {
    //         if ((randomValue - adjustedChances[j]) < 0) { return j; }
    //     }

    //     return 5;
    // }
}
