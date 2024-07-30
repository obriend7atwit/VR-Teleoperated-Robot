using UnityEngine;

public class QuadFollower : MonoBehaviour
{
    public Transform leftEyeQuad;
    public Transform rightEyeQuad;
    public Transform vrCamera;

    public float distanceFromCamera = 1.0f; // Distance of quads from the camera

    void Update()
    {
        if (vrCamera != null)
        {
            // Set the position of the left eye quad
            leftEyeQuad.position = vrCamera.position + (vrCamera.right * -0.5f) + (vrCamera.forward * distanceFromCamera);
            leftEyeQuad.rotation = vrCamera.rotation;

            // Set the position of the right eye quad
            rightEyeQuad.position = vrCamera.position + (vrCamera.right * 0.5f) + (vrCamera.forward * distanceFromCamera);
            rightEyeQuad.rotation = vrCamera.rotation;
        }
    }
}
