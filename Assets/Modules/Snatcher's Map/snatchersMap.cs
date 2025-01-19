﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class snatchersMap : MonoBehaviour
{
    [SerializeField] private KMBombInfo Bomb;
    [SerializeField] private KMAudio Audio;

    [SerializeField] private KMSelectable CycleButton;
    [SerializeField] private TextMesh InfoText;
    [SerializeField] private GameObject Map;
    [SerializeField] private List<KMSelectable> MapStamps;
    [SerializeField] private AudioClip FoolClip;

    string randomDW;
    string randomDWInfo;
    int infoWordCount;

    int lastTime;
    bool holdingCycleButton;
    bool overATick;
    int modWordIdx = -1;

    int TPCycleSpeed = 1;
    bool TPSubmissionMode;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    // The Death Wish Database
    Dictionary<string, List<string>> deathWishes = new Dictionary<string, List<string>>()
    {
        {"Beat the Heat", new List<string>() { "Turn off all the lava faucets", "It's much hotter than usual, cool yourself off!", "Don't get too hot!", "Don't cool down more than 2 times"} },
        {"Snatcher's Hit List", new List<string>() { "Kill 5 Mafia without getting punched", "They've got you all figured out!", "Kill 10 Alpine Pompous Crows without getting bullied", "Kill every type of enemy"} },
        {"So You're Back From Outer Space", new List<string>() { "Reach the Time Piece", "Mafia saw spaceship!!", "Smash all the UFOs", "Reach the end in 80 seconds"} },
        {"Rift Collapse: Mafia of Cooks", new List<string>() { "Escape the Mafia of Cooks Time Rift", "The Time Rift has become unstable!", "Escape with 30 seconds to spare", "Collect every Rift Pon"} },
        {"Snatcher Coins in Mafia Town", new List<string>() { "Find a Snatcher Coin in Mafia Town", "Find 2 Snatcher Coins", "Find all 3 Snatcher Coins"} },
        {"Collect-A-Thon", new List<string>() { "Simply collect 100 Pons", "Collection progress drains over time", "Don't pick up more than 200 Pons in total", "Don't break any crates or barrels"} },
        {"She Speedran From Outer Space", new List<string>() { "Clear 'She Came From Outer Space' in 1:50 or less", "Clear in 1:35 or less", "Clear in 1:20 or less"} },
        {"Vault Codes in the Wind", new List<string>() { "Open the Golden Vault", "You only have 30 seconds to collect each Vault Code", "Don't slow the flow of time", "Find time to defeat 10 Mafia"} },
        {"Mafia's Jumps", new List<string>() { "Complete 'She Came From Outer Space' in only 15 jumps", "Complete in 11 jumps", "Complete in 7 jumps"} },
        {"Encore! Encore!", new List<string>() { "Defeat the Mafia Boss", "He's all charged up!", "Don't miss your cue to attack", "With One-Hit Hero equipped"} },
        {"Security Breach", new List<string>() { "Infiltrate Dead Bird Studio", "Security has been upped, significantly", "Complete with less than 200,000 in fees", "Complete without getting caught"} },
        {"Rift Collapse: Dead Bird Studio", new List<string>() { "Escape the Dead Bird Studio Time Rift", "The Time Rift has become unstable!", "Escape with 30 seconds to spare", "Collect every Rift Pon"} },
        {"Snatcher Coins in Battle of the Birds", new List<string>() { "Find a Snatcher Coin in Battle of the Birds", "Find 2 Snatcher Coins", "Find all 3 Snatcher Coins"} },
        {"Zero Jumps", new List<string>() { "Collect any Time Piece without jumping", "You jump, you die!", "Collect 4 Time Pieces without jumping", "Clear Train Rush without jumping"} },
        {"10 Seconds to Self-Destruct", new List<string>() { "Escape Train Rush", "The Owl Express is way behind schedule", "Collect all the Conductor tokens", "No hat abilities, One-Hit Hero equipped"} },
        {"The Great Big Hootenanny", new List<string>() { "Complete The Big Parade", "It looks like you've gained quite a following", "Don't get hit by the Express Band", "Don't let any DJ Grooves tokens expire"} },
        {"Community Rift: Rhythm Jump Studio", new List<string>() { "Complete the Time Rift", "Contributed by Lane 'FlameLFH' Haskins", "Collect every Rift Pon", "Collect all the Storybook Pages"} },
        {"Killing Two Birds", new List<string>() { "Defeat both Directors", "This next project is a collaboration...", "Win with more than 100 seconds left on the clock", "Only attack one director until the very end"} },
        {"Speedrun Well", new List<string>() { "Clear 'Subcon Well' in 2:00 or less", "Clear in 1:40 or less", "Clear in 1:25 or less"} },
        {"Rift Collapse: Sleepy Subcon", new List<string>() { "Escape the Sleepy Subcon Time Rift", "The Time Rift has become unstable!", "Escape with 30 seconds to spare", "Collect every Rift Pon" } },
        {"Snatcher Coins in Subcon Forest", new List<string>() { "Find a Snatcher Coin in Subcon Forest", "Find 2 Snatcher Coins", "Find all 3 Snatcher Coins" } },
        {"Boss Rush", new List<string>() { "Down with the Mafia (and Everyone Else!)", "No healing between fights", "You have 3 chances", "Don't miss any chance to hit", "Win with One-Hit Hero equipped"} },
        {"Quality Time With Snatcher", new List<string>() { "Survive Snatcher for 2 minutes", "This time, no blue potions!", "Survive for 4 minutes", "Survive for 6 minutes"} },
        {"Breaching the Contract", new List<string>() { "Defeat the Snatcher", "I'm even more handsome than usual!", "Don't let your health drop to 1HP", "Don't attack me! Only throw blue potions!"} },
        {"Community Rift: Twilight Travels", new List<string>() { "Complete the Time Rift", "Contributed by Eric 'Jak' Ridge", "Collect every Rift Pon", "Collect all the Storybook Pages" } },
        {"Bird Sanctuary", new List<string>() { "Tiptoe through the Birdhouse", "Hurting birds is not allowed!", "With One-Hit Hero equipped", "Defeat the 6 hidden Mafia"} },
        {"Rift Collapse: Alpine Skyline", new List<string>() { "Escape the Alpine Skyline Time Rift", "The Time Rift has become unstable!", "Escape with 30 seconds to spare", "Collect every Rift Pon" } },
        {"Camera Tourist", new List<string>() { "Snap a picture of 8 different enemies", "Snap a single picture with 3 different enemies in it", "Snap a picture of every boss"} },
        {"Snatcher Coins in Alpine Skyline", new List<string>() { "Find a Snatcher Coin in Alpine Skyline", "Find 2 Snatcher Coins", "Find all 3 Snatcher Coins" } },
        {"Wound-Up Windmill", new List<string>() { "Reach the top of The Windmill", "The Windmill is operating at \"peak\" performance", "No hat abilities, One-Hit Hero equipped", "Reach the end in 4 minutes"} },
        {"The Illness Has Speedrun", new List<string>() { "Clear 'The Illness Has Spread' in 7:00 or less", "Clear in 6:30 or less", "Clear in 6:00 or less"} },
        {"Community Rift: The Mountain Rift", new List<string>() { "Complete the Time Rift", "Contributed by Devan 'Mr.Brawls' Miller", "Collect every Rift Pon", "Collect all the Storybook Pages" } },
        {"Rift Collapse: Deep Sea", new List<string>() { "Escape the Deep Sea Time Rift", "The Time Rift has become unstable!", "Escape with 30 seconds to spare", "Collect every Rift Pon" } },
        {"Cruisin' for a Bruisin'", new List<string>() { "Complete 40 tasks", "There's no end in sight", "Complete 40 tasks without missing one", "Complete 70 tasks"} },
        {"The Mustache Gauntlet", new List<string>() { "Defeat all the Bad Guys", "Don't get burned", "Don't use any projectile weapons"} },
        {"No More Bad Guys", new List<string>() { "Defeat Mustache Girl", "She stole a bunch of those weird Time Pieces", "Don't use any of your stupid hat abilities!", "Don't spend more than 3 minutes in the Hyper Zone"} },
        {"Seal the Deal", new List<string>() { "Defeat your strongest foes", "Good Luck!", "You have 3 chances", "Don't use any hat abilities", "Don't let your health drop to 1HP"} },
        {"Snatcher Coins in Nyakuza Metro", new List<string>() { "Find a Snatcher Coin in Nyakuza Metro", "Find 2 Snatcher Coins", "Find all 3 Snatcher Coins" } },
    };

    void Awake()
    {
        ModuleId = ModuleIdCounter++;

        CycleButton.OnInteract += delegate () { CycleButtonHold(); return false; };
        CycleButton.OnInteractEnded += delegate () { CycleButtonRelease(); };
        foreach (KMSelectable stamp in MapStamps)
        {
            stamp.OnInteract += delegate () { StampPressed(stamp); return false; };
        }
    }

    void CycleButtonHold()
    {
        overATick = false;
        holdingCycleButton = true;
    }

    void CycleButtonRelease()
    {
        holdingCycleButton = false;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, CycleButton.transform);
        if (!overATick)
        {
            CycleText();
        }
        else
        {
            Log("Cycle button was held over a timer tick, entering submission mode.");
            CycleButton.AddInteractionPunch();
            EnterSubmission();
        }
    }

    void CycleText()
    {
        modWordIdx++;
        modWordIdx %= infoWordCount;
        InfoText.text = randomDWInfo.Split(' ')[modWordIdx].ToUpperInvariant();
        SetFontSize();
        if (modWordIdx == 0)
        {
            InfoText.color = Color.gray;
        }
        else
        {
            InfoText.color = Color.white;
        }
    }

    void EnterSubmission()
    {
        CycleButton.gameObject.SetActive(false);
        InfoText.gameObject.SetActive(false);
        Map.SetActive(true);
        TPSubmissionMode = true;
    }

    void StampPressed(KMSelectable stamp)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, stamp.transform);
        stamp.AddInteractionPunch();

        Stamp stampScript = stamp.gameObject.GetComponent<Stamp>();
        Log($"The \"{stampScript.StampName}\" stamp was pressed, checking submission...");
        if (deathWishes[stampScript.StampName].Contains(randomDWInfo))
        {
            Log($"Selected Death Wish contains the chosen information, module solved!");
            GetComponent<KMBombModule>().HandlePass();
            Map.SetActive(false);
            ModuleSolved = true;
        }
        else
        {
            Log($"Selected Death Wish does not contain the chosen information, strike!");
            GetComponent<KMBombModule>().HandleStrike();
            StartModule();
        }
    }

    void Start()
    {
        if (Bomb.GetModuleIDs().Contains("rhythmMaze"))
        {
            Audio.PlaySoundAtTransform(FoolClip.name, transform);
        }
        StartModule();
    }

    void StartModule()
    {
        CycleButton.gameObject.SetActive(true);
        InfoText.gameObject.SetActive(true);
        Map.SetActive(false);
        TPSubmissionMode = false;

        int randomDWIdx = Rnd.Range(0, deathWishes.Keys.Count);
        randomDW = deathWishes.Keys.ElementAt<string>(randomDWIdx);
        int randomDWInfoIdx = Rnd.Range(0, deathWishes[randomDW].Count);

        randomDWInfo = deathWishes[randomDW].ElementAt<string>(randomDWInfoIdx);
        infoWordCount = randomDWInfo.Split(' ').ToList<string>().Count;
        modWordIdx = -1;
        CycleText();

        Log($"Selected Death Wish is \"{randomDW}\".");
        Log($"Selected information about that Death Wish is \"{randomDWInfo}\".");

        lastTime = (int)Bomb.GetTime();
    }

    void Update()
    {
        if (ModuleSolved)
        {
            return;
        }
        int time = (int)Bomb.GetTime();
        if (time != lastTime)
        {
            lastTime = time;
            if (holdingCycleButton)
            {
                overATick = true;
            }
        }
    }

    void Log(string arg)
    {
        Debug.Log($"[Snatcher's Map #{ModuleId}] {arg}");
    }

    void SetFontSize()
    {
        InfoText.fontSize = 500;
        float width = 0;
        do
        {
            width = 0;
            foreach (char c in InfoText.text)
            {
                CharacterInfo info;
                if (InfoText.font.GetCharacterInfo(c, out info, InfoText.fontSize, InfoText.fontStyle))
                {
                    width += info.advance;
                }
            }
            width *= InfoText.characterSize * 0.1f;
            if (width > 100)
            {
                InfoText.fontSize -= 5;
            }
        } while (width > 100);
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use <!{0} next> To cycle the text once. <!{0} cycle> To cycle through all the words. <!{0} cyclespeed #> To set the cycling speed to # timer ticks. <!{0} toggle> To enter submission mode. <!{0} submit Collect-A-Thon> To submit the Collect-A-Thon deathwish. (use the names directly from the manual)";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        var commandArgs = Command.Split(new[] { ' ' }, 2);
        if (ModuleSolved)
        {
            yield break;
        }
        if (commandArgs.Length < 1)
        {
            yield return "sendtochatmessage Invalid command!";
            yield break;
        }
        switch (commandArgs[0].ToLowerInvariant())
        {
            case ("next"):
                if (TPSubmissionMode)
                {
                    yield return "sendtochatmessage Already in submission mode!";
                    yield break;
                }
                yield return null;
                CycleButton.OnInteractEnded();
                break;
            case ("cycle"):
                if (TPSubmissionMode)
                {
                    yield return "sendtochatmessage Already in submission mode!";
                    yield break;
                }
                yield return null;
                for (int i = 0; i < infoWordCount; i++)
                {
                    int TPstartTime = (int)Bomb.GetTime();
                    int TPtime = (int)Bomb.GetTime();
                    while (true)
                    {
                        TPtime = (int)Bomb.GetTime();
                        yield return null;
                        if (Math.Max(TPstartTime, TPtime) - Math.Min(TPstartTime, TPtime) >= TPCycleSpeed + 1)
                        {
                            yield return null;
                            CycleButton.OnInteractEnded();
                            break;
                        }
                    }
                }
                yield return null;
                break;
            case ("cyclespeed"):
                if (TPSubmissionMode)
                {
                    yield return "sendtochatmessage Already in submission mode!";
                    yield break;
                }
                if (commandArgs.Length < 2)
                {
                    yield return "sendtochatmessage Cycle speed not specified!";
                    yield break;
                }
                else
                {
                    int tryParse;
                    if (int.TryParse(commandArgs[1], out tryParse))
                    {
                        if (tryParse > -1)
                        {
                            TPCycleSpeed = tryParse;
                            yield return $"Setting the cycle speed to {TPCycleSpeed} ticks.";
                        }
                        else
                        {
                            yield return "sendtochatmessage Invalid cycle speed!";
                            yield break;
                        }
                    }
                    else
                    {
                        yield return "sendtochatmessage Invalid cycle speed!";
                        yield break;
                    }
                }
                break;
            case ("toggle"):
                if (TPSubmissionMode)
                {
                    yield return "sendtochatmessage Already in submission mode!";
                    yield break;
                }
                yield return null;
                CycleButton.OnInteract();
                while (true)
                {
                    yield return null;
                    if (overATick)
                    {
                        yield return null;
                        CycleButton.OnInteractEnded();
                        break;
                    }
                }
                break;
            case ("submit"):
                if (!TPSubmissionMode)
                {
                    yield return "sendtochatmessage Not in submission mode!";
                    yield break;
                }
                if (commandArgs.Length < 2)
                {
                    yield return "sendtochatmessage Death Wish not specified!";
                    yield break;
                }
                else if (!deathWishes.Keys.ToList<string>().Contains(commandArgs[1]))
                {
                    yield return "sendtochatmessage Invalid Death Wish name!";
                    yield break;
                }
                else
                {
                    yield return null;
                    Map.transform.Find(commandArgs[1]).GetComponent<KMSelectable>().OnInteract();
                }
                break;
            default:
                yield return "sendtochatmessage Invalid command!";
                yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (!TPSubmissionMode)
        {
            yield return null;
            CycleButton.OnInteract();
            while (true)
            {
                yield return null;
                if (overATick)
                {
                    yield return null;
                    CycleButton.OnInteractEnded();
                    break;
                }
            }
        }
        yield return null;
        Map.transform.Find(randomDW).GetComponent<KMSelectable>().OnInteract();
    }
}
