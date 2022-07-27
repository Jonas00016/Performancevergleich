using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateBefore(typeof(DestroySystem))]
public partial class GameOverSystem : SystemBase
{
    private EntityQuery gameOverQuery;
    private EndSimulationEntityCommandBufferSystem endSimulationECB;

    protected override void OnCreate()
    {
        gameOverQuery = GetEntityQuery(ComponentType.ReadOnly<GameOverTag>());
        endSimulationECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        bool gameOver = gameOverQuery.CalculateEntityCountWithoutFiltering() > 0;
        if (!gameOver) return;

        World.GetOrCreateSystem<PlayerSpawnSystem>().Enabled = false;
        World.GetOrCreateSystem<EnemySpawnSystem>().Enabled = false;
        EntityCommandBuffer.ParallelWriter commandBuffer = endSimulationECB.CreateCommandBuffer().AsParallelWriter();

        Entities
            .WithAny<PlayerTag, EnemyTag, ProjectileTag>()
            .ForEach((Entity e, int entityInQueryIndex) => 
            {
                commandBuffer.AddComponent(entityInQueryIndex, e, new DestroyTag { });
            }
        ).ScheduleParallel();

        endSimulationECB.AddJobHandleForProducer(Dependency);
    }
}
