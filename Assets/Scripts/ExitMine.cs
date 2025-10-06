using UnityEngine;

public class ExitMine : MonoBehaviour
{
	[SerializeField] string sceneToLoad;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		GameManager.instance.LoadScene(sceneToLoad);
	}
}
