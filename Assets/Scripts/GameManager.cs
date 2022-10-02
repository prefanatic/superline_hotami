using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Animator transition;
    public TextMeshProUGUI timeLabel;
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public Camera camera;
    public MenuController menuController;

    public VolumeProfile volume;

    public ParticleSystem menuParticles;
    public CanvasGroup timerCanvasGroup;
    public CanvasGroup fadeCanvasGroup;
    public CanvasGroup menuCanvasGroup;
    public CanvasGroup deathCanvasGroup;

    public TextMeshProUGUI causeOfDeath;

    public int levelsFinished = 0;
    public float difficultyFactor = 1f;

    public bool gameRunning = false;
    private float levelStartTime = 0;
    private bool gameEnding = false;
    [SerializeField] private bool timeScaleLock = false;
    private Coroutine timeScaleLockRoutine;

    public int loadLevel = 1;
    [SerializeField] private Scene activeScene;
    public float timeSinceLoad;

    public bool mainMenu = true;
    public bool menuOpen = true;
    public bool deadMenuOpen = false;
    public bool tutorial = false;

    private string lastCauseOfDeath = "";

    public Level[] levels;
    public Level activeLevel;

    public System.Action sceneChange;

    public Weapon[] weapons;
    public Weapon activeWeapon;

    public CanvasGroup weaponPickerGroup;
    public TextMeshProUGUI weaponPicker1;
    public TextMeshProUGUI weaponPicker2;

    private Weapon weaponChoiceOne, weaponChoiceTwo;

    public int weaponSwitchPerLevel = 2;
    private int weaponLevelCounter = 0;

    public GameObject levelClearLinePrefab;
    public VerticalLayoutGroup levelClearGroup;

    const float CAMERA_PLAY_Y = 12.29f;
    const float CAMERA_PAUSE_Y = 30.29f;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        SceneManager.sceneLoaded += delegate (Scene scene, LoadSceneMode mode)
        {
            // Don't care about some scenes.
            if (scene.name == "MainScene") return;

            print($"Loaded: {scene.name}");
            activeScene = scene;
            SceneManager.SetActiveScene(scene);
            SpawnPlayer();
            SpawnEnemies();

            // Offset for main scene at zero
            activeLevel = levels[activeScene.buildIndex - 1];
            menuController.UpdateLevelName(activeLevel.name);
            menuController.UpdateClears(activeLevel.clears);

            // Update the weapon switch increment if it's not tutorial based.
            if (!activeLevel.tutorial)
            {
                weaponLevelCounter += 1;
            }

            sceneChange.Invoke();

            gameRunning = true;
            gameEnding = false;
            timeScaleLock = false;
            levelStartTime = Time.time;
        };

        // Start with the pistol
        activeWeapon = weapons[0];
    }

    public void StartTutorial()
    {
        mainMenu = false;
        gameRunning = false;
        tutorial = true;
        CloseMenu();
        StartCoroutine(LoadLevel(1));
        AudioController.Instance.Play("GameLoop");
    }

    public void StartGameNoTutorial()
    {
        mainMenu = false;
        tutorial = false;
        gameRunning = false;
        CloseMenu();
        StartCoroutine(LoadLevel(3));
        AudioController.Instance.Play("GameLoop");
    }

    public void StartLoadLevel(int index)
    {
        StartCoroutine(LoadLevel(index));
    }

    public void OnPickWeapon(int index)
    {
        switch (index)
        {
            case 0:
                activeWeapon = weaponChoiceOne;
                break;
            case 1:
                activeWeapon = weaponChoiceTwo;
                break;
            default:
                break;
        }

        HideWeaponPicker();
    }

    private void ShowWeaponPicker()
    {
        // Pick two weapons randomly to populate
        int indexOne, indexTwo;

        indexOne = Random.Range(0, weapons.Length);
        do
        {
            indexTwo = Random.Range(0, weapons.Length);
        } while (indexTwo == indexOne); // Prevent picking the same weapon.

        weaponChoiceOne = weapons[indexOne];
        weaponChoiceTwo = weapons[indexTwo];

        weaponPicker1.text = weaponChoiceOne.description.ToString();
        weaponPicker2.text = weaponChoiceTwo.description.ToString();

        weaponPickerGroup.DOFade(1f, 0.3f)
            .SetUpdate(true);
        weaponPickerGroup.interactable = true;
        weaponPickerGroup.blocksRaycasts = true;
    }

    private void HideWeaponPicker()
    {
        weaponPickerGroup.DOFade(0f, 0.3f)
           .SetUpdate(true);
        weaponPickerGroup.interactable = false;
        weaponPickerGroup.blocksRaycasts = false;
    }

    private void PopulateDeathMenu()
    {
        // Clear it if it already exists
        for (int i = 0; i < levelClearGroup.transform.childCount; i++)
        {
            Destroy(levelClearGroup.transform.GetChild(i).gameObject);
        }

        // Pick up all the levels we've cleared
        var cleared = new List<Level>();
        foreach (var level in levels)
        {
            if (level.clears >= 1)
            {
                cleared.Add(level);
            }
        }

        foreach (var clear in cleared)
        {
            var line = Instantiate(levelClearLinePrefab, levelClearGroup.transform);
            var levelName = line.transform.GetChild(0);
            var personalBest = line.transform.GetChild(1);
            var weaponUsed = line.transform.GetChild(2);

            levelName.GetComponent<TextMeshProUGUI>().text = clear.name;
            personalBest.GetComponent<TextMeshProUGUI>().text = clear.bestTime.ToString("F2") + "s";
            weaponUsed.GetComponent<TextMeshProUGUI>().text = clear.bestTimeWeapon;
        }
    }

    void CloseMenu()
    {
        menuOpen = false;
        timerCanvasGroup.DOFade(1f, 0.3f)
            .SetUpdate(true)
            .SetEase(Ease.InQuart);
        menuCanvasGroup.DOFade(0f, 0.3f)
            .SetUpdate(true)
            .SetEase(Ease.OutQuart);
        camera.transform.DOLocalMoveY(CAMERA_PLAY_Y, 0.3f)
            .SetUpdate(true)
            .SetEase(Ease.OutQuart)
            .OnComplete(delegate ()
            { menuParticles.Pause(); });
        menuCanvasGroup.blocksRaycasts = false;
        menuCanvasGroup.interactable = false;

        if (deadMenuOpen)
        {
            deathCanvasGroup.DOFade(1f, 0.3f)
               .SetUpdate(true)
               .SetEase(Ease.OutQuart);
        }
    }

    void OpenMenu()
    {
        menuController.SetupForPause();
        menuOpen = true;
        timerCanvasGroup.DOFade(0f, 0.3f)
            .SetUpdate(true)
            .SetEase(Ease.OutQuart);
        menuCanvasGroup.DOFade(1f, 0.3f)
            .SetUpdate(true)
            .SetEase(Ease.InQuart);
        camera.transform.DOLocalMoveY(CAMERA_PAUSE_Y, 0.3f)
            .SetUpdate(true)
            .SetEase(Ease.InQuart)
            .OnComplete(delegate ()
            { menuParticles.Play(); });
        menuCanvasGroup.blocksRaycasts = true;
        menuCanvasGroup.interactable = true;

        if (deadMenuOpen)
        {
            deathCanvasGroup.DOFade(0f, 0.3f)
               .SetUpdate(true)
               .SetEase(Ease.OutQuart);
        }

        SetDepthOfField(5.8f);
    }

    public void Retry()
    {
        StartCoroutine(Restart());
    }

    void Update()
    {
        if (mainMenu) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }

        if (menuOpen)
        {
            Time.timeScale = 0f;
            return;
        }

        if (!gameRunning) return;
        timeSinceLoad = Time.time - levelStartTime;

        var timeRemaining = Mathf.Clamp(10f - timeSinceLoad, 0f, 10f);
        timeLabel.text = timeRemaining.ToString("F2");

        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        var timeScale = 1f;
        var lerpScale = 0.05f;
        if (horizontal == 0 && vertical == 0 && !gameEnding && !timeScaleLock)
        {
            timeScale = 0.05f;
            lerpScale = 0.5f;
        }
        Time.timeScale = Mathf.Lerp(Time.timeScale, timeScale, lerpScale);

        if (!tutorial)
        {
            NormalGameLoop();
        }
    }

    private void SetDepthOfField(float focusDistance)
    {
        DepthOfField depthOfField;
        if (volume.TryGet<DepthOfField>(out depthOfField))
        {
            print(focusDistance);
            //depthOfField.focusDistance = new MinFloatParameter(focusDistance, 0f, true);
        }
    }

    private void NormalGameLoop()
    {
        var player = GameObject.FindWithTag("Player");

        if (player)
        {
            var focusDistance = camera.transform.position.y - player.transform.position.y;
            //SetDepthOfField(focusDistance);
        }

        // 10 seconds kill check
        bool playerDead = false;
        if (timeSinceLoad > 10)
        {
            if (player)
            {
                var health = player.GetComponent<Health>();
                health.RemoveHealth(1000);

                lastCauseOfDeath = "Outta time";
            }
            playerDead = true;
        }

        // If we died some other way
        if (!player)
        {
            lastCauseOfDeath = "Bested by bullets";
            playerDead = true;
        }

        if (playerDead && gameRunning && !gameEnding)
        {
            StartCoroutine(Lose());
        }

        // Level win check
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0 && !playerDead && !gameEnding)
        {
            StartCoroutine(Win());
        }
    }

    public void DeferTimeScale()
    {
        if (timeScaleLockRoutine != null)
        {
            StopCoroutine(timeScaleLockRoutine);
        }
        timeScaleLockRoutine = StartCoroutine(LockTimeScale());
    }

    private int GetRandomLevel()
    {
        // Shuffles and returns the least played level.
        var count = int.MaxValue;
        var leastPlayed = levels[1];
        foreach (var level in levels)
        {
            if (level.tutorial) continue;
            if (level.clears < count)
            {
                leastPlayed = level;
                count = level.clears;
            }
        }

        // One more loop to tease out levels of the same least played count.
        var ofSameCount = new List<Level>();
        foreach (var level in levels)
        {
            if (level.tutorial) continue;
            if (level.clears == count)
            {
                ofSameCount.Add(level);
            }
        }
        if (ofSameCount.Count > 1)
        {
            // Randomly return one of these instead.
            var index = Random.Range(0, ofSameCount.Count);
            return ofSameCount[index].buildIndex;
        }

        return leastPlayed.buildIndex;
    }

    private IEnumerator LockTimeScale()
    {
        timeScaleLock = true;
        yield return new WaitForSeconds(0.2f);
        timeScaleLock = false;
    }

    private IEnumerator LoadLevel(int index)
    {
        print($"Loading scene {index}");
        fadeCanvasGroup.blocksRaycasts = true;
        fadeCanvasGroup.interactable = true;
        yield return fadeCanvasGroup.DOFade(1f, 0.3f)
            .SetUpdate(true)
            .WaitForCompletion();
        if (activeScene.IsValid())
        {
            print($"Unloading {activeScene.name}");
            yield return SceneManager.UnloadSceneAsync(activeScene);
        }
        deathCanvasGroup.alpha = 0f;
        deathCanvasGroup.interactable = false;
        deathCanvasGroup.blocksRaycasts = false;
        deadMenuOpen = false;

        // Should we prompt a weapon switch?
        if (weaponLevelCounter % weaponSwitchPerLevel == 1)
        {
            ShowWeaponPicker();
            // This is about as hacky as can be.
            do
            {
                yield return 0;
            } while (weaponPickerGroup.alpha != 0f);
        }
        yield return new WaitForSecondsRealtime(.10f);
        yield return SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
        yield return fadeCanvasGroup.DOFade(0f, 0.3f)
            .SetUpdate(true)
            .WaitForCompletion();
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.interactable = false;
    }

    private IEnumerator Win()
    {
        print("Win");
        levelsFinished += 1;
        difficultyFactor += 0.05f;
        gameRunning = false;
        timeScaleLock = true;

        var bestTime = activeLevel.bestTime;
        if (bestTime < timeSinceLoad)
        {
            activeLevel.bestTime = timeSinceLoad;
            activeLevel.bestTimeWeapon = activeWeapon.name;
        }
        activeLevel.clears += 1;

        yield return new WaitForSecondsRealtime(1f);
        yield return LoadLevel(GetRandomLevel());
    }

    private IEnumerator Lose()
    {
        print("Lose");
        levelsFinished = 0;

        gameRunning = false;
        timeScaleLock = true;
        yield return new WaitForSecondsRealtime(1f);

        causeOfDeath.text = lastCauseOfDeath;
        PopulateDeathMenu();
        yield return deathCanvasGroup.DOFade(1f, 0.3f)
           .SetUpdate(true)
           .WaitForCompletion();
        deadMenuOpen = true;
        deathCanvasGroup.interactable = true;
        deathCanvasGroup.blocksRaycasts = true;
    }

    private IEnumerator Restart()
    {
        print("Restart");
        yield return LoadLevel(activeLevel.buildIndex);
    }

    private void SpawnPlayer()
    {
        var point = GameObject.FindWithTag("PlayerSpawnPoint");
        var player = Instantiate(playerPrefab, point.transform.position, Quaternion.identity);
        var controller = player.GetComponent<PlayerController>();
        var weaponPoint = controller.weaponPoint;

        var weapon = Instantiate(activeWeapon.prefab, weaponPoint.transform);
        var abstractWeapon = weapon.GetComponent<AbstractWeapon>();
        abstractWeapon.isPlayer = true;
        controller.weapon = abstractWeapon;
    }

    private void SpawnEnemies()
    {
        // Iterate through spawn points, use them to create enemy instances.
        var points = GameObject.FindGameObjectsWithTag("EnemySpawnPoint");
        foreach (var point in points)
        {
            var enemy = Instantiate(enemyPrefab, point.transform.position, Quaternion.identity);
        }
    }
}

[System.Serializable]
public class Level
{
    public string name;
    public int buildIndex;
    public bool tutorial;

    [HideInInspector] public int clears;
    [HideInInspector] public float bestTime;
    [HideInInspector] public string bestTimeWeapon;
}

[System.Serializable]
public class Weapon
{
    public string name;
    [TextArea(5, 10)]
    public string description;
    public GameObject prefab;
}