using UnityEngine;

public class IntroScreenManager : MonoBehaviour
{
    public GameObject introScreen; // Reference to the IntroScreen Canvas
    public GameObject droneFlock; // Reference to the Drone Flock

    void Start()
    {
        // Ensure the intro screen is active at the start
        introScreen.SetActive(true);

        // Stop drone movement at the beginning
        droneFlock.SetActive(false);
    }

    public void StartGame()
    {
        // Disable the intro screen
        introScreen.SetActive(false);

        // Activate the drone movement
        droneFlock.SetActive(true);
    }
}
