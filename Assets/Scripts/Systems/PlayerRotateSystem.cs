using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;

public partial class PlayerRotateSystem : SystemBase
{
    private const float ROTATIONSPEED = 0.04f;
    protected override void OnUpdate()
    {
        float rotationOffset = ROTATIONSPEED * -Input.GetAxis("Mouse X");

        if (rotationOffset == 0f) return;

        Entities.WithAll<PlayerTag>().ForEach((ref Rotation rotation) => {
            rotation.Value = math.mul(rotation.Value, quaternion.EulerXYZ(0f, -rotationOffset, 0f));
        }).Schedule();
    }
}
