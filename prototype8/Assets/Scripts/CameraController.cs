using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 8f;

    private float shakeIntensity;
    private float shakeDuration;
    private bool needsSnap = true;

    void LateUpdate()
    {
        if (target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                needsSnap = true;
            }
            else
            {
                return;
            }
        }

        Vector3 desired = new Vector3(target.position.x, target.position.y, -10f);

        if (needsSnap)
        {
            transform.position = desired;
            needsSnap = false;
        }
        else
        {
            transform.position = Vector3.Lerp(
                transform.position,
                desired,
                smoothSpeed * Time.deltaTime
            );
        }

        if (shakeDuration > 0)
        {
            transform.position += (Vector3)Random.insideUnitCircle * shakeIntensity;
            shakeDuration -= Time.deltaTime;
        }
    }

    public void SnapToTarget()
    {
        needsSnap = true;
    }

    public void Shake(float intensity, float duration)
    {
        shakeIntensity = intensity;
        shakeDuration = duration;
    }
}
