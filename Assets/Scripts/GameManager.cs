using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager instance;

	const string SAVE_NAME = "save.json";

	[Title("Upgrades")]
	[SerializeField] SO_Upgrades SO_pickaxeStrength;
	[SerializeField] SO_Upgrades SO_pickaxeSpeed;
	[SerializeField] SO_Upgrades SO_moveSpeed;
	[SerializeField] SO_Upgrades SO_lightDuration;
	[SerializeField] SO_Upgrades SO_bagCapacity;

	[Title("Inventory")]
	[SerializeField] int capacity;

	[Title("Light")]
	[SerializeField] PlayerLightManager lightManager;
	[SerializeField, Tooltip("Duration in seconds")] float lightDuration = 60f;
	[SerializeField] AnimationCurve lightShakyness;

	public Action<float> OnAddItem;
	public Action<float> OnLightUpdate;

	Upgrades upgrades;

	int currentWeight = 0;
	Dictionary<string, int> inventory = new();

	float currentLightState = 0f;

	bool isSceneLoading = false;

	private void OnEnable()
	{
		instance = this;

		LoadData();

		currentLightState = lightDuration;
	}

	private void Update()
	{
		UpdateLight();
	}

	#region SAVE_SYSTEM

	public void SaveData()
	{
		string path = Path.Combine(Application.persistentDataPath, SAVE_NAME);
		string json = JsonUtility.ToJson(upgrades, true);
		Debug.Log("Save JSON : " + json + " at " + path);
		File.WriteAllText(path, json);
	}

	public void LoadData()
	{
		string path = Path.Combine(Application.persistentDataPath, SAVE_NAME);

		if (!File.Exists(path))
		{
			Debug.Log("Created file !");
			upgrades = new();
			SaveData();
		}

		string json = File.ReadAllText(path);
		Debug.Log("Load JSON : " + json + " from " + path);
		upgrades = JsonUtility.FromJson<Upgrades>(json);
	}

	public int GetCostFromType(Upgrades.Type type)
	{
		SO_Upgrades upgradeSO = null;
		int level = 0;

		switch (type)
		{
			case Upgrades.Type.PickaxeStrength:
				upgradeSO = SO_pickaxeStrength;
				level = upgrades.pickaxeStrengthLevel;
				break;
			case Upgrades.Type.PickaxeSpeed:
				upgradeSO = SO_pickaxeSpeed;
				level = upgrades.pickaxeSpeedLevel;
				break;
			case Upgrades.Type.MoveSpeed:
				upgradeSO = SO_moveSpeed;
				level = upgrades.moveSpeedLevel;
				break;
			case Upgrades.Type.LightDuration:
				upgradeSO = SO_lightDuration;
				level = upgrades.lightDurationLevel;
				break;
			case Upgrades.Type.BagCapacity:
				upgradeSO = SO_bagCapacity;
				level = upgrades.bagCapacityLevel;
				break;
		}

		return upgradeSO.costPerLevel * level;
	}

	public float GetValueFromType(Upgrades.Type type)
	{
		SO_Upgrades upgradeSO = null;
		int level = 0;

		switch (type)
		{
			case Upgrades.Type.PickaxeStrength:
				upgradeSO = SO_pickaxeStrength;
				level = upgrades.pickaxeStrengthLevel;
				break;
			case Upgrades.Type.PickaxeSpeed:
				upgradeSO = SO_pickaxeSpeed;
				level = upgrades.pickaxeSpeedLevel;
				break;
			case Upgrades.Type.MoveSpeed:
				upgradeSO = SO_moveSpeed;
				level = upgrades.moveSpeedLevel;
				break;
			case Upgrades.Type.LightDuration:
				upgradeSO = SO_lightDuration;
				level = upgrades.lightDurationLevel;
				break;
			case Upgrades.Type.BagCapacity:
				upgradeSO = SO_bagCapacity;
				level = upgrades.bagCapacityLevel;
				break;
		}

		return upgradeSO.baseValue + upgradeSO.valuePerLevel * (level - 1);
	}

	public int GetLevelFromType(Upgrades.Type type)
	{
		int level = 0;

		switch (type)
		{
			case Upgrades.Type.PickaxeStrength:
				level = upgrades.pickaxeStrengthLevel;
				break;
			case Upgrades.Type.PickaxeSpeed:
				level = upgrades.pickaxeSpeedLevel;
				break;
			case Upgrades.Type.MoveSpeed:
				level = upgrades.moveSpeedLevel;
				break;
			case Upgrades.Type.LightDuration:
				level = upgrades.lightDurationLevel;
				break;
			case Upgrades.Type.BagCapacity:
				level = upgrades.bagCapacityLevel;
				break;
		}

		return level;
	}

	public float GetUpgradeFromType(Upgrades.Type type)
	{
		SO_Upgrades upgradeSO = null;

		switch (type)
		{
			case Upgrades.Type.PickaxeStrength:
				upgradeSO = SO_pickaxeStrength;
				break;
			case Upgrades.Type.PickaxeSpeed:
				upgradeSO = SO_pickaxeSpeed;
				break;
			case Upgrades.Type.MoveSpeed:
				upgradeSO = SO_moveSpeed;
				break;
			case Upgrades.Type.LightDuration:
				upgradeSO = SO_lightDuration;
				break;
			case Upgrades.Type.BagCapacity:
				upgradeSO = SO_bagCapacity;
				break;
		}

		return upgradeSO.valuePerLevel;
	}

	[Serializable]
	public class Upgrades
	{
		public int pickaxeStrengthLevel = 1;
		public int pickaxeSpeedLevel = 1;
		public int moveSpeedLevel = 1;
		public int lightDurationLevel = 1;
		public int bagCapacityLevel = 1;

		public enum Type
		{
			PickaxeStrength,
			PickaxeSpeed,
			MoveSpeed,
			LightDuration,
			BagCapacity,
		}
	}

	#endregion

	#region LIGHT
	void UpdateLight()
	{
		if (lightManager == null)
			return;

		currentLightState -= Time.deltaTime;

		OnLightUpdate?.Invoke(currentLightState / lightDuration);

		if (currentLightState <= 0f)
			StartCoroutine(KillSequence());
	}

	#endregion

	#region CAPACITY

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

	#endregion

	#region SCENE_MANAGEMENT

	public void LoadScene(string sceneName)
	{
		if (!isSceneLoading)
			StartCoroutine(LoadSceneAsync(sceneName));
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

	IEnumerator LoadSceneAsync(string name)
	{
		isSceneLoading = true;

		AsyncOperation loadingSceneAO = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
		loadingSceneAO.allowSceneActivation = false;


		while (loadingSceneAO.progress < 0.9f)
		{
			Debug.Log("Scene Loading");
			yield return null;
		}


		Scene sceneToLoad = SceneManager.GetSceneByPath(name);
		Scene currentScene = SceneManager.GetActiveScene();
		loadingSceneAO.allowSceneActivation = true;
		SceneManager.UnloadSceneAsync(currentScene);
		SceneManager.SetActiveScene(sceneToLoad);

		isSceneLoading = false;
	}

	#endregion
}
