using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
	public static PlayerController instance;

	[Title("Properties")]
	public float moveSpeed;
	public float miningSpeed = 1f;
	public int miningStrength = 1;
	[SerializeField] float fogRadius = 5f;

	[Title("Audio")]
	[SerializeField] AudioSource audioSource;
	[SerializeField] AudioClip blockBreakSFX;
	[SerializeField] AudioClip blockBounceSFX;
	[SerializeField] AudioClip swishSFX;
	[SerializeField] AudioClip pebbleSFX;


	[Title("References")]
	[SerializeField] PlayerMining playerMining;
	[SerializeField] Rigidbody2D rb;
	[SerializeField] Animator animator;
	[SerializeField] Tilemap fogTilemap;
	[SerializeField] Tilemap layoutTilemap;

	bool _isMining = false;
	Vector2 lookDirection = Vector2.down;

	public bool canInput = true;

	private void OnEnable()
	{
		instance = this;
		playerMining.OnMine += MineBlock;
	}

	private void OnDisable()
	{
		playerMining.OnMine -= MineBlock;
	}

	private void FixedUpdate()
	{
		HandleMove();
	}

	private void Update()
	{
		HandleMining();

		CarveFog();
	}

	public void StopMove()
	{
		rb.linearVelocity = Vector2.zero;
	}

	public void PlaySFXBounce()
	{
		audioSource.PlayOneShot(blockBounceSFX);
	}

	public void PlaySFXBreak()
	{
		audioSource.PlayOneShot(blockBreakSFX);
	}

	public void PlaySFXSwish()
	{
		audioSource.PlayOneShot(swishSFX);
	}

	public void PlaySFXPebble()
	{
		audioSource.PlayOneShot(pebbleSFX);
	}

	public void Knockback()
	{
		StartCoroutine(BlockInputForSeconds(0.5f));
		rb.AddForce(-lookDirection * 5f, ForceMode2D.Impulse);
	}

	void HandleMove()
	{
		if (!canInput)
		{
			animator.SetBool("Moving", false);
			return;
		}

		Vector2 inputMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

		animator.SetBool("Moving", rb.linearVelocity.sqrMagnitude > 0);

		rb.linearVelocity = inputMove * moveSpeed;

		if (inputMove.sqrMagnitude > 0)
			lookDirection = ClampToCardinal(inputMove);

		animator.SetFloat("DirectionX", lookDirection.x);
		animator.SetFloat("DirectionY", lookDirection.y);
	}

	void MineBlock()
	{
		if (MapManager.instance == null)
		{
			PlaySFXSwish();
			return;
		}

		Vector2Int miningPos = (Vector2Int)layoutTilemap.WorldToCell(transform.position + (Vector3)lookDirection * 0.6f);

		MapManager.instance.MineTile(miningPos, miningStrength);
	}

	void HandleMining()
	{
		if (!canInput)
		{
			_isMining = false;
			animator.SetBool("Mining", _isMining);
			return;
		}

		_isMining = Input.GetButton("Fire1");
		animator.SetBool("Mining", _isMining);
		animator.SetFloat("MiningSpeed", miningSpeed);
	}

	void CarveFog()
	{
		if (fogTilemap == null)
			return;

		for (int x = -Mathf.CeilToInt(fogRadius); x <= Mathf.CeilToInt(fogRadius); x++)
		{
			for (int y = -Mathf.CeilToInt(fogRadius); y <= Mathf.CeilToInt(fogRadius); y++)
			{
				if (x * x + y * y <= fogRadius * fogRadius)
				{
					Vector3Int playerCell = fogTilemap.WorldToCell(transform.position);
					Vector3Int cellPos = new(playerCell.x + x, playerCell.y + y, 0);
					fogTilemap.SetTile(cellPos, null); // remove fog
				}
			}
		}
	}

	Vector2 ClampToCardinal(Vector2 dir)
	{
		// Handle zero vector safely
		if (dir == Vector2.zero)
			return Vector2.zero;

		// Compare absolute x and y to find which axis dominates
		if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
		{
			// Horizontal direction dominates
			return dir.x > 0 ? Vector2.right : Vector2.left;
		}
		else
		{
			// Vertical direction dominates
			return dir.y > 0 ? Vector2.up : Vector2.down;
		}
	}

	IEnumerator BlockInputForSeconds(float time)
	{
		canInput = false;
		StopMove();
		yield return new WaitForSeconds(time);
		canInput = true;
	}


	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, fogRadius);

		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(layoutTilemap.WorldToCell(transform.position + (Vector3)lookDirection * 0.6f) + layoutTilemap.tileAnchor, 0.5f);
	}
}
