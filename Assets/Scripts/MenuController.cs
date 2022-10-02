using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

public class MenuController : MonoBehaviour
{
    public GameObject startButton;
    public GameObject exitButton;
    public ParticleSystem particleSystem;

    public CanvasGroup startExitGroup;
    public CanvasGroup tutorialOptionGroup;

    public TextMeshProUGUI musicVolume;
    public TextMeshProUGUI sfxVolume;

    public TextMeshProUGUI levelTitle;
    public TextMeshProUGUI clearLabel;

    private bool starting = false;

    void Update()
    {
        if (GameManager.Instance.gameRunning)
        {
            startButton.SetActive(false);
        }

        musicVolume.text = (AudioController.Instance.musicVolumeOffset + 5).ToString();
        sfxVolume.text = (AudioController.Instance.sfxVolumeOffset + 5).ToString();
    }

    public void UpdateLevelName(string name)
    {
        levelTitle.text = name;
    }

    public void UpdateClears(int clears)
    {
        if (clears == 0)
        {
            clearLabel.enabled = false;
            return;
        }

        clearLabel.text = $"Clears: {clears}";
        clearLabel.enabled = true;
    }

    public void StartPressed()
    {
        StartCoroutine(TransitionTutorialQuestion());
    }

    public void StartTutorial()
    {
        if (starting) return;
        starting = true;
        GameManager.Instance.StartTutorial();
    }

    public void StartGame()
    {
        if (starting) return;
        starting = true;
        GameManager.Instance.StartGameNoTutorial();
    }

    public void ExitPressed()
    {
        Application.Quit();
    }

    public void MusicVolDown()
    {
        AudioController.Instance.musicVolumeOffset -= 1;
        AudioController.Instance.Play("TestMusicSound");
    }

    public void MusicVolUp()
    {
        AudioController.Instance.musicVolumeOffset += 1;
        AudioController.Instance.Play("TestMusicSound");
    }

    public void SfxVolDown()
    {
        AudioController.Instance.sfxVolumeOffset -= 1;
        AudioController.Instance.Play("TestSfxSound");
    }

    public void SfxVolUp()
    {
        AudioController.Instance.sfxVolumeOffset += 1;
        AudioController.Instance.Play("TestSfxSound");
    }

    private IEnumerator TransitionTutorialQuestion()
    {
        yield return startExitGroup.DOFade(0f, 0.3f)
            .SetUpdate(true)
            .WaitForCompletion();
        startExitGroup.interactable = false;
        startExitGroup.blocksRaycasts = false;
        yield return tutorialOptionGroup.DOFade(1f, 0.3f)
            .SetUpdate(true)
            .WaitForCompletion();
        tutorialOptionGroup.interactable = true;
        tutorialOptionGroup.blocksRaycasts = true;
    }

    public void SetupForPause()
    {
        startExitGroup.interactable = true;
        startExitGroup.blocksRaycasts = true;
        tutorialOptionGroup.interactable = false;
        tutorialOptionGroup.blocksRaycasts = false;
        tutorialOptionGroup.alpha = 0f;
        startExitGroup.alpha = 1f;
    }

}
