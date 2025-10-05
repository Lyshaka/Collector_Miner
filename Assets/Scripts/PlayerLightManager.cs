using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerLightManager : MonoBehaviour
{
	[Title("Light references")]
	[SerializeField] Light2D globalLight;
	[SerializeField] Light2D playerLight;
	[SerializeField] Light2D layoutLight;
	[SerializeField] Light2D wallLight;

	float globalLightOriginalIntensity;
	float playerLightOriginalIntensity;
	float layoutLightOriginalIntensity;
	float wallLightOriginalIntensity;

	private void Start()
	{
		// Global light
		globalLightOriginalIntensity = globalLight.intensity;

		// Player light
		playerLightOriginalIntensity = playerLight.intensity;

		// Layout light
		layoutLightOriginalIntensity = layoutLight.intensity;

		// Wall light
		wallLightOriginalIntensity = wallLight.intensity;

	}

	public void SetLightPower(float ratio)
	{
		globalLight.intensity = Mathf.Lerp(0f, globalLightOriginalIntensity, ratio);
		playerLight.intensity = Mathf.Lerp(0f, playerLightOriginalIntensity, ratio);
		layoutLight.intensity = Mathf.Lerp(0f, layoutLightOriginalIntensity, ratio);
		wallLight.intensity = Mathf.Lerp(0f, wallLightOriginalIntensity, ratio);
	}
}
