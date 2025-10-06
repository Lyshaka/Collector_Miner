using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerLightManager : MonoBehaviour
{
	[Title("Light references")]
	[SerializeField] Light2D globalLight;
	[SerializeField] Light2D playerLight;

	float globalLightOriginalIntensity;
	float playerLightOriginalIntensity;

	private void Start()
	{
		// Global light
		globalLightOriginalIntensity = globalLight.intensity;

		// Player light
		playerLightOriginalIntensity = playerLight.intensity;
	}

	public void SetLightPower(float ratio)
	{
		globalLight.intensity = Mathf.Lerp(0f, globalLightOriginalIntensity, ratio);
		playerLight.intensity = Mathf.Lerp(0f, playerLightOriginalIntensity, ratio);
	}
}
