using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour
{
	[Title("References")]
	[SerializeField] Image oreIcon;
	[SerializeField] TextMeshProUGUI quantityTMP;

	//[SerializeField] GameObject tooltipObject;
	//[SerializeField] TextMeshProUGUI tooltipTitleTMP;
	//[SerializeField] TextMeshProUGUI tooltipPricePerUnitTMP;
	//[SerializeField] TextMeshProUGUI tooltipTotalCostTMP;

	//bool isTooltipActive = false;

	////private void Update()
	////{
	////	tooltipObject.SetActive(isTooltipActive);
	////	if (isTooltipActive)
	////		tooltipObject.transform.position = Input.mousePosition;
	////}

	//public void OnPointerEnter(PointerEventData eventData)
	//{
	//	isTooltipActive = true;
	//}

	//public void OnPointerExit(PointerEventData eventData)
	//{
	//	isTooltipActive = false;
	//}

	public void SetOre(SO_Ore oreSO, int quantity)
	{
		oreIcon.sprite = oreSO.oreSprite;
		quantityTMP.text = quantity.ToString();
		//tooltipTitleTMP.text = oreSO.oreName;
		//tooltipPricePerUnitTMP.text = $"Price per unit : {oreSO.price}";
		//tooltipTotalCostTMP.text = $"Total price : {oreSO.price * quantity}";
	}
}
