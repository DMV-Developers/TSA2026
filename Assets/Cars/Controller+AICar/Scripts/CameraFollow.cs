using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    private CinemachineThirdPersonFollow follow;
    public GameObject car;
    private PrometeoCarController carController;
    private float newx;
    private float currentVelocity;

    void Start()
    {
        follow = GetComponent<CinemachineThirdPersonFollow>();
        carController = car.GetComponent<PrometeoCarController>();
    }

    void Update()
    {
        if (carController.isAPressed)
            newx = 2.0f;
        else if (carController.isDPressed)
            newx = -2.0f;
        else
            newx = 0.0f;

        follow.ShoulderOffset.x = Mathf.SmoothDamp(follow.ShoulderOffset.x, newx, ref currentVelocity, 0.5f);
    }
}
