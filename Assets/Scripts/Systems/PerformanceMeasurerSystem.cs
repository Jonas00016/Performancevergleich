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
    private const int ROTATING_MAX_AMOUNT_MONO = 27500;

    private string fileName = "performanceReport";
    private string performanceReportPath;

    private int fps;
    private int sumFps = 0;
    private int fpsMeasured = 0;
    private int minFps = int.MaxValue;
    private int maxFps = int.MinValue;

    private int incrementations = 0;
    private float nextIncrementation = 6f;
    private float breakeTime = 5f;
    private int spawnAmount = -1;

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

        spawnAmount = World.GetOrCreateSystem<CubeSpawnSystem>().spawnAmount;
    }

    protected override void OnUpdate()
    {
        if (UnityEngine.Time.time < 0f) return;

        CalculateFPS();

        if (UnityEngine.Time.time < nextIncrementation) return;

        SaveCalculations();

        IncreaseCubes();
    }

    private void CalculateFPS()
    {
        fps = (int)(1f / Time.DeltaTime);
        fpsMeasured++;
        sumFps += fps;

        if (fps > maxFps)
        {
            maxFps = fps;
            return;
        }
        if (fps < minFps) minFps = fps;
    }

    private void SaveCalculations()
    {
#if UNITY_EDITOR
        if (!File.Exists(performanceReportPath))
        {
            File.WriteAllText(performanceReportPath, "MIN_FPS, AVG_FPS, MAX_FPS, CUBES,");
        }
#else
        string directory = performanceReportPath.Substring(0, performanceReportPath.LastIndexOf("/"));
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
#endif

        int avgFps = (fpsMeasured == 0 ? 0 : sumFps / fpsMeasured);

        File.AppendAllText(performanceReportPath, $"\n{minFps}, {avgFps}, {maxFps}, {spawnAmount * incrementations},");

        CheckForEnd(avgFps);

        sumFps = 0;
        fpsMeasured = 0;
        minFps = int.MaxValue;
        maxFps = int.MinValue;
    }

    private void CheckForEnd(int avgFps)
    {
        int spawnedEntities = cubeQuery.CalculateEntityCountWithoutFiltering();
        if (avgFps >= 24 && spawnedEntities < ROTATING_MAX_AMOUNT_MONO) return;

#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    private void IncreaseCubes()
    {
        nextIncrementation = UnityEngine.Time.time + breakeTime;

        incrementations++;

        World.GetOrCreateSystem<CubeSpawnSystem>().SpawnNextWave();
    }
}
