using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IsometricPlayerController : MonoBehaviour
{
    [SerializeField] private MovementBehaviour movementBehaviour;
    [SerializeField] private AnimationBehaviour animationBehaviour;
    private void Awake()
    {
        if (movementBehaviour == null || animationBehaviour == null)
        {
            Debug.LogError("Inspector is not set correctly");
        }
        else
        {
            animationBehaviour.Initialize();
            movementBehaviour.Initialize();
        }
    }

    private void Update()
    {
        animationBehaviour?.UpdateBehaviour();
    }

    private void FixedUpdate()
    {
        movementBehaviour?.UpdateBehaviour();
    }
}
