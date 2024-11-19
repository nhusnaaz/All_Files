using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    float fps;
    float updateTimer = 0.2f;

    [SerializeField] TextMeshProUGUI fpsTitle; // Ensure this is assigned in the Inspector

    private void updateFPS()
    {
        updateTimer -= Time.deltaTime;
        if (updateTimer < 0)
        {
            updateTimer = 0.2f;

            fps = 1f / Time.unscaledDeltaTime;

            // Check if fpsTitle is not null before accessing its properties
            if (fpsTitle != null)
            {
                fpsTitle.text = "FPS: " + Mathf.Round(fps);
            }
        }
    }

    void Update()
    {
        updateFPS();
    }
}