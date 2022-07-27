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
[UpdateAfter(typeof(PlayerEnemyCollisionSystem))]
public partial class ProjectileEnemyCollisionSystem : SystemBase
{
    [BurstCompile]
    public struct CollisionJob : ICollisionEventsJob
    {
        public ComponentDataFromEntity<ProjectileTag> projectiles;
        public ComponentDataFromEntity<EnemyTag> enemies;
        public EntityCommandBuffer ecb;

        public void Execute(CollisionEvent collisionEvent)
        {
            if (
                projectiles.HasComponent(collisionEvent.EntityA) &&
                enemies.HasComponent(collisionEvent.EntityB) ||
                projectiles.HasComponent(collisionEvent.EntityB) &&
                enemies.HasComponent(collisionEvent.EntityA)
                )
            {
                ecb.AddComponent(collisionEvent.EntityA, new DestroyTag { });
                ecb.AddComponent(collisionEvent.EntityB, new DestroyTag { });
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

    [BurstCompile]
    protected override void OnUpdate()
    {
        JobHandle playerEnemyCollisionHandle = World.GetOrCreateSystem<PlayerEnemyCollisionSystem>().collisionEventHandle;

        Dependency = JobHandle.CombineDependencies(Dependency, playerEnemyCollisionHandle);

        EntityCommandBuffer commandBuffer = endFixedStepSimulationECB.CreateCommandBuffer();

        JobHandle collisionEventHandle = new CollisionJob
        {
            projectiles = GetComponentDataFromEntity<ProjectileTag>(),
            enemies = GetComponentDataFromEntity<EnemyTag>(),
            ecb = commandBuffer
        }.Schedule(stepPhysicsWorld.Simulation, Dependency);

        endFixedStepSimulationECB.AddJobHandleForProducer(collisionEventHandle);
    }
}
