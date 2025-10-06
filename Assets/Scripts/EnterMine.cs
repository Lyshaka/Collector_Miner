using UnityEngine;

public class EnterMine : MonoBehaviour
{
	[SerializeField] string displayMessage = "Enter mine ?";
	[SerializeField] string sceneToLoad;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Debug.Log(displayMessage);
		GameManager.instance.LoadScene(sceneToLoad);
	}
}