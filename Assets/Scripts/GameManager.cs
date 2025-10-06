using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager instance;

	const string SAVE_NAME = "save.json";
	const string INVENTORY_NAME = "temp.json";

	[Title("Upgrades")]
	[SerializeField] SO_Upgrades SO_pickaxeStrength;
	[SerializeField] SO_Upgrades SO_pickaxeSpeed;
	[SerializeField] SO_Upgrades SO_moveSpeed;
	[SerializeField] SO_Upgrades SO_lightDuration;
	[SerializeField] SO_Upgrades SO_bagCapacity;

	[Title("Ores SO")]
	[SerializeField] SO_Ore[] oreSOs;

	[Title("Inventory")]
	public int capacity;
	public int money = 0;

	[Title("Light")]
	[SerializeField] PlayerLightManager lightManager;
	[SerializeField, Tooltip("Duration in seconds")] float lightDuration = 60f;
	[SerializeField] AnimationCurve lightShakyness;

	public Action<float> OnUpdateItem;
	public Action<float> OnLightUpdate;

	public Action<int> OnMoneyUpdate;

	DataSaved dataSaved;

	int currentWeight = 0;
	Dictionary<string, int> inventory = new();

	float currentLightState = 0f;
	bool counterStarted = false;

	bool isSceneLoading = false;

	private async void OnEnable()
	{
		instance = this;

		LoadData();

		while (UIManager.instance == null || PlayerController.instance == null)
			await Task.Yield();

		ApplyData();
		LoadInventory();

		currentWeight = GetCurrentWeight();
		OnUpdateItem?.Invoke((float)currentWeight / capacity);

		StartCounter();
	}

	private void Update()
	{
		UpdateLight();
	}

	void StartCounter()
	{
		counterStarted = true;
		currentLightState = lightDuration;
	}

	#region UPGRADES

	public bool CanUpgrade(DataSaved.Type type)
	{
		switch (type)
		{
			case DataSaved.Type.PickaxeStrength:
				return dataSaved.pickaxeStrengthLevel < SO_pickaxeStrength.maxLevel;
			case DataSaved.Type.PickaxeSpeed:
				return dataSaved.pickaxeSpeedLevel < SO_pickaxeSpeed.maxLevel;
			case DataSaved.Type.MoveSpeed:
				return dataSaved.moveSpeedLevel < SO_moveSpeed.maxLevel;
			case DataSaved.Type.LightDuration:
				return dataSaved.lightDurationLevel < SO_lightDuration.maxLevel;
			case DataSaved.Type.BagCapacity:
				return dataSaved.bagCapacityLevel < SO_bagCapacity.maxLevel;
		}
		return false;
	}

	public void UpgradeData(DataSaved.Type type)
	{
		switch (type)
		{
			case DataSaved.Type.PickaxeStrength:
				dataSaved.pickaxeStrengthLevel++;
				break;
			case DataSaved.Type.PickaxeSpeed:
				dataSaved.pickaxeSpeedLevel++;
				break;
			case DataSaved.Type.MoveSpeed:
				dataSaved.moveSpeedLevel++;
				break;
			case DataSaved.Type.LightDuration:
				dataSaved.lightDurationLevel++;
				break;
			case DataSaved.Type.BagCapacity:
				dataSaved.bagCapacityLevel++;
				break;
		}

		ApplyData();
		SaveData();
	}

	void ApplyData()
	{
		money = dataSaved.money;
		OnMoneyUpdate?.Invoke(money);

		capacity = (int)GetValueFromType(DataSaved.Type.BagCapacity);
		lightDuration = GetValueFromType(DataSaved.Type.LightDuration) + 1f; // One extra seconds because I'm cool

		PlayerController.instance.moveSpeed = GetValueFromType(DataSaved.Type.MoveSpeed);
		PlayerController.instance.miningSpeed = GetValueFromType(DataSaved.Type.PickaxeSpeed);
		PlayerController.instance.miningStrength = (int)GetValueFromType(DataSaved.Type.PickaxeStrength);
	}

	#endregion

	#region SAVE_SYSTEM

	void SaveInventory()
	{
		InventoryWrapper inv = new(inventory); 
		string path = Path.Combine(Application.persistentDataPath, INVENTORY_NAME);
		string json = JsonUtility.ToJson(inv, true);
		File.WriteAllText(path, json);
	}

	void LoadInventory()
	{
		string path = Path.Combine(Application.persistentDataPath, INVENTORY_NAME);

		if (!File.Exists(path))
		{
			inventory = new();
		}
		else
		{
			string json = File.ReadAllText(path);
			InventoryWrapper inv = JsonUtility.FromJson<InventoryWrapper>(json);
			inventory = inv.GetValues();
			File.Delete(path);
		}
	}

	public void ClearInventory()
	{
		inventory = new();
	}

	public InventoryWrapper GetInventoryData()
	{
		return new(inventory);
	}

	public void SaveData()
	{
		string path = Path.Combine(Application.persistentDataPath, SAVE_NAME);
		string json = JsonUtility.ToJson(dataSaved, true);
		//Debug.Log("Save JSON : " + json + " at " + path);
		File.WriteAllText(path, json);
	}

	public void LoadData()
	{
		string path = Path.Combine(Application.persistentDataPath, SAVE_NAME);

		if (!File.Exists(path))
		{
			//Debug.Log("Created file !");
			dataSaved = new();
			SaveData();
		}

		string json = File.ReadAllText(path);
		//Debug.Log("Load JSON : " + json + " from " + path);
		dataSaved = JsonUtility.FromJson<DataSaved>(json);
	}

	public int GetCostFromType(DataSaved.Type type)
	{
		SO_Upgrades upgradeSO = null;
		int level = 0;

		switch (type)
		{
			case DataSaved.Type.PickaxeStrength:
				upgradeSO = SO_pickaxeStrength;
				level = dataSaved.pickaxeStrengthLevel;
				break;
			case DataSaved.Type.PickaxeSpeed:
				upgradeSO = SO_pickaxeSpeed;
				level = dataSaved.pickaxeSpeedLevel;
				break;
			case DataSaved.Type.MoveSpeed:
				upgradeSO = SO_moveSpeed;
				level = dataSaved.moveSpeedLevel;
				break;
			case DataSaved.Type.LightDuration:
				upgradeSO = SO_lightDuration;
				level = dataSaved.lightDurationLevel;
				break;
			case DataSaved.Type.BagCapacity:
				upgradeSO = SO_bagCapacity;
				level = dataSaved.bagCapacityLevel;
				break;
		}

		return upgradeSO.costPerLevel * level;
	}

	public float GetValueFromType(DataSaved.Type type)
	{
		SO_Upgrades upgradeSO = null;
		int level = 0;

		switch (type)
		{
			case DataSaved.Type.PickaxeStrength:
				upgradeSO = SO_pickaxeStrength;
				level = dataSaved.pickaxeStrengthLevel;
				break;
			case DataSaved.Type.PickaxeSpeed:
				upgradeSO = SO_pickaxeSpeed;
				level = dataSaved.pickaxeSpeedLevel;
				break;
			case DataSaved.Type.MoveSpeed:
				upgradeSO = SO_moveSpeed;
				level = dataSaved.moveSpeedLevel;
				break;
			case DataSaved.Type.LightDuration:
				upgradeSO = SO_lightDuration;
				level = dataSaved.lightDurationLevel;
				break;
			case DataSaved.Type.BagCapacity:
				upgradeSO = SO_bagCapacity;
				level = dataSaved.bagCapacityLevel;
				break;
		}

		return upgradeSO.baseValue + upgradeSO.valuePerLevel * (level - 1);
	}

	public int GetLevelFromType(DataSaved.Type type)
	{
		int level = 0;

		switch (type)
		{
			case DataSaved.Type.PickaxeStrength:
				level = dataSaved.pickaxeStrengthLevel;
				break;
			case DataSaved.Type.PickaxeSpeed:
				level = dataSaved.pickaxeSpeedLevel;
				break;
			case DataSaved.Type.MoveSpeed:
				level = dataSaved.moveSpeedLevel;
				break;
			case DataSaved.Type.LightDuration:
				level = dataSaved.lightDurationLevel;
				break;
			case DataSaved.Type.BagCapacity:
				level = dataSaved.bagCapacityLevel;
				break;
		}

		return level;
	}

	public float GetUpgradeFromType(DataSaved.Type type)
	{
		SO_Upgrades upgradeSO = null;

		switch (type)
		{
			case DataSaved.Type.PickaxeStrength:
				upgradeSO = SO_pickaxeStrength;
				break;
			case DataSaved.Type.PickaxeSpeed:
				upgradeSO = SO_pickaxeSpeed;
				break;
			case DataSaved.Type.MoveSpeed:
				upgradeSO = SO_moveSpeed;
				break;
			case DataSaved.Type.LightDuration:
				upgradeSO = SO_lightDuration;
				break;
			case DataSaved.Type.BagCapacity:
				upgradeSO = SO_bagCapacity;
				break;
		}

		return upgradeSO.valuePerLevel;
	}

	[Serializable]
	public class DataSaved
	{
		public int money = 0;

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

	[Serializable]
	public class InventoryWrapper
	{
		public Slot[] slots;

		public InventoryWrapper(Dictionary<string, int> inventory)
		{
			SetValues(inventory);
		}

		public void SetValues(Dictionary<string, int> inventory)
		{
			slots = new Slot[inventory.Count];
			for (int i = 0; i < inventory.Count; i++)
			{
				slots[i].key = inventory.ElementAt(i).Key;
				slots[i].value = inventory.ElementAt(i).Value;
			}
		}

		public Dictionary<string, int> GetValues()
		{
			Dictionary<string, int> inventory = new();

			for (int i = 0; i < slots.Length; i++)
				inventory.Add(slots[i].key, slots[i].value);

			return inventory;
		}

		[Serializable]
		public struct Slot
		{
			public string key;
			public int value;
		}
	}

	#endregion

	#region LIGHT
	void UpdateLight()
	{
		if (!counterStarted || lightManager == null)
			return;

		currentLightState -= Time.deltaTime;

		OnLightUpdate?.Invoke(currentLightState / lightDuration);

		if (currentLightState <= 0f)
			StartCoroutine(KillSequence());
	}

	#endregion

	#region INVENTORY
	
	public bool CanAfford(int cost)
	{
		return money >= cost;
	}

	public void SpendMoney(int amount)
	{
		money -= amount;
		dataSaved.money = money;
		SaveData();
		OnMoneyUpdate?.Invoke(money);
	}

	public void AddMoney(int amount)
	{
		money += amount;
		dataSaved.money = money;
		SaveData();
		OnMoneyUpdate?.Invoke(money);
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

		OnUpdateItem?.Invoke((float)currentWeight / capacity);
	}

	public int GetCurrentWeight()
	{
		InventoryWrapper inv = new(inventory);

		int totalWeight = 0;

		for (int i = 0; i < inv.slots.Length; i++)
			totalWeight += inv.slots[i].value * GetSOByName(inv.slots[i].key).weight;

		return totalWeight;
	}

	public int GetTotalValueOfInventory()
	{
		InventoryWrapper inv = new(inventory);

		int total = 0;

		for (int i = 0; i < inv.slots.Length; i++)
			total += inv.slots[i].value * GetSOByName(inv.slots[i].key).price;

		return total;
	}

	public bool SellAll()
	{
		int amount = GetTotalValueOfInventory();
		bool hasAnything = amount > 0;
		money += amount;
		dataSaved.money = money;
		OnMoneyUpdate?.Invoke(money);

		SaveData();

		ClearInventory();
		currentWeight = 0;
		OnUpdateItem?.Invoke((float)currentWeight / capacity);

		return hasAnything;
	}

	public SO_Ore GetSOByName(string name)
	{
		SO_Ore so = null;

		for (int i = 0; i < oreSOs.Length; i++)
		{
			if (oreSOs[i].oreName == name)
				return oreSOs[i];
		}

		return so;
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
		PlayerController.instance.StopMove();
		PlayerController.instance.canInput = false;

		float elapsedTime = 0f;
		float duration = 1f;

		while (elapsedTime < duration)
		{
			lightManager.SetLightPower(1f - (elapsedTime / duration));
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		ClearInventory();
		LoadScene("Assets/Scenes/ShopScene.unity");
	}

	IEnumerator LoadSceneAsync(string name)
	{
		isSceneLoading = true;

		PlayerController.instance.StopMove();
		PlayerController.instance.canInput = false;

		SaveInventory();

		AsyncOperation loadingSceneAO = SceneManager.LoadSceneAsync(name);
		loadingSceneAO.allowSceneActivation = false;


		while (loadingSceneAO.progress < 0.9f)
		{
			//Debug.Log("Scene Loading");
			yield return null;
		}

		PlayerController.instance.canInput = true;
		isSceneLoading = false;

		loadingSceneAO.allowSceneActivation = true;
	}

	#endregion
}
