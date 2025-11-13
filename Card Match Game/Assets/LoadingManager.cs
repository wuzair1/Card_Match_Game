using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressText;
    [SerializeField] private float smoothSpeed = 0.5f; // Slider smoothing speed

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    public void LoadScene(string sceneName)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        StartCoroutine(LoadAsync(sceneName));
    }

    private IEnumerator LoadAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float displayedProgress = 0f;

        while (!operation.isDone)
        {
            // Smoothly move slider towards target progress
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
            displayedProgress = Mathf.Lerp(displayedProgress, targetProgress, smoothSpeed * Time.deltaTime);

            if (progressBar != null) progressBar.value = displayedProgress;
            if (progressText != null) progressText.text = $"{Mathf.RoundToInt(displayedProgress * 100)}%";

            // Activate scene when fully loaded
            if (operation.progress >= 0.9f && Mathf.Abs(displayedProgress - 1f) < 0.01f)
            {
                yield return new WaitForSeconds(0.3f); // small delay for effect
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }
}
