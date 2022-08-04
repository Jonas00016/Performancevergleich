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
    private const float BREAK_TIME = 10f;
    private const string FILE_NAME = "GamePrototypeReport";

    private string performanceReportPath;

    private int fps;
    private int sumFps = 0;
    private int fpsMeasured = 0;
    private int minFps = int.MaxValue;
    private int maxFps = int.MinValue;
    private float timer = 0f;
    private float timePassed = 0f;

    private EntityQuery enemyQuery;
    private EntityQuery projectileQuery;

    protected override void OnCreate()
    {
#if UNITY_EDITOR
        performanceReportPath = $"Assets/PerformanceReports/{FILE_NAME}.csv";
#else
        QualitySettings.vSyncCount = 0;
        performanceReportPath = $"{Application.dataPath}/PerformanceReports/{FILE_NAME}.csv";
#endif

        enemyQuery = GetEntityQuery(ComponentType.ReadOnly<EnemyTag>());
        projectileQuery = GetEntityQuery(ComponentType.ReadOnly<ProjectileTag>());
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        if (UnityEngine.Time.time < 1f) return;

        timePassed += Time.DeltaTime;
        if (timePassed >= 60f) CheckForEnd(-1);

        CalculateFPS();

        timer += Time.DeltaTime;
        if (timer < BREAK_TIME) return;

        SaveCalculations();

        IncreaseEnemies();
    }
    
    [BurstCompile]
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

    [BurstCompile]
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

        int entitieSum = 2 + enemyQuery.CalculateEntityCountWithoutFiltering() + projectileQuery.CalculateEntityCountWithoutFiltering();

        File.AppendAllText(performanceReportPath, $"\n{minFps}, {avgFps}, {maxFps}, {entitieSum},");

        CheckForEnd(avgFps);

        sumFps = 0;
        fpsMeasured = 0;
        minFps = int.MaxValue;
        maxFps = int.MinValue;
    }

    [BurstCompile]
    private void CheckForEnd(int avgFps)
    {
        if (avgFps >= 24) return;

#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    [BurstCompile]
    private void IncreaseEnemies()
    {
        timer = 0f;
        World.GetOrCreateSystem<EnemySpawnSystem>().SpawnEnemies();
    }
}
