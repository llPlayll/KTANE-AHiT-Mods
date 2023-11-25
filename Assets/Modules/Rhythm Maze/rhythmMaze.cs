using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class rhythmMaze : MonoBehaviour
{
    [SerializeField] private KMBombInfo Bomb;
    [SerializeField] private KMAudio Audio;

    [SerializeField] private MeshRenderer ModuleRenderer;
    [SerializeField] private KMSelectable StartButton;
    [SerializeField] private MeshRenderer StartButtonRenderer;
    [SerializeField] private List<Material> StartButtonMaterials;

    bool deathWish;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    void Awake()
    {
        ModuleId = ModuleIdCounter++;
        /*
        foreach (KMSelectable object in keypad) {
            object.OnInteract += delegate () { keypadPress(object); return false; };
            }
        */

        //button.OnInteract += delegate () { buttonPress(); return false; };
        StartButton.OnInteract += delegate () { StartButtonPressed(); return false; };
    }

    void StartButtonPressed()
    {
        Log("Starting Module");
    }

    void Start()
    {
        if (Bomb.GetModuleIDs().Contains("snatchersMap"))
        {
            StartButtonRenderer.material = StartButtonMaterials[1];
            deathWish = true;
            Log("Looks like someone has a Death Wish...");
            Log("This bomb has a Snatcher's Map, the Time Rift has become unstable!");
        }
        else
        {
            StartButtonRenderer.material = StartButtonMaterials[0];
        }
    }

    void Log(string arg)
    {
        Debug.Log($"[Rhythm Maze #{ModuleId}] {arg}");
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
    }
}
