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
    [SerializeField] private List<Material> ItemMaterials;
    [SerializeField] private GameObject FinalSlot;
    [SerializeField] private AudioClip StartupJingle;

    List<string> itemNames = new List<string>() { "Cardboard", "Anarchy", "Pizza Time", "Virtual Kid", "Citrus", "Old Film", "Wireframe", "Obnoxious", "Blue Comet", "The Forest Critter", "City Girl", "Punk Set", "Milky Way", "Shadow Puppet", "Transcendent", "Too Hot to Handle", "Ice Hat", "Sprint Hat", "Brewing Hat", "Dweller Mask", "Time Stop Hat", "Kid's Hat" };
    List<string> cosmicMods = new List<string>() { "Astrological", "spwizAstrology", "cruelStars", "earth", "exoplanets", "jupiter", "mars", "matchRefereeing", "mercury", "neptune", "nomai", "planets", "planetX", "pluto", "saturn", "xelSpace", "stars", "syzygyModule", "uranus", "venus"};
    int startDay;
    int startMin;

    List<int> basePool = new List<int>() { };
    List<int> finalPool = new List<int>() { };
    int goalItem;
    int goalItemIdx;

    int X, A;
    int rolledCount;
    int startingItem, lastRolled, lastIndex;
    bool XandA, success;

    bool TPSubmissionMode;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    void Awake()
    {
        ModuleId = ModuleIdCounter++;
        foreach (GameObject slot in Slots) {
            slot.GetComponent<KMSelectable>().OnInteract += delegate () { InitSlotRoll(slot); return false; };
            }
        FinalSlot.GetComponent<KMSelectable>().OnInteract += delegate () { FinalSlotRoll(); return false; };
    }

    void InitSlotRoll(GameObject slot)
    {
        if (!success)
        {
            ModuleSolved = true;
            GetComponent<KMBombModule>().HandlePass();
            SlotsParent.SetActive(false);
        }
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, slot.transform);
        slot.GetComponent<KMSelectable>().AddInteractionPunch();

        int slotIndex = Slots.IndexOf(slot);
        if (slotIndex == rolledCount)
        {
            rolledCount++;
            for (int i = 0; i < Slots.Count; i++)
            {
                Slots[i].GetComponent<MeshRenderer>().material.color = (i == rolledCount || rolledCount == 4 ? Color.white : Color.gray);
                Slots[i].transform.Find("Item Icon").gameObject.GetComponent<MeshRenderer>().material = null;
            }
            slot.transform.Find("Item Icon").gameObject.GetComponent<MeshRenderer>().material = ItemMaterials[lastRolled];
            Log($"Roll number {Slots.IndexOf(slot) + 1}: {itemNames[lastRolled]}.");
            RollThroughPool();
        }
        else if (rolledCount == 4)
        {
            SlotsParent.SetActive(false);
            FinalSlot.SetActive(true);
            TPSubmissionMode = true;
            Log($"Entered Submission Mode! Current Roll: {itemNames[lastRolled]}.");
        }
    }

    void FinalSlotRoll()
    {
        if (ModuleSolved)
        {
            return;
        }
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, FinalSlot.transform);

        if ((int)Bomb.GetTime() % 2 == 0)
        {
            FinalSlot.GetComponent<KMSelectable>().AddInteractionPunch();
            Log($"Submitted {itemNames[lastRolled]}. That was {(lastRolled == goalItem ? "correct. Module Solved!" : "incorrect. Strike!")}");
            if (lastRolled == goalItem)
            {
                ModuleSolved = true;
                GetComponent<KMBombModule>().HandlePass();    
                
            }
            else
            {
                GetComponent<KMBombModule>().HandleStrike();
            }
            FinalSlot.transform.Find("Item Icon").gameObject.GetComponent<MeshRenderer>().material = ItemMaterials[lastRolled];
        }
        else
        {
            RollThroughPool();
            FinalSlot.transform.Find("Item Icon").gameObject.GetComponent<MeshRenderer>().material = null;
            Log($"Rolling! Current Roll: {itemNames[lastRolled]}.");
        }
    }

    void RollThroughPool()
    {
        lastIndex += X;
        if (XandA)
        {
            lastIndex += A;
            XandA = false;
        }
        else
        {
            XandA = true;
        }
        lastIndex %= finalPool.Count;
        lastRolled = finalPool[lastIndex];
    }

    void Start()
    {
        startDay = Convert.ToInt32(DateTime.Now.ToString("dd"));
        startMin = (int)Bomb.GetTime() / 60;
        FinalSlot.SetActive(false);
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
        Audio.PlaySoundAtTransform(StartupJingle.name, transform);
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

        startingItem = finalPool[Rnd.Range(0, finalPool.Count - 1)];
        lastRolled = startingItem;
        lastIndex = finalPool.IndexOf(lastRolled);
        Log($"The Starting Item (1st Roll) is {itemNames[startingItem]}");

        XandA = Rnd.Range(0, 1) == 1;
        GenXandA();
    }

    void FailSafe()
    {
        foreach (GameObject slot in Slots)
        {
            slot.GetComponent<MeshRenderer>().material.color = Rnd.ColorHSV();
        }
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

    void GenXandA()
    {
        for (int i = 0; i < 1000000; i++)
        {
            X = Rnd.Range(1, finalPool.Count);
            A = Rnd.Range(1, finalPool.Count);
            bool CheckXandA = XandA;
            int CheckPos = finalPool.IndexOf(startingItem);
            List<int> CheckRolled = new List<int>() {  };

            for (int j = 0; j < 50; j++)
            {
                CheckPos += X;
                if (CheckXandA)
                {
                    CheckPos += A;
                    CheckXandA = false;
                }
                else
                {
                    CheckXandA = true;
                }
                CheckPos %= finalPool.Count;
                CheckRolled.Add(CheckPos);
            }
            bool val = true;
            for (int j = 0; j < finalPool.Count; j++)
            {
                if (!CheckRolled.Contains(j))
                {
                    val = false;
                    break;
                }
            }
            if (val)
            {
                success = true;
                break;
            }
        }
        if (success)
        {
            Log($"Generated variables are: X = {X}, A = {A}. The second roll will be gotten by going forward {(XandA ? "X + A" : "X")} times.");
        }
        else
        {
            Log("The module has failed to generate X and A values. Any button press will solve the module.");
            FailSafe();
        }
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
    private readonly string TwitchHelpMessage = @"Use <!{0} roll> in initial phase to roll the next slot / enter submission mode. In submission mode, use <!{0} next #> to roll the slot # times (or 1 if # not specified) and use <!{0} submit> to submit the current item.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        string[] Args = Command.ToLowerInvariant().Split(new[] { ' ' }, 2);
        switch (Args[0])
        {
            case "roll":
                if (TPSubmissionMode)
                {
                    yield return "sendtochatmessage Module already in submission mode!";
                }
                else
                {
                    yield return null;
                    Slots[rolledCount % 4].GetComponent<KMSelectable>().OnInteract();
                }
                break;
            case "next":
                if (!TPSubmissionMode)
                {
                    yield return "sendtochatmessage Module not in submission mode!";
                }
                else
                {
                    int times = 1;
                    if (Args.Length == 2)
                    {
                        int tryParse;
                        if (int.TryParse(Args[1], out tryParse))
                        {
                            times = tryParse;
                        }
                        else
                        {
                            yield return "sendtochatmessage Invalid number of times!";
                        }
                    }
                    yield return null;
                    for (int i = 0; i < times; i++)
                    {
                        int currTime = (int)Bomb.GetTime();
                        while (currTime % 2 == 0)
                        {
                            yield return null;
                            currTime = (int)Bomb.GetTime();
                        }
                        FinalSlot.GetComponent<KMSelectable>().OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                break;
            case "submit":
                if (!TPSubmissionMode)
                {
                    yield return "sendtochatmessage Module not in submission mode!";
                }
                else
                {
                    yield return null;
                    int currTime = (int)Bomb.GetTime();
                    while (currTime % 2 == 1)
                    {
                        yield return null;
                        currTime = (int)Bomb.GetTime();
                    }
                    FinalSlot.GetComponent<KMSelectable>().OnInteract();
                }
                break;
            default:
                break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        if (!TPSubmissionMode)
        {
            for (int i = rolledCount; i < 4; i++)
            {
                Slots[i].GetComponent<KMSelectable>().OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            Slots[0].GetComponent<KMSelectable>().OnInteract();
        }
        while (lastRolled != goalItem)
        {
            int currTime = (int)Bomb.GetTime();
            while (currTime % 2 == 0)
            {
                yield return null;
                currTime = (int)Bomb.GetTime();
            }
            FinalSlot.GetComponent<KMSelectable>().OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        int currFinalTime = (int)Bomb.GetTime();
        while (currFinalTime % 2 == 1)
        {
            yield return null;
            currFinalTime = (int)Bomb.GetTime();
        }
        FinalSlot.GetComponent<KMSelectable>().OnInteract();
    }
}
