using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager instance;

	[Title("Inventory")]
	[SerializeField] int capacity;

	[Title("Light")]
	[SerializeField] PlayerLightManager lightManager;
	[SerializeField, Tooltip("Duration in seconds")] float lightDuration = 60;
	[SerializeField] AnimationCurve lightShakyness;

	public Action<float> OnAddItem;
	public Action<float> OnLightUpdate;

	int currentWeight = 0;
	Dictionary<string, int> inventory = new();

	float currentLightState = 0f;

	private void OnEnable()
	{
		instance = this;

		currentLightState = lightDuration;
	}

	private void Update()
	{
		UpdateLight();
	}

	void UpdateLight()
	{
		currentLightState -= Time.deltaTime;

		OnLightUpdate?.Invoke(currentLightState / lightDuration);

		if (currentLightState <= 0f)
			StartCoroutine(KillSequence());
	}

	public bool HasCapacityFor(int space)
	{
		return (capacity - currentWeight) >= space;
	}

	public void AddItem(string name, int weight)
	{
		if (inventory.ContainsKey(name))
			inventory[name]++;
		else
			inventory.Add(name, 1);

		currentWeight += weight;

		OnAddItem?.Invoke((float)currentWeight / capacity);
	}

	IEnumerator KillSequence()
	{
		PlayerController.instance.canInput = false;

		float elapsedTime = 0f;
		float duration = 1f;

		while (elapsedTime < duration)
		{
			lightManager.SetLightPower(1f - (elapsedTime / duration));
			elapsedTime += Time.deltaTime;
			yield return null;
		}
	}
}
