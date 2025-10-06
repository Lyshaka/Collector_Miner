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

		pickaxeStrength.SetValues();
		pickaxeSpeed.SetValues();
		moveSpeed.SetValues();
		lightDuration.SetValues();
		bagCapacity.SetValues();

		shopCanvas.SetActive(true);
		PlayerController.instance.canInput = false;
	}

	[Serializable]
	public class UI_Upgrade
	{
		public SO_Upgrades upgradeSO;
		public GameManager.Upgrades.Type upgrade;
		public TextMeshProUGUI nameTMP;
		public TextMeshProUGUI valueTMP;
		public TextMeshProUGUI upgradeTMP;
		public VerticalLayoutGroup levelVLG;
		public Button button;
		public TextMeshProUGUI costTMP;

		public void SetValues()
		{
			nameTMP.text = upgradeSO.upgradeName;
			float value = GameManager.instance.GetValueFromType(upgrade);
			float upgradeValue = GameManager.instance.GetUpgradeFromType(upgrade);
			int level = GameManager.instance.GetLevelFromType(upgrade);

			switch (upgradeSO.type)
			{
				case SO_Upgrades.Type.Value:
					valueTMP.text = value.ToString("0.");
					if (level < upgradeSO.maxLevel)
						upgradeTMP.text = "+" + upgradeValue.ToString("0.");
					else
					{
						upgradeTMP.color = new(0.7f, 0.04f, 0.04f);
						upgradeTMP.text = "MAX";
					}
					break;
				case SO_Upgrades.Type.Percentage:
					valueTMP.text = (value * 100f).ToString("##.") + "%";
					if (level < upgradeSO.maxLevel)
						upgradeTMP.text = "+" + (upgradeValue * 100f).ToString("##.") + "%";
					else
					{
						upgradeTMP.color = new(0.7f, 0.04f, 0.04f);
						upgradeTMP.text = "MAX";
					}
					break;
				case SO_Upgrades.Type.Seconds:
					valueTMP.text = value.ToString("0.") + "s";
					if (level < upgradeSO.maxLevel)
						upgradeTMP.text = "+" + upgradeValue.ToString("0." + "s");
					else
					{
						upgradeTMP.color = new(0.7f, 0.04f, 0.04f);
						upgradeTMP.text = "MAX";
					}
					break;
			}

			
			GameObject levelObject = levelVLG.transform.GetChild(0).gameObject;
			for (int i = 1; i < level; i++)
				Instantiate(levelObject, levelVLG.transform);

			int cost = GameManager.instance.GetCostFromType(upgrade);
			costTMP.text = $"{cost} <sprite index=0>";
		}
	}
}
