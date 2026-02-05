using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerProgress : MonoBehaviour
{
    [Header("Settings")]
    public int stepsPerCoin = 1000;
    
    [Header("UI")]
    private Text stepCount;
    public TextMeshProUGUI stepCountTMP;
    private Text coinsCount;
    public TextMeshProUGUI coinsCountTMP;
    public Button addStepsButton;
    public Button addMoreStepsButton;
    
    [Header("Current Values")]
    public int currentSteps = 0;
    public int currentCoins = 0;
    
    private const string STEPS_KEY = "Steps";
    private const string COINS_KEY = "Coins";

    void Start()
    {
        // Load saved data
        currentSteps = PlayerPrefs.GetInt(STEPS_KEY, 0);
        currentCoins = PlayerPrefs.GetInt(COINS_KEY, 0);

        // Setup buttons safely (remove existing listeners first)
        if (addStepsButton != null)
        {
            addStepsButton.onClick.RemoveListener(AddSteps);
            addStepsButton.onClick.AddListener(AddSteps);
        }

        if (addMoreStepsButton != null)
        {
            addMoreStepsButton.onClick.RemoveListener(AddThousandSteps);
            addMoreStepsButton.onClick.AddListener(AddThousandSteps);
        }

        UpdateUI();
        Debug.Log("PlayerProgress loaded: " + currentSteps + " steps, " + currentCoins + " coins");
    }
    
    public void AddSteps()
    {
        // Add 100 steps for testing
        currentSteps += 100;
        
        // Convert steps to coins
        int newCoins = currentSteps / stepsPerCoin;
        if (newCoins > currentCoins)
        {
            int coinsEarned = newCoins - currentCoins;
            currentCoins = newCoins;
            Debug.Log("Earned " + coinsEarned + " coins!");
        }
        
        SaveAndUpdateUI();
    }

    /// <summary>
    /// Add 1000 steps (for the second button)
    /// </summary>
    public void AddThousandSteps()
    {
        currentSteps += 1000;

        // Convert steps to coins
        int newCoins = currentSteps / stepsPerCoin;
        if (newCoins > currentCoins)
        {
            int coinsEarned = newCoins - currentCoins;
            currentCoins = newCoins;
            Debug.Log("Earned " + coinsEarned + " coins!");
        }

        SaveAndUpdateUI();
    }
    
    public void ResetProgress()
    {
        currentSteps = 0;
        currentCoins = 0;
        SaveAndUpdateUI();
        Debug.Log("Progress reset");
    }
    
    void SaveAndUpdateUI()
    {
        // Save progress and update UI
        SaveProgress();
        UpdateUI();
    }

    /// <summary>
    /// Save current steps and coins to PlayerPrefs
    /// </summary>
    void SaveProgress()
    {
        PlayerPrefs.SetInt(STEPS_KEY, currentSteps);
        PlayerPrefs.SetInt(COINS_KEY, currentCoins);
        PlayerPrefs.Save();
    }
    
    void UpdateUI()
    {
        if (stepCount != null)
            stepCount.text = "Steps: " + currentSteps;
        else if (stepCountTMP != null)
            stepCountTMP.text = "Steps: " + currentSteps;

        if (coinsCount != null)
            coinsCount.text = "Coins: " + currentCoins;
        else if (coinsCountTMP != null)
            coinsCountTMP.text = "Coins: " + currentCoins;
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SaveProgress();
    }

    void OnApplicationQuit()
    {
        SaveProgress();
    }
}
