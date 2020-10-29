﻿using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSequenceController : Singleton<EventSequenceController>
{
    // Game Start + Application Entry Points
    #region
    private void Start()
    {
        RunApplication();
    }
    private void RunApplication()
    {
        Debug.Log("CombatTestSceneController.Start() called...");

        // Establish settings

        // this prevents mobiles from sleeping due to inactivity
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Start application type
        if (GlobalSettings.Instance.gameMode == StartingSceneSetting.CombatSceneSingle)
        {
            StartCoroutine(RunCombatSceneSetup());
        }
        else if (GlobalSettings.Instance.gameMode == StartingSceneSetting.Standard)
        {
            StartCoroutine(RunStandardGameModeSetup());
        }
        else if (GlobalSettings.Instance.gameMode == StartingSceneSetting.CombatEndLootEvent)
        {
            StartCoroutine(RunCombatEndLootEventSetup());
        }
        else if (GlobalSettings.Instance.gameMode == StartingSceneSetting.RecruitCharacterEvent)
        {
            StartCoroutine(RunRecruitCharacterEventSetup());
        }
    }
    #endregion

    // Specific Game Set up style logic
    #region
    private IEnumerator RunStandardGameModeSetup()
    {
        // Set starting view state
        BlackScreenController.Instance.DoInstantFadeOut();
        MainMenuController.Instance.ShowFrontScreen();
        MainMenuController.Instance.frontScreenGuiCg.alpha = 0;
        MainMenuController.Instance.frontScreenBgParent.transform.DOScale(1.2f, 0f);
        yield return new WaitForSeconds(1);

        AudioManager.Instance.FadeInSound(Sound.Music_Main_Menu_Theme_1, 3f);
        BlackScreenController.Instance.FadeInScreen(2f);
        yield return new WaitForSeconds(2);

        MainMenuController.Instance.frontScreenBgParent.transform.DOKill();
        MainMenuController.Instance.frontScreenBgParent.transform.DOScale(1f, 1.5f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(1f);

        MainMenuController.Instance.frontScreenGuiCg.DOFade(1f, 1f).SetEase(Ease.OutCubic);
    }
    private IEnumerator RunCombatSceneSetup()
    {
        yield return null;

        // Play battle theme music
        AudioManager.Instance.PlaySound(Sound.Music_Battle_Theme_1);

        // Build character data
        CharacterDataController.Instance.BuildCharacterRosterFromCharacterTemplateList(GlobalSettings.Instance.testingCharacterTemplates);

        // Create player characters in scene
        CharacterEntityController.Instance.CreateAllPlayerCombatCharacters();

        // Spawn enemies
        EnemySpawner.Instance.SpawnEnemyWave("Basic", GlobalSettings.Instance.testingEnemyWave);

        // Start a new combat event
        ActivationManager.Instance.OnNewCombatEventStarted();
    }
    private IEnumerator RunCombatEndLootEventSetup()
    {
        yield return null;

        // Build character data
        CharacterDataController.Instance.BuildCharacterRosterFromCharacterTemplateList(GlobalSettings.Instance.testingCharacterTemplates);

        // Create player characters in scene
        CharacterEntityController.Instance.CreateAllPlayerCombatCharacters();
        
        StartCombatVictorySequence();
    }
    private IEnumerator RunRecruitCharacterEventSetup()
    {
        yield return null;

        // Build character data
        CharacterDataController.Instance.BuildCharacterRosterFromCharacterTemplateList(GlobalSettings.Instance.testingCharacterTemplates);

        // Set up mock data
        EncounterData mockData = new EncounterData();
        mockData.encounterType = EncounterType.RecruitCharacter;
        RecruitCharacterController.Instance.currentChoices = RecruitCharacterController.Instance.GetThreeValidRecruitableCharacters();
        JourneyManager.Instance.SetCheckPoint(SaveCheckPoint.RecruitCharacterStart);

        // Load event
        HandleLoadEncounter(mockData);

    }
    #endregion

    // Setup from save file
    #region
    public void HandleStartNewGameFromMainMenuEvent()
    {
        // Set up characters
        PersistencyManager.Instance.BuildNewSaveFileOnNewGameStarted();

        // Build and prepare all session data
        PersistencyManager.Instance.SetUpGameSessionDataFromSaveFile();

        BlackScreenController.Instance.FadeOutAndBackIn(1, 0f, 1f, () =>
         {
             // Hide Main Menu
             MainMenuController.Instance.HideNewGameScreen();
             MainMenuController.Instance.HideFrontScreen();

             // Start the first encounter set up sequence
             HandleLoadEncounter(JourneyManager.Instance.CurrentEncounter);
         });

        // Hide Main Menu
        //MainMenuController.Instance.HideNewGameScreen();
       // MainMenuController.Instance.HideFrontScreen();

        // CREATE NEW SAVE FILE

        // Set up characters
       // PersistencyManager.Instance.BuildNewSaveFileOnNewGameStarted();

        // Build and prepare all session data
        //PersistencyManager.Instance.SetUpGameSessionDataFromSaveFile();

        // Start the first encounter set up sequence
        //HandleLoadEncounter(JourneyManager.Instance.CurrentEncounter);
    }
    public void HandleLoadSavedGameFromMainMenuEvent()
    {
        StartCoroutine(HandleLoadSavedGameFromMainMenuEventCoroutine());
    }
    private IEnumerator HandleLoadSavedGameFromMainMenuEventCoroutine()
    {
        // Build and prepare all session data
        PersistencyManager.Instance.SetUpGameSessionDataFromSaveFile();

        BlackScreenController.Instance.FadeOutScreen(2f);
        yield return new WaitForSeconds(1f);

        // Hide Main Menu
        MainMenuController.Instance.HideFrontScreen();

        // Load the encounter the player saved at
        HandleLoadEncounter(JourneyManager.Instance.CurrentEncounter);
    }

    #endregion

    // Scene Transitions
    #region
    public void HandleQuitToMainMenuFromInGame()
    {
        StartCoroutine(HandleQuitToMainMenuFromInGameCoroutine());
    }
    private IEnumerator HandleQuitToMainMenuFromInGameCoroutine()
    {
        // Hide menus + GUI + misc annoying stuff
        MainMenuController.Instance.HideInGameMenuView();
        AudioManager.Instance.StopSound(Sound.Character_Footsteps);

        // Fade out battle music
        AudioManager.Instance.FadeOutSound(Sound.Music_Battle_Theme_1, 2f);

        // Do black screen fade out
        BlackScreenController.Instance.FadeOutScreen(2f);

        // Wait for the current visual event to finish playing
        VisualEvent handle = VisualEventManager.Instance.HandleEventQueueTearDown();

        // Wait till its safe to tearn down event queue and scene
        yield return new WaitForSeconds(2f);
        if (handle != null && handle.cData != null)
        {
            yield return new WaitUntil(() => handle.cData.CoroutineCompleted() == true);
        }

        // Destroy game scene
        HandleCombatSceneTearDown();

        // Hide Recruit character screen
        RecruitCharacterController.Instance.ResetAllViews();

        // Hide Loot screen elements
        LootController.Instance.CloseAndResetAllViews();

        // Fade in menu music
        AudioManager.Instance.FadeInSound(Sound.Music_Main_Menu_Theme_1, 1f);

        // Show menu screen
        MainMenuController.Instance.ShowFrontScreen();
        MainMenuController.Instance.RenderMenuButtons();

        // Do black screen fade in
        BlackScreenController.Instance.FadeInScreen(2f);
    }
    #endregion

    // Load Encounters Logic
    #region
    public void HandleLoadEncounter(EncounterData encounter)
    {
        if ((encounter.encounterType == EncounterType.BasicEnemy ||
            encounter.encounterType == EncounterType.EliteEnemy) &&
            JourneyManager.Instance.CheckPointType == SaveCheckPoint.CombatStart
            )
        {
            HandleLoadCombatEncounter(JourneyManager.Instance.CurrentEnemyWave);
        }

        else if ((encounter.encounterType == EncounterType.BasicEnemy ||
            encounter.encounterType == EncounterType.EliteEnemy) &&
            JourneyManager.Instance.CheckPointType == SaveCheckPoint.CombatEnd
            )
        {
            StartCombatVictorySequence();
        }

        else if (JourneyManager.Instance.CheckPointType == SaveCheckPoint.RecruitCharacterStart)
        {
            HandleLoadRecruitCharacterEncounter();
        }
    }
    public void HandleLoadNextEncounter()
    {
        StartCoroutine(HandleLoadNextEncounterCoroutine());
    }
    public IEnumerator HandleLoadNextEncounterCoroutine()
    {
        // Cache previous encounter data 
        EncounterData previousEncounter = JourneyManager.Instance.CurrentEncounter;
        EnemyWaveSO previousEnemyWave = JourneyManager.Instance.CurrentEnemyWave;

        // Increment world position
        JourneyManager.Instance.SetNextEncounterAsCurrentLocation();

        // Destroy all characters and activation windows if the 
        // previous encounter was a combat event
        if (previousEncounter.encounterType == EncounterType.BasicEnemy ||
            previousEncounter.encounterType == EncounterType.EliteEnemy)
        {
            // Mark wave as seen
            JourneyManager.Instance.AddEnemyWaveToAlreadyEncounteredList(previousEnemyWave);

            // Fade out visual event
            BlackScreenController.Instance.FadeOutScreen(3f);

            // Fade and close loot screen views
            LootController.Instance.FadeOutMainLootView(()=> LootController.Instance.HideMainLootView());

            // Move characters off screen
            CharacterEntityController.Instance.MoveCharactersToOffScreenRight(CharacterEntityController.Instance.AllDefenders, null);

            // Wait for visual events
            yield return new WaitForSeconds(4f);

            // Tear down combat scene
            HandleCombatSceneTearDown();
        }

        // Tear down recruit character screen
        else if(previousEncounter.encounterType == EncounterType.RecruitCharacter)
        {
            // Fade out visual event
            BlackScreenController.Instance.FadeOutScreen(1f);
            yield return new WaitForSeconds(1f);

            // Teardown recruit event views
            RecruitCharacterController.Instance.ResetAllViews();
        }

        // If next event is a combat, get + set enemy wave before saving to disk
        if(JourneyManager.Instance.CurrentEncounter.encounterType == EncounterType.BasicEnemy ||
            JourneyManager.Instance.CurrentEncounter.encounterType == EncounterType.EliteEnemy)
        {
            // Calculate and cache the next enemy wave group
            JourneyManager.Instance.SetCurrentEnemyWaveData 
                (JourneyManager.Instance.GetRandomEnemyWaveFromEncounterData(JourneyManager.Instance.CurrentEncounter));

            // Set check point
            JourneyManager.Instance.SetCheckPoint(SaveCheckPoint.CombatStart);

            // Auto save
            PersistencyManager.Instance.AutoUpdateSaveFile();

            // Start Load combat
            HandleLoadCombatEncounter(JourneyManager.Instance.CurrentEnemyWave);
        }

        // Recruit character event
        else if(JourneyManager.Instance.CurrentEncounter.encounterType == EncounterType.RecruitCharacter)
        {
            // Generate 3 random characters, if we are not loading from save
            if (RecruitCharacterController.Instance.currentChoices.Count == 0)
            {
                RecruitCharacterController.Instance.currentChoices = RecruitCharacterController.Instance.GetThreeValidRecruitableCharacters();
            }

            // Set check point
            JourneyManager.Instance.SetCheckPoint(SaveCheckPoint.RecruitCharacterStart);

            // Auto save
            PersistencyManager.Instance.AutoUpdateSaveFile();

            HandleLoadRecruitCharacterEncounter();
        }
    }
    private void HandleLoadCombatEncounter(EnemyWaveSO enemyWave)
    {
        // Fade In
        BlackScreenController.Instance.FadeInScreen(1f);

        // Camera Zoom out effect
        CameraManager.Instance.DoCameraZoomOut(4, 5, 1);

        // Play battle theme music
        AudioManager.Instance.FadeOutSound(Sound.Music_Main_Menu_Theme_1, 1f);
        AudioManager.Instance.FadeInSound(Sound.Music_Battle_Theme_1, 1f);

        // Create player characters in scene
        CharacterEntityController.Instance.CreateAllPlayerCombatCharacters();

        // Spawn enemies
        EnemySpawner.Instance.SpawnEnemyWave(enemyWave.combatDifficulty.ToString(), enemyWave);

        // move characters offscreen
        CharacterEntityController.Instance.MoveAllCharactersToOffScreenPosition();

        // make characters move towards start nodes
        CoroutineData cData = new CoroutineData();
        VisualEventManager.Instance.CreateVisualEvent(() => CharacterEntityController.Instance.MoveAllCharactersToStartingNodes(cData));       

        // Start a new combat event
        ActivationManager.Instance.OnNewCombatEventStarted();
    }
    private void HandleLoadRecruitCharacterEncounter()
    {
        // Fade in
        BlackScreenController.Instance.FadeInScreen(2f);

        // Build + Show views
        RecruitCharacterController.Instance.ResetAllViews();
        RecruitCharacterController.Instance.ShowRecruitCharacterScreen();
        RecruitCharacterController.Instance.BuildRecruitCharacterWindows();
    }
    #endregion

    // Handle Post Combat Stuff
    #region
    public void StartCombatVictorySequence()
    {
        StartCoroutine(StartCombatVictorySequenceCoroutine());
    }
    private IEnumerator StartCombatVictorySequenceCoroutine()
    {
        // wait until v queue count = 0
        yield return new WaitUntil(()=> VisualEventManager.Instance.EventQueue.Count == 0);

        AudioManager.Instance.FadeOutSound(Sound.Music_Battle_Theme_1, 1f);

        // fade out combat music
        // play victory music sfx
        // create victory pop up + firework particles + xp gain stuff (in future)

        // Disable any player characteer gui's if they're still active
        foreach (CharacterEntityModel model in CharacterEntityController.Instance.AllDefenders)
        {
            CharacterEntityController.Instance.FadeOutCharacterWorldCanvas(model.characterEntityView, null);
            CharacterEntityController.Instance.FadeOutCharacterUICanvas(model.characterEntityView, null);            
            if (model.characterEntityView.uiCanvasParent.activeSelf == true)
            {
                CharacterEntityController.Instance.FadeOutCharacterUICanvas(model.characterEntityView, null);
            }
        }

        // Destroy Activation windows
        ActivationManager.Instance.DestroyAllActivationWindows();

        // Hide end turn button
        UIManager.Instance.DisableEndTurnButtonView();

        // Generate loot result + Auto save
        if(JourneyManager.Instance.CheckPointType != SaveCheckPoint.CombatEnd)
        {
            LootController.Instance.SetAndCacheNewLootResult();

            // Save and set cechkpoint + cache loot result
            JourneyManager.Instance.SetCheckPoint(SaveCheckPoint.CombatEnd);
            PersistencyManager.Instance.AutoUpdateSaveFile();
        }       

        // Build loot screen views
        LootController.Instance.BuildLootScreenElementsFromLootResultData();        

        // fade in loot window
        LootController.Instance.FadeInMainLootView();
        LootController.Instance.ShowFrontPageView();
    }
    #endregion

    // Handle Teardown Encounters
    #region
    public void HandleCombatSceneTearDown()
    {
        CharacterEntityController.Instance.DestroyCharacterViewModelsAndGameObjects(CharacterEntityController.Instance.AllCharacters);
        CharacterEntityController.Instance.ClearAllCharacterPersistencies();
        ActivationManager.Instance.DestroyAllActivationWindows();
        LevelManager.Instance.ClearAndResetAllNodes();
        UIManager.Instance.DisableEndTurnButtonView();
    }
    #endregion
}
