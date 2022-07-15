using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class PerformanceMeasurer : MonoBehaviour
{
    private int incrementations = 0;
    private float nextIncrementation = 6f;
    private float breakeTime = 5f;

    private int fps;
    private int sumFps = 0;
    private int fpsMeasured = 0;
    private int minFps = int.MaxValue;
    private int maxFps = int.MinValue;
    
    void Start()
    {
        EditorApplication.playModeStateChanged += Log;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.unscaledTime < 1f) return;
        if (Input.GetKey("s")) EditorApplication.ExitPlaymode();

        if (Time.unscaledTime >= nextIncrementation) IncreaseCubes();

        CalculateFPS();
    }

    private void Log(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            SaveCalculations();
        }
    }

    private void SaveCalculations()
    {
        string[] data = (fpsMeasured == 0
                ?
                new string[] { }
                :
                new string[] { "FPS Values:", $"minimum FPS: {minFps}", $"maximum FPS: {maxFps}", $"average FPS: {(fpsMeasured == 0 ? 0 : sumFps / fpsMeasured)}", "" }
                );
        File.WriteAllLines($"Assets/PerformanceReport/data{incrementations}.txt", data);
        Debug.Log(data);

        sumFps = 0;
        fpsMeasured = 0;
        minFps = int.MaxValue;
        maxFps = int.MinValue;
}

    private void IncreaseCubes()
    {
        nextIncrementation = Time.unscaledTime + breakeTime;
        SaveCalculations();
        CubeSpawnSystem.forceCubeIncrementation = true;
        incrementations++;
    }

    private void CalculateFPS()
    {
        fps = (int)(1f / Time.unscaledDeltaTime);
        fpsMeasured++;
        sumFps += fps;

        if (fps > maxFps)
        {
            maxFps = fps;
            return;
        }
        if (fps < minFps) minFps = fps;
    }
}
