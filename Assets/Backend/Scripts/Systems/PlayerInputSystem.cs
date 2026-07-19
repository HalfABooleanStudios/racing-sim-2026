using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerInputSystem : SystemBase
{
    InputAction inputMove;
    InputAction inputEBrake;

    protected override void OnCreate()
    {
        inputMove = InputSystem.actions.FindAction("Move");
        inputEBrake = InputSystem.actions.FindAction("EBrake");
    }

    protected override void OnUpdate()
    {
        foreach (var carState in SystemAPI.Query<RefRW<CarState>>()
            .WithAny<TagPlayerCar, GhostOwnerIsLocal>())
        {
            float2 moveFBLR = inputMove.ReadValue<Vector2>();
            carState.ValueRW.moveCommand = new float3(
                moveFBLR.x,
                moveFBLR.y,
                inputEBrake.ReadValue<float>()
            );
            Debug.Log(carState.ValueRO.moveCommand);
        }
    } 
}