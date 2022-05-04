using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CardRotateInDrag : MonoBehaviour
{
    private const float EPSILON = 0.0001f;
    private const float SMOOTH_DAMP_SEC_FUDGE = 0.1f;
    private float VerticalRotation;
    private float HorizontalRotation;
    private float VerticalForce;
    private float HorizontalForce;
    private Vector3 m_prevPos;
    private Vector3 m_originalAngles;

    public float XAxisClamp;
    public float XAxisForce;
    public float YAxisClamp;
    public float YAxisForce;
    public float restTime;

    private void Update()
    {
        Vector3 position = transform.position;
        Vector3 vector3 = position - m_prevPos;
        if ((double)vector3.sqrMagnitude > 9.99999974737875E-05)
        {
            VerticalRotation += vector3.z * YAxisForce;
            VerticalRotation = Mathf.Clamp(VerticalRotation, -YAxisClamp, YAxisClamp);
            HorizontalRotation -= vector3.x * XAxisForce;
            HorizontalRotation = Mathf.Clamp(HorizontalRotation, -XAxisClamp, XAxisClamp);
        }
        VerticalRotation = Mathf.SmoothDamp(VerticalRotation, 0.0f, ref VerticalForce, restTime * 0.1f);
        HorizontalRotation = Mathf.SmoothDamp(HorizontalRotation, 0.0f, ref HorizontalForce, restTime * 0.1f);
        transform.localRotation = Quaternion.Euler(m_originalAngles.x + VerticalRotation, m_originalAngles.y, m_originalAngles.z + HorizontalRotation);
        m_prevPos = position;
    }
    public void Reset()
    {
        m_prevPos = transform.position;
        transform.localRotation = Quaternion.Euler(m_originalAngles);
        HorizontalRotation = 0.0f;
        HorizontalForce = 0.0f;
        VerticalRotation = 0.0f;
        VerticalForce = 0.0f;
    }
}
