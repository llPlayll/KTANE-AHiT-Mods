using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class riftRoulette : MonoBehaviour
{
    [SerializeField] private KMBombInfo Bomb;
    [SerializeField] private KMAudio Audio;

    [SerializeField] private GameObject DeactivatedIcon;
    [SerializeField] private List<Material> IconMaterials;

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
    }

    void Start()
    {
        if (Bomb.GetModuleIDs().Contains("rhythmMaze"))
        {
            DeactivatedIcon.SetActive(true);
            DeactivatedIcon.GetComponent<MeshRenderer>().material = IconMaterials[Bomb.GetModuleIDs().Contains("snatchersMap") ? 1 : 0];
        }
        else
        {
            DeactivatedIcon.SetActive(false);
            Activate();
        }
    }

    void Activate()
    {

    }

    void Log(string arg)
    {
        Debug.Log($"[Rift Roulette #{ModuleId}] {arg}");
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
