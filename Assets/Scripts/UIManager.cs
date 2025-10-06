using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public static UIManager instance;

	[Title("Properties")]
	[SerializeField] Color inventoryColorEmpty = Color.green;
	[SerializeField] Color inventoryColorFull = Color.red;
	[SerializeField] float inventoryScaleDuration = 0.3f;
	[SerializeField] float inventoryScaleMultiplier = 1.5f;

	[Title("References")]
	[SerializeField] TextMeshProUGUI moneyText;
	[SerializeField] RectTransform inventoryIcon;
	[SerializeField] RectTransform lightIcon;
	[SerializeField] Image inventoryProgressBar;
	[SerializeField] Image lightProgressBar;
	[SerializeField] Material progressMaterial;

	Material inventoryMat;
	Material lightMat;

	float inventoryScaleElapsedTime = 0f;

	private void OnEnable()
	{
		instance = this;
		inventoryProgressBar.material = new(progressMaterial);
		inventoryMat = inventoryProgressBar.material;
		lightProgressBar.material = new(progressMaterial);
		lightMat = lightProgressBar.material;

		inventoryMat.SetFloat("_Right", 1f);

		GameManager.instance.OnMoneyUpdate += UpdateMoney;
		GameManager.instance.OnAddItem += AddItem;
		GameManager.instance.OnLightUpdate += UpdateLight;
	}

	private void OnDisable()
	{
		GameManager.instance.OnMoneyUpdate -= UpdateMoney;
		GameManager.instance.OnAddItem -= AddItem;
		GameManager.instance.OnLightUpdate -= UpdateLight;
	}

	private void Update()
	{
		if (inventoryScaleElapsedTime > 0f)
		{
			inventoryIcon.localScale = Vector3.one * Mathf.Lerp(1f, inventoryScaleMultiplier, inventoryScaleElapsedTime / inventoryScaleDuration);
			inventoryScaleElapsedTime -= Time.deltaTime;
		}
	}

	void UpdateMoney(int amount)
	{
		moneyText.text = $"<sprite index=0> {amount}";
	}

	void UpdateLight(float progress)
	{
		lightMat.SetFloat("_Left", 1f - progress);
	}

	void AddItem(float progress)
	{
		inventoryScaleElapsedTime = inventoryScaleDuration;
		inventoryMat.SetFloat("_Right", 1f - progress);
		inventoryProgressBar.color = Color.Lerp(inventoryColorEmpty, inventoryColorFull, progress);
	}
}
