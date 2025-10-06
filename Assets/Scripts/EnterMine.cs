using UnityEngine;

public class EnterMine : MonoBehaviour
{
	[SerializeField] string sceneToLoad;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		GameManager.instance.ClearInventory();
		GameManager.instance.LoadScene(sceneToLoad);
	}
}