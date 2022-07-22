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
public partial class PlayerEnemyCollisionSystem : SystemBase
{
    private StepPhysicsWorld stepPhysicsWorld;
    private BuildPhysicsWorld buildPhysicsWorld;
    private EndFixedStepSimulationEntityCommandBufferSystem endFixedStepSimulationECB;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<PlayerTag>();
        RequireSingletonForUpdate<EnemyTag>();

        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        endFixedStepSimulationECB = World.GetOrCreateSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer commandBuffer = endFixedStepSimulationECB.CreateCommandBuffer();

        JobHandle collisionEventHandle = new CollisionJob
        {
           /* players = GetComponentDataFromEntity<PlayerTag>(),
            enemies = GetComponentDataFromEntity<EnemyTag>(),
            ecb = commandBuffer*/
        }.Schedule(stepPhysicsWorld.Simulation, Dependency);

        collisionEventHandle.Complete();

        endFixedStepSimulationECB.AddJobHandleForProducer(Dependency);
    }
}

public struct CollisionJob : ICollisionEventsJob
{
    /*public ComponentDataFromEntity<PlayerTag> players;
    public ComponentDataFromEntity<EnemyTag> enemies;
    public EntityCommandBuffer ecb;*/

    public void Execute(CollisionEvent collisionEvent)
    {
        Debug.Log("A: " + collisionEvent.EntityA);
        Debug.Log("B: " + collisionEvent.EntityB);
    }
}
