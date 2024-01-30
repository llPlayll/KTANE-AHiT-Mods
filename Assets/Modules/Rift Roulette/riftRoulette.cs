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
    [SerializeField] private GameObject SlotsParent;
    [SerializeField] private List<GameObject> Slots;

    List<string> itemNames = new List<string>() {"Cardboard", "Anarchy", "Pizza Time", "Virtual Kid", "Citrus", "Old Film", "Wireframe", "Obnoxious", "Blue Comet", "The Forest Critter", "City Girl", "Punk Set", "Milky Way", "Shadow Puppet", "Transcendent", "Too Hot to Handle", "Ice Hat", "Sprint Hat", "Brewing Hat", "Dweller Mask", "Time Stop Hat", "Kid's Hat" };
    List<string> cosmicMods = new List<string>() { "Astrological", "spwizAstrology", "cruelStars", "earth", "exoplanets", "jupiter", "mars", "matchRefereeing", "mercury", "neptune", "nomai", "planets", "planetX", "pluto", "saturn", "xelSpace", "stars", "syzygyModule", "uranus", "venus"};
    int startDay;
    int startMin;

    List<int> basePool = new List<int>() { };
    List<int> finalPool = new List<int>() { };
    int goalItem;
    int goalItemIdx;

    int rolled;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    void Awake()
    {
        ModuleId = ModuleIdCounter++;
        foreach (GameObject slot in Slots) {
            slot.GetComponent<KMSelectable>().OnInteract += delegate () { InitSlotRoll(slot); return false; };
            }

        //button.OnInteract += delegate () { buttonPress(); return false; };
    }

    void InitSlotRoll(GameObject slot)
    {

    }

    void Start()
    {
        startDay = Convert.ToInt32(DateTime.Now.ToString("dd"));
        startMin = (int)Bomb.GetTime() / 60;
        if (Bomb.GetModuleIDs().Contains("rhythmMaze"))
        {
            Log($"The module detected a Rhythm Maze module on the bomb. The module will not activate untill all of the Rhythm Mazes are solved.");
            DeactivatedIcon.SetActive(true);
            DeactivatedIcon.GetComponent<MeshRenderer>().material = IconMaterials[Bomb.GetModuleIDs().Contains("snatchersMap") ? 1 : 0];
            SlotsParent.SetActive(false);
            StartCoroutine("WaitForSolves");
        }
        else
        {
            Activate();
        }
    }

    void Activate()
    {
        Log("Module Activated!");
        DeactivatedIcon.SetActive(false);
        SlotsParent.SetActive(true);
        foreach (GameObject slot in Slots)
        {
            slot.GetComponent<MeshRenderer>().material.color = Color.gray;
        }
        Slots[0].GetComponent<MeshRenderer>().material.color = Color.white;

        GenBasePool();
        GenFinalPool();

        foreach (char letter in Bomb.GetSerialNumberLetters())
        {
            goalItemIdx += "ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(letter) + 1;
        }
        goalItem = basePool[(goalItemIdx - 1) % basePool.Count];
        Log($"The Goal Item is at position {goalItemIdx} and it is {itemNames[goalItem]}.");
    }

    void GenBasePool()
    {
        string basePoolLog = "";
        for (int i = 0; i < 22; i++)
        {
            bool val = false;
            switch (i)
            {
                case 0:
                    val = (Bomb.GetBatteryCount() % 2 == 0);
                    break;
                case 1:
                    val = (Bomb.GetModuleNames().Count() >= 23);
                    break;
                case 2:
                    val = (Bomb.GetModuleIDs().Contains("Spiderman2004") || Bomb.GetModuleIDs().Contains("papasPizzeria"));
                    break;
                case 3:
                    int digitProd = 1;
                    foreach (int digit in Bomb.GetSerialNumberNumbers())
                    {
                        digitProd *= (digit != 0 ? digit : 10);
                    }
                    val = digitProd < 256;
                    break;
                case 4:
                    val = Bomb.GetOnIndicators().Count() > Bomb.GetOffIndicators().Count();
                    break;
                case 5:
                    val = startDay < 16;
                    break;
                case 6:
                    val = Bomb.GetPorts().ToList().Contains("PS2");
                    break;
                case 7:
                    val = false;
                    foreach (string modName in Bomb.GetModuleNames())
                    {
                        if (modName.ToLower().Contains("j") || modName.ToLower().Contains("q") || modName.ToLower().Contains("x") || modName.ToLower().Contains("z"))
                        {
                            val = true;
                            break;
                        }
                    }
                    break;
                case 8:
                    val = Bomb.GetPorts().ToList().Contains("Serial");
                    break;
                case 9:
                    val = false;
                    foreach (string modName in Bomb.GetModuleNames())
                    {
                        if (modName.ToLower().Contains("forest") || modName.ToLower().Contains("green"))
                        {
                            val = true;
                            break;
                        }
                    }
                    break;
                case 10:
                    val = basePool.Count < 2;
                    break;
                case 11:
                    val = Bomb.GetSerialNumberNumbers().Last() % 2 == 1;
                    break;
                case 12:
                    val = false;
                    foreach (string modName in Bomb.GetModuleIDs())
                    {
                        if (cosmicMods.Contains(modName))
                        {
                            val = true;
                            break;
                        }
                    }
                    break;
                case 13:
                    val = Bomb.GetModuleIDs().Contains("snatchersMap");
                    break;
                case 14:
                    val = true;
                    foreach (int digit in Bomb.GetSerialNumberNumbers())
                    {
                        if (digit % 2 == 1)
                        {
                            val = false;
                            break;
                        }
                    }
                    break;
                case 15:
                    bool greaterThanFive = false;
                    foreach (int digit in Bomb.GetSerialNumberNumbers())
                    {
                        if (digit > 5)
                        {
                            greaterThanFive = true;
                        }
                    }
                    val = !(Bomb.GetSerialNumberLetters().ToList().Contains('V') & Bomb.GetSerialNumberLetters().ToList().Contains('C') & greaterThanFive);
                    break;
                case 16:
                    val = startMin > 20;
                    break;
                case 17:
                    val = Bomb.GetModuleNames().Count < 5;
                    break;
                case 18:
                    val = true;
                    foreach (char letter in Bomb.GetSerialNumberLetters())
                    {
                        if ("AEOUI".Contains(letter))
                        {
                            val = false;
                            break;
                        }
                    }
                    break;
                case 19:
                    val = Bomb.GetOffIndicators().Count() == 0;
                    break;
                case 20:
                    val = Bomb.GetBatteryHolderCount(Battery.AA) == 0;
                    break;
                case 21:
                    val = true;
                    break;
                default:
                    break;
            }
            if (val)
            {
                basePool.Add(i);
                basePoolLog += $"{itemNames[i]}, ";
            }
        }
        basePoolLog = basePoolLog.Substring(0, basePoolLog.Length - 2);
        Log($"Base pool is: {basePoolLog}");
    }
    
    void GenFinalPool()
    {
        int offset = 0;
        foreach (int digit in Bomb.GetSerialNumberNumbers().ToList())
        {
            offset += digit;
        }
        offset %= 10;
        Log($"The Offset for determining the Final Pool is {offset + 1}.");

        List<int> tempBasePool = basePool.ConvertAll(item => item);
        while (tempBasePool.Count > 0)
        {
            tempBasePool = ShiftList(tempBasePool, offset);
            finalPool.Add(tempBasePool[0]);
            tempBasePool.RemoveAt(0);
        }

        string finalPoolLog = "";
        foreach (int item in finalPool)
        {
            finalPoolLog += $"{itemNames[item]}, ";
        }
        finalPoolLog = finalPoolLog.Substring(0, finalPoolLog.Length - 2);
        Log($"The Final Pool is: {finalPoolLog}");
    }

    List<int> ShiftList(List<int> l, int shift)
    {
        List<int> newL = new List<int>();
        for (int i = 0; i < l.Count; i++)
        {
            newL.Add(l[(shift + i) % l.Count]);
        }
        return newL;
    }

    void Log(string arg)
    {
        Debug.Log($"[Rift Roulette #{ModuleId}] {arg}");
    }

    IEnumerator WaitForSolves()
    {
        while(Bomb.GetSolvedModuleIDs().Where(solve => solve.Equals("rhythmMaze")).Count() != Bomb.GetModuleIDs().Where(mod => mod.Equals("rhythmMaze")).Count())
        {
            yield return new WaitForSeconds(0.1f);
        }
        Activate();
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
