using UnityEngine;
using System.Collections;

public class Framerate : MonoBehaviour
{
    int frameCount = 0;
    float totalFrameTime = 0.0f;
    float highestFrameRate = 0.0f;
    float lowestFrameRate = float.MaxValue;
    float reportInterval = 5.0f; // Report every 5 seconds
    public bool startchecking = false;

    IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(reportInterval);
            ReportFramerate();
        }
    }

    void Update()
    {
        if (startchecking)
        {
            float currentFrameTime = Time.deltaTime;
            frameCount++;
            totalFrameTime += currentFrameTime;

            if (currentFrameTime > 0) // Important check!
            {
                float currentFPS = 1.0f / currentFrameTime;

                if (currentFPS > highestFrameRate)
                {
                    highestFrameRate = currentFPS;
                }

                if (currentFPS < lowestFrameRate)
                {
                    lowestFrameRate = currentFPS;
                }
            }
        }
    }

    public void ReportFramerate()
    {
        float averageFrameRate = frameCount / totalFrameTime;

        Debug.Log("Highest Framerate: " + highestFrameRate.ToString("F2"));
        Debug.Log("Lowest Framerate: " + lowestFrameRate.ToString("F2"));
        Debug.Log("Average Framerate: " + averageFrameRate.ToString("F2"));
    }
}