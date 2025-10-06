using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Shopkeeper : MonoBehaviour
{
	[Title("References")]
	[SerializeField] GameObject shopCanvas;
	[SerializeField] Transform inventoryTransform;
	[SerializeField] GameObject inventorySlotPrefab;
	[SerializeField] TextMeshProUGUI totalTMP;

	[Title("Audio")]
	[SerializeField] AudioSource audioSource;
	[SerializeField] AudioClip cashOutSFX;
	[SerializeField] AudioClip tickSFX;

	[Title("Upgrades References")]
	[SerializeField] Color defaultTextColor = Color.white;
	[SerializeField] Color cantAffordColor = Color.white;
	[SerializeField] Color maxColor = Color.white;
	[SerializeField] UI_Upgrade pickaxeStrength;
	[SerializeField] UI_Upgrade pickaxeSpeed;
	[SerializeField] UI_Upgrade moveSpeed;
	[SerializeField] UI_Upgrade lightDuration;
	[SerializeField] UI_Upgrade bagCapacity;

	private void OnEnable()
	{
		shopCanvas.SetActive(false);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		OpenShop();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			CloseShop();
	}

	void OpenShop()
	{
		GameManager.instance.LoadData();

		UpdateShop();

		shopCanvas.SetActive(true);

		PlayerController.instance.StopMove();
		PlayerController.instance.canInput = false;
	}

	public void CloseShop()
	{
		audioSource.PlayOneShot(tickSFX);
		shopCanvas.SetActive(false);
		PlayerController.instance.canInput = true;
	}

	public void Sell()
	{
		audioSource.PlayOneShot(tickSFX);

		if (GameManager.instance.SellAll())
			audioSource.PlayOneShot(cashOutSFX);

		UpdateShop();
	}

	void UpdateShop()
	{
		pickaxeStrength.SetValues(defaultTextColor, cantAffordColor, maxColor, UpgradeButton);
		pickaxeSpeed.SetValues(defaultTextColor, cantAffordColor, maxColor, UpgradeButton);
		moveSpeed.SetValues(defaultTextColor, cantAffordColor, maxColor, UpgradeButton);
		lightDuration.SetValues(defaultTextColor, cantAffordColor, maxColor, UpgradeButton);
		bagCapacity.SetValues(defaultTextColor, cantAffordColor, maxColor, UpgradeButton);

		GameManager.InventoryWrapper inventoryData = GameManager.instance.GetInventoryData();

		for (int i = inventoryTransform.childCount - 1; i >= 0; i--)
			Destroy(inventoryTransform.GetChild(i).gameObject);

		for (int i = 0; i < inventoryData.slots.Length; i++)
		{
			GameObject obj = Instantiate(inventorySlotPrefab, inventoryTransform);
			SO_Ore oreSO = GameManager.instance.GetSOByName(inventoryData.slots[i].key);
			obj.GetComponent<InventorySlot>().SetOre(oreSO, inventoryData.slots[i].value);
		}

		totalTMP.text = $"+{GameManager.instance.GetTotalValueOfInventory()} <sprite index=0>";
	}

	public void UpgradeButton(GameManager.DataSaved.Type type)
	{
		int cost = GameManager.instance.GetCostFromType(type);

		audioSource.PlayOneShot(tickSFX);

		if (!GameManager.instance.CanAfford(cost) || !GameManager.instance.CanUpgrade(type))
			return;

		GameManager.instance.SpendMoney(cost);
		GameManager.instance.UpgradeData(type);

		UpdateShop();
	}

	[Serializable]
	public class UI_Upgrade
	{
		public SO_Upgrades upgradeSO;
		public GameManager.DataSaved.Type upgrade;
		public TextMeshProUGUI nameTMP;
		public TextMeshProUGUI valueTMP;
		public TextMeshProUGUI upgradeTMP;
		public VerticalLayoutGroup levelVLG;
		public Button button;
		public TextMeshProUGUI costTMP;

		public void SetValues(Color defaultTextColor, Color cantAffordColor, Color maxColor, Action<GameManager.DataSaved.Type> buttonCallback)
		{
			nameTMP.text = upgradeSO.upgradeName;
			float value = GameManager.instance.GetValueFromType(upgrade);
			float upgradeValue = GameManager.instance.GetUpgradeFromType(upgrade);
			int level = GameManager.instance.GetLevelFromType(upgrade);
			bool canUpgrade = level < upgradeSO.maxLevel;

			switch (upgradeSO.type)
			{
				case SO_Upgrades.Type.Value:
					valueTMP.text = value.ToString("0.");
					if (canUpgrade)
						upgradeTMP.text = "+" + upgradeValue.ToString("0.");
					else
					{
						upgradeTMP.color = maxColor;
						upgradeTMP.text = "MAX";
					}
					break;
				case SO_Upgrades.Type.Percentage:
					valueTMP.text = (value * 100f).ToString("##.") + "%";
					if (canUpgrade)
						upgradeTMP.text = "+" + (upgradeValue * 100f).ToString("##.") + "%";
					else
					{
						upgradeTMP.color = maxColor;
						upgradeTMP.text = "MAX";
					}
					break;
				case SO_Upgrades.Type.Seconds:
					valueTMP.text = value.ToString("0.") + "s";
					if (canUpgrade)
						upgradeTMP.text = "+" + upgradeValue.ToString("0." + "s");
					else
					{
						upgradeTMP.color = maxColor;
						upgradeTMP.text = "MAX";
					}
					break;
			}

			for (int i = levelVLG.transform.childCount - 1; i >= 1; i--)
				Destroy(levelVLG.transform.GetChild(i).gameObject);
			GameObject levelObject = levelVLG.transform.GetChild(0).gameObject;
			for (int i = 1; i < level; i++)
				Instantiate(levelObject, levelVLG.transform);

			if (canUpgrade)
			{
				int cost = GameManager.instance.GetCostFromType(upgrade);
				costTMP.text = $"{cost} <sprite index=0>";
				costTMP.color = GameManager.instance.CanAfford(cost) ? defaultTextColor : cantAffordColor;

				button.onClick.RemoveAllListeners();
				button.onClick.AddListener(() => buttonCallback(upgrade) );
				button.transform.gameObject.SetActive(true);
			}
			else
			{
				button.transform.gameObject.SetActive(false);
			}

		}
	}
}
