using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(StepPhysicsWorld))]
[UpdateAfter(typeof(ExportPhysicsWorld))]
[UpdateBefore(typeof(EndFramePhysicsSystem))]
public partial class PlayerEnemyCollisionSystem : SystemBase
{
    [BurstCompile]
    public struct CollisionJob : ICollisionEventsJob
    {
        public ComponentDataFromEntity<PlayerTag> players;
        public ComponentDataFromEntity<EnemyTag> enemies;
        public EntityCommandBuffer ecb;

        public void Execute(CollisionEvent collisionEvent)
        {
            if (
                players.HasComponent(collisionEvent.EntityA) &&
                enemies.HasComponent(collisionEvent.EntityB) ||
                players.HasComponent(collisionEvent.EntityB) &&
                enemies.HasComponent(collisionEvent.EntityA)
                )
            {
                ecb.AddComponent(collisionEvent.EntityA, new GameOverTag { });
            }
        }
    }
    private StepPhysicsWorld stepPhysicsWorld;
    private EndFixedStepSimulationEntityCommandBufferSystem endFixedStepSimulationECB;

    protected override void OnStartRunning()
    {
        this.RegisterPhysicsRuntimeSystemReadWrite();
    }

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<PlayerTag>();
        RequireSingletonForUpdate<EnemyTag>();

        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        endFixedStepSimulationECB = World.GetOrCreateSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer commandBuffer = endFixedStepSimulationECB.CreateCommandBuffer();

        JobHandle collisionEventHandle = new CollisionJob
        {
            players = GetComponentDataFromEntity<PlayerTag>(),
            enemies = GetComponentDataFromEntity<EnemyTag>(),
            ecb = commandBuffer
        }.Schedule(stepPhysicsWorld.Simulation, Dependency);

        endFixedStepSimulationECB.AddJobHandleForProducer(collisionEventHandle);
    }
}
