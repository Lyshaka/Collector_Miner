using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
	[SerializeField] float smoothDampTime = 0.3f;
	[SerializeField] Transform transformToFollow;

	Vector3 currentVelocity;

	private void LateUpdate()
	{
		transform.position = Vector3.SmoothDamp(transform.position, transformToFollow.position + Vector3.forward * -10f, ref currentVelocity, smoothDampTime);
	}
}
