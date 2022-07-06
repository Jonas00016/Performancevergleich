using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

class TestGameObject : MonoBehaviour
{
    public float Value;
}

struct TestStruct : IComponentData
{
    public float SquaredValue;
}

class TestStructConverter : GameObjectConversionSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((TestGameObject input) =>
        {
            var entity = GetPrimaryEntity(input);

            DstEntityManager.AddComponentData(entity, new TestStruct
            {
                SquaredValue = input.Value * input.Value
            });

            Debug.Log("Converted a GameObject!");
        });
    }
}