using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour
{
    public GameObject timeMovesWhenYouDo;
    public GameObject tenSecondsToLive;
    public GameObject afterThatYouDie;
    public GameObject toSurviveKill;
    public GameObject illGiveYouMoreTime;
    public GameObject good;
    public GameObject tookYouLongEnough;
    public int level;

    // Start is called before the first frame update
    void Start()
    {
        switch (level)
        {
            case 0:
                StartCoroutine(Level0());
                break;
            case 1:
                StartCoroutine(Level1());
                break;
            default:
                break;
        }
    }

    private IEnumerator Level0()
    {
        timeMovesWhenYouDo.SetActive(true);
        yield return new WaitForSeconds(5f);
        timeMovesWhenYouDo.SetActive(false);
        yield return new WaitForSeconds(0.3f);
        tenSecondsToLive.SetActive(true);
        yield return new WaitForSeconds(4f);
        tenSecondsToLive.SetActive(false);

        // Yield until 10 seconds is up.
        do
        {
            yield return 0;
        } while (GameManager.Instance.timeSinceLoad < 10);

        afterThatYouDie.SetActive(true);
        var player = GameObject.FindWithTag("Player");
        var health = player.GetComponent<Health>();
        health.RemoveHealth(1000);
        yield return new WaitForSecondsRealtime(2f);
        GameManager.Instance.StartLoadLevel(2);
    }

    private IEnumerator Level1()
    {
        toSurviveKill.SetActive(true);
        do
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            yield return 0;
            if (enemies.Length == 0)
            {
                toSurviveKill.SetActive(false);
                yield return new WaitForSecondsRealtime(0.8f);
                good.SetActive(true);
                yield return new WaitForSecondsRealtime(2f);
                GameManager.Instance.StartGameNoTutorial();
                yield break;
            }
        } while (GameManager.Instance.timeSinceLoad < 10);

        toSurviveKill.SetActive(false);
        yield return new WaitForSecondsRealtime(0.3f);
        illGiveYouMoreTime.SetActive(true);

        while (true)
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            yield return 0;
            if (enemies.Length == 0)
            {
                illGiveYouMoreTime.SetActive(false);
                yield return new WaitForSecondsRealtime(0.8f);
                tookYouLongEnough.SetActive(true);
                yield return new WaitForSecondsRealtime(2f);
                GameManager.Instance.StartGameNoTutorial();
                yield break;
            }
        }
    }
}
