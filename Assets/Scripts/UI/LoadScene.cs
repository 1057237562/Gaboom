using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public string sceneName;
    AsyncOperation operation;
    public void LoadSceneDelay(float second)
    {
        operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;
        StartCoroutine(Delay(second));
    }

    IEnumerator Delay(float second)
    {
        yield return new WaitForSeconds(second);
        operation.allowSceneActivation = true;
    }
}
