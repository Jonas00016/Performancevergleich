using System.IO;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

[AlwaysUpdateSystem]
public partial class PerformanceMeasurerSystem : SystemBase
{
    private const int PHYSICS_MAX_AMOUNT_MONO = 8000;

    private string fileName = "performanceReport";
    private string performanceReportPath;

    private int fps;
    private int sumFps = 0;
    private int fpsMeasured = 0;

    private int spawnAmount = -1;
    private string tempFPSCalculations = "";

    private EntityQuery cubeQuery;

    protected override void OnCreate()
    {
#if UNITY_EDITOR
        performanceReportPath = $"Assets/PerformanceReports/{fileName}.csv";
#else
        QualitySettings.vSyncCount = 0;
        performanceReportPath = $"{Application.dataPath}/PerformanceReports/{fileName}.csv";
#endif

        cubeQuery = GetEntityQuery(ComponentType.ReadOnly<CubeTag>());
    }

    protected override void OnUpdate()
    {
        if (UnityEngine.Time.time < 0f) return;

        CalculateFPS();

        SaveCalculations();
    }

    private void CalculateFPS()
    {
        fps = (int)(1f / Time.DeltaTime);
        fpsMeasured++;
        sumFps += fps;

        spawnAmount = World.GetOrCreateSystem<CubeSpawnSystem>().SpawnNextWave();
    }

    private void SaveCalculations()
    {
#if UNITY_EDITOR
        if (!File.Exists(performanceReportPath))
        {
            File.WriteAllText(performanceReportPath, "FPS, CUBES,");
        }
#else
        string directory = performanceReportPath.Substring(0, performanceReportPath.LastIndexOf("/"));
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
#endif

        tempFPSCalculations += $"\n{fps}, {spawnAmount}";

        if (spawnAmount % 50 != 0) return;

        File.AppendAllText(performanceReportPath, tempFPSCalculations);

        tempFPSCalculations = "";

        CheckForEnd(sumFps / fpsMeasured);

        sumFps = 0;
        fpsMeasured = 0;
    }

    private void CheckForEnd(int avgFps)
    {
        int spawnedEntities = cubeQuery.CalculateEntityCountWithoutFiltering();
        if (avgFps >= 24 && spawnedEntities < PHYSICS_MAX_AMOUNT_MONO) return;

#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}
