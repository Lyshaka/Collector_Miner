using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Shopkeeper : MonoBehaviour
{
	[Title("References")]
	[SerializeField] GameObject shopCanvas;

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
		Debug.Log("Shopkeeper");
		OpenShop();
	}

	void OpenShop()
	{
		GameManager.instance.LoadData();

		UpdateShop();

		shopCanvas.SetActive(true);
		PlayerController.instance.canInput = false;
	}

	void UpdateShop()
	{
		pickaxeStrength.SetValues(defaultTextColor, cantAffordColor, maxColor, UpgradeButton);
		pickaxeSpeed.SetValues(defaultTextColor, cantAffordColor, maxColor, UpgradeButton);
		moveSpeed.SetValues(defaultTextColor, cantAffordColor, maxColor, UpgradeButton);
		lightDuration.SetValues(defaultTextColor, cantAffordColor, maxColor, UpgradeButton);
		bagCapacity.SetValues(defaultTextColor, cantAffordColor, maxColor, UpgradeButton);
	}

	public void UpgradeButton(GameManager.DataSaved.Type type)
	{
		int cost = GameManager.instance.GetCostFromType(type);

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
			{
				Debug.Log("Destroy children !!!!");
				Destroy(levelVLG.transform.GetChild(i).gameObject);
			}
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
