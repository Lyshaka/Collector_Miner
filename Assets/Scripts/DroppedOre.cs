using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class DroppedOre : MonoBehaviour
{
	[Title("Properties")]
	[SerializeField] float timeToReachPlayer = 1f;
	[SerializeField] AnimationCurve movementCurve;
	[SerializeField] AnimationCurve scaleCurve;
	[SerializeField, MinMaxSlider(0f, 1f)] Vector2 forceRange = new(0.5f, 1f);
	[SerializeField] float forceMultiplier = 5f;

	[Title("References")]
	[SerializeField] SpriteRenderer spriteRenderer;
	[SerializeField] ParticleSystem spawnPS;
	[SerializeField] Rigidbody2D rb;
	[SerializeField] Collider2D coll;

	SO_Ore oreSO;

	private void OnEnable()
	{
		spriteRenderer.enabled = false;
		coll.enabled = false;
	}

	public void Init(SO_Ore oreSO)
	{
		this.oreSO = oreSO;
		spriteRenderer.sprite = oreSO.oreSprite;
		var main = spawnPS.main;
		main.startColor = oreSO.color;

		spriteRenderer.enabled = true;
		spawnPS.Play();
		coll.enabled = true;

		Vector2 randomForce = forceMultiplier * Random.Range(forceRange.x, forceRange.y) * Random.insideUnitSphere;
		rb.AddForce(randomForce, ForceMode2D.Impulse);

		if (GameManager.instance.HasCapacityFor(oreSO.weight))
			StartCoroutine(GoToPlayer());
	}

	IEnumerator GoToPlayer()
	{
		yield return new WaitForSeconds(Random.Range(0.5f, 0.7f));

		while (rb.linearVelocity.sqrMagnitude > 0.01f)
			yield return null;

		coll.enabled = false;

		timeToReachPlayer = Mathf.Max(timeToReachPlayer, Vector2.Distance(transform.position, PlayerController.instance.transform.position) / 5f);

		float elapsedTime = 0f;
		while (elapsedTime < timeToReachPlayer)
		{
			float ratio = elapsedTime / timeToReachPlayer;

			rb.MovePosition(Vector2.LerpUnclamped(rb.position, PlayerController.instance.transform.position, movementCurve.Evaluate(ratio)));

			spriteRenderer.transform.localScale = Vector3.one * scaleCurve.Evaluate(ratio);

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		if (GameManager.instance.HasCapacityFor(oreSO.weight))
		{
			GameManager.instance.AddItem(oreSO.name, oreSO.weight);
			Destroy(gameObject);
		}
		else
		{
			coll.enabled = true;

			elapsedTime = 0f;
			float duration = 0.4f;
			while (elapsedTime < duration)
			{
				float ratio = elapsedTime / duration;

				spriteRenderer.transform.localScale = Vector3.one * ratio;

				elapsedTime += Time.deltaTime;
				yield return null;
			}
		}
	}
}
