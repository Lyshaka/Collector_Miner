using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
	public static MapManager instance;

	const int CHUNK_SIZE = 16;

	[Title("Properties")]
	[SerializeField, Range(1, 8)] int chunkLoadDistance = 3;

	[Title("Player")]
	[SerializeField] Transform playerTransform;
	[SerializeField, ReadOnly] Vector2 _playerPos;
	[SerializeField, ReadOnly] Vector2Int _playerTilePos;
	[SerializeField, ReadOnly] Vector2Int _playerChunkPos;
	[SerializeField, ReadOnly] Vector2Int _playerChunkTilePos;

	[Title("Generation")]
	[SerializeField, MinMaxSlider(0f, 1f)] Vector2 corridorsThreshold = new(0.4f, 0.47f);
	[SerializeField] float corridorsScale = 8f;
	[SerializeField, MinMaxSlider(0f, 1f)] Vector2 caveThreshold = new(0.7f, 1f);
	[SerializeField] float caveScale = 4f;

	[Title("Ores")]
	[SerializeField] SO_Ore[] ores;

	[Title("Tiles References")]
	[SerializeField] Tile groundTile;
	[SerializeField] Tile fogTile;
	[SerializeField] Tile wallTile;

	[Title("Tilmaps References")]
	[SerializeField] Tilemap groundTilemap;
	[SerializeField] Tilemap fogTilemap;
	[SerializeField] Tilemap layoutTilemap;
	[SerializeField] TilemapCollider2D layoutTilemapCollider;

	[Title("Other References")]
	[SerializeField] GameObject destructionParticlePrefab;
	[SerializeField] GameObject droppedOrePrefab;
	[SerializeField] Animator breakingTileAnimator;
	[SerializeField] SpriteRenderer breakingTileRenderer;

	/// <summary>
	/// Position of the player in the world
	/// </summary>
	public Vector2 playerPos => _playerPos;

	/// <summary>
	/// Position of the tile the player stands on
	/// </summary>
	public Vector2Int playerTilePos => _playerTilePos;

	/// <summary>
	/// Coordinates of the chunk the player stands in
	/// </summary>
	public Vector2Int playerChunkPos => _playerChunkPos;

	/// <summary>
	/// Local position of the tile of the chunk the player stands in
	/// </summary>
	public Vector2Int playerChunkTilePos => _playerChunkTilePos;


	Vector2Int[] chunkWithinDistance;
	List<Vector2Int> chunkLoaded = new();

	// Data of tiles based on their pos within a chunk
	/// <summary>
	/// Chunk Pos, (Local Tile Pos, Tile Data)
	/// </summary>
	Dictionary<Vector2Int, Dictionary<Vector2Int, TileData>> chunksData = new();

	Tile[] fogTiles;
	Tile[] groundTiles;
	Tile[] wallTiles;

	MinedTile currentlyMinedTile = null;

	BoundsInt startBounds;

	[Title("Debug")]
	[SerializeField] bool randomSeed = true;
	[SerializeField, DisableIf("@randomSeed")] int seed = 0;
	[SerializeField, ReadOnly] Vector2Int corridorsSeed;
	[SerializeField, ReadOnly] Vector2Int caveSeed;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		chunkWithinDistance = new Vector2Int[(chunkLoadDistance * 2 + 1) * (chunkLoadDistance * 2 + 1)];

		breakingTileRenderer.enabled = false;

		if (randomSeed)
			seed = Random.Range(int.MinValue, int.MaxValue);
		Random.InitState(seed);

		corridorsSeed = RandomRange(-100_000, 100_000);
		caveSeed = RandomRange(-100_000, 100_000);

		CreateBaseTiles();

		// Save tilemap as is
		layoutTilemap.CompressBounds();
		startBounds = layoutTilemap.cellBounds;
		TileBase[] startTiles = layoutTilemap.GetTilesBlock(startBounds);

		ComputeChunkWithinDistance();
		LoadNewChunks();

		// Load tilemap and replace existing blocks
		layoutTilemap.SetTilesBlock(startBounds, startTiles);
	}

	private void Update()
	{
		_playerPos = playerTransform.position;
		_playerTilePos = GetTilePos(groundTilemap, _playerPos);
		_playerChunkPos = GetChunkPos(_playerTilePos);
		_playerChunkTilePos = GetChunkTilePos(_playerTilePos);

		ComputeChunkWithinDistance();
		LoadNewChunks();
	}

	public void MineTile(Vector2Int tilePos, int strength)
	{
		// If there is no tile, nothing happen
		if (!layoutTilemap.HasTile((Vector3Int)tilePos))
		{
			PlayerController.instance.PlaySFXSwish();
			return;
		}

		// If it's a tile from the start area nothing happen
		if (startBounds.Contains((Vector3Int)tilePos))
		{
			PlayerController.instance.PlaySFXBounce();
			PlayerController.instance.Knockback();
			return;
		}

		Vector2Int chunkPos = GetChunkPos(tilePos);
		Vector2Int localTilePos = GetChunkTilePos(tilePos);

		TileData tileData = null;

		// Retrieve data
		if (chunksData.ContainsKey(chunkPos))
		{
			if (chunksData[chunkPos].ContainsKey(localTilePos))
			{
				tileData = chunksData[chunkPos][localTilePos];
			}
			//else
			//{
			//	Debug.Log($"Couldn't retrieve data from tile ({localTilePos}) in chunk ({chunkPos})");
			//}
		}
		//else
		//{
		//	Debug.Log($"Couldn't retrieve data from chunk ({chunkPos})");
		//}

		if (tileData != null)
		{
			// If it's an ore
			if (currentlyMinedTile == null)
				currentlyMinedTile = new(tilePos, tileData);
			else if (currentlyMinedTile.tilePos != tilePos)
			{
				currentlyMinedTile.ResetHealth();
				currentlyMinedTile = new(tilePos, tileData);
			}

			// If we don't have enough strength, knockack and nope
			if (strength < currentlyMinedTile.tileData.oreSO.hardness)
			{
				PlayerController.instance.PlaySFXBounce();
				PlayerController.instance.Knockback();
				return;
			}

			currentlyMinedTile.health -= strength;

			Vector3 worldPos = GetWorldFromTilePos(layoutTilemap, tilePos);

			// Update tile breaking animation here
			breakingTileRenderer.transform.position = worldPos;
			float ratio = 1f - currentlyMinedTile.healthRatio;
			breakingTileAnimator.Play("TileBreaking", 0, ratio);
			breakingTileRenderer.enabled = true;

			if (currentlyMinedTile.health <= 0)
			{
				// Give resources here
				for (int i = 0; i < tileData.quantity; i++)
				{
					GameObject obj = Instantiate(droppedOrePrefab, worldPos + (Vector3)RandomRange(-0.3f, 0.3f), Quaternion.identity);
					obj.GetComponent<DroppedOre>().Init(tileData.oreSO);
				}


				DestroyTile(tilePos);
			}
		}
		else
		{
			// If it's a simple tile
			DestroyTile(tilePos);
		}
	}

	public void DestroyTile(Vector2Int tilePos)
	{
		PlayerController.instance.PlaySFXBreak();
		currentlyMinedTile = null;
		breakingTileRenderer.enabled = false;
		breakingTileAnimator.Play("TileBreaking", 0, 0f);
		Instantiate(destructionParticlePrefab, GetWorldFromTilePos(layoutTilemap, tilePos), Quaternion.identity);
		layoutTilemap.SetTile((Vector3Int)tilePos, null);
	}

	#region CHUNK_MANAGEMENT

	void CreateBaseTiles()
	{
		int size = CHUNK_SIZE * CHUNK_SIZE;
		fogTiles = new Tile[size];
		groundTiles = new Tile[size];
		wallTiles = new Tile[size];

		for (int i = 0; i < size; i++)
		{
			fogTiles[i] = fogTile;
			groundTiles[i] = groundTile;
		}
	}

	bool IsChunkLoaded(Vector2Int chunkPos)
	{
		return chunkLoaded.Contains(chunkPos);
	}

	void GenerateChunk(Vector2Int chunkPos)
	{
		Vector3Int[] positions = new Vector3Int[CHUNK_SIZE * CHUNK_SIZE];
		Vector2Int chunkTilePos = new(chunkPos.x * CHUNK_SIZE, chunkPos.y * CHUNK_SIZE);
		int chunkSeed = seed ^ (chunkPos.x * 73856093) ^ (chunkPos.y * 19349663);

		for (int x = 0; x < CHUNK_SIZE; x++)
			for (int y = 0; y < CHUNK_SIZE; y++)
				positions[y * CHUNK_SIZE + x] = new(chunkTilePos.x + x, chunkTilePos.y + y);

		// Generate chunk data
		GenerateChunkWalls(chunkPos, chunkSeed);
		GenerateChunkOres(chunkPos, chunkSeed);

		groundTilemap.SetTiles(positions, groundTiles);
		fogTilemap.SetTiles(positions, fogTiles);
		layoutTilemap.SetTiles(positions, wallTiles);

		layoutTilemapCollider.ProcessTilemapChanges();

		chunkLoaded.Add(chunkPos);
	}

	void GenerateChunkWalls(Vector2Int chunkPos, int chunkSeed)
	{
		Vector2Int chunkTilePos = new(chunkPos.x * CHUNK_SIZE, chunkPos.y * CHUNK_SIZE);
		Random.InitState(chunkSeed);

		for (int x = 0; x < CHUNK_SIZE; x++)
		{
			for (int y = 0; y < CHUNK_SIZE; y++)
			{
				int worldX = chunkTilePos.x + x;
				int worldY = chunkTilePos.y + y;

				float corridorNoise = (Mathf.PerlinNoise(worldX * corridorsScale + corridorsSeed.x, worldY * corridorsScale + corridorsSeed.y));
				float caveNoise = (Mathf.PerlinNoise(worldX * caveScale + caveSeed.x, worldY * caveScale + caveSeed.y));

				if ((corridorNoise > corridorsThreshold.x && corridorNoise < corridorsThreshold.y) ||
					(caveNoise > caveThreshold.x && caveNoise < caveThreshold.y))
				{
					wallTiles[y * CHUNK_SIZE + x] = null;
				}
				else
				{
					wallTiles[y * CHUNK_SIZE + x] = wallTile;
				}
			}
		}
	}

	void GenerateChunkOres(Vector2Int chunkPos, int chunkSeed)
	{
		Random.InitState(chunkSeed);

		Dictionary<Vector2Int, TileData> chunkData = new();

		for (int oreIndex = 0;  oreIndex < ores.Length; oreIndex++)
		{
			for (int attempt = 0; attempt < ores[oreIndex].maxVeinPerChunk; attempt++)
			{
				int oreAmount = Random.Range(ores[oreIndex].veinSize.x, ores[oreIndex].veinSize.y + 1);
				Vector2Int randomPos = RandomRange(0, CHUNK_SIZE);

				if (wallTiles[randomPos.y * CHUNK_SIZE + randomPos.x] == null || oreAmount == 0)
					continue;

				for (int i = 0; i < oreAmount; i++)
				{
					if (wallTiles[randomPos.y * CHUNK_SIZE + randomPos.x] != null)
					{
						int oreQuantity = 0;
						Tile tile = null;
						switch (Random.Range(0, 3))
						{
							case 0:
								oreQuantity = ores[oreIndex].poorQuantity;
								tile = ores[oreIndex].poorTiles[Random.Range(0, ores[oreIndex].poorTiles.Length)];
								break;
							case 1:
								oreQuantity = ores[oreIndex].mediumQuantity;
								tile = ores[oreIndex].mediumTiles[Random.Range(0, ores[oreIndex].mediumTiles.Length)];
								break;
							case 2:
								oreQuantity = ores[oreIndex].richQuantity;
								tile = ores[oreIndex].richTiles[Random.Range(0, ores[oreIndex].richTiles.Length)];
								break;
						}

						// Save the data of that tile
						TileData tileData = new() { oreSO = ores[oreIndex], quantity = oreQuantity };
						if (chunkData.ContainsKey(randomPos))
							chunkData[randomPos] = tileData;
						else
							chunkData.Add(randomPos, tileData);

						wallTiles[randomPos.y * CHUNK_SIZE + randomPos.x] = tile;
					}

					randomPos += RandomRange(-1, 2);
					randomPos.x = Mathf.Clamp(randomPos.x, 0, CHUNK_SIZE - 1);
					randomPos.y = Mathf.Clamp(randomPos.y, 0, CHUNK_SIZE - 1);
				}
			}
		}

		chunksData.Add(chunkPos, chunkData);
	}

	void LoadNewChunks()
	{
		for (int i = 0; i < chunkWithinDistance.Length; i ++)
			if (!IsChunkLoaded(chunkWithinDistance[i]))
				GenerateChunk(chunkWithinDistance[i]);
	}

	void ComputeChunkWithinDistance()
	{
		int areaSize = chunkLoadDistance * 2 + 1;
		for (int x = 0; x < areaSize; x++)
			for (int y = 0; y < areaSize; y++)
				chunkWithinDistance[y * areaSize + x] = _playerChunkPos + new Vector2Int(x - chunkLoadDistance, y - chunkLoadDistance);
	}

	public static Vector2Int GetTilePos(Tilemap tilemap, Vector2 pos)
	{
		return (Vector2Int)tilemap.WorldToCell(pos);
	}

	public static Vector2Int GetChunkPos(Vector2Int tilePos)
	{
		return new Vector2Int(
			Mathf.FloorToInt((float)tilePos.x / CHUNK_SIZE),
			Mathf.FloorToInt((float)tilePos.y / CHUNK_SIZE)
		);
	}

	public static Vector2Int GetChunkTilePos(Vector2Int tilePos)
	{
		return new Vector2Int(
			((tilePos.x % CHUNK_SIZE) + CHUNK_SIZE) % CHUNK_SIZE,
			((tilePos.y % CHUNK_SIZE) + CHUNK_SIZE) % CHUNK_SIZE
		);
	}

	public static Vector3 GetWorldFromTilePos(Tilemap tilemap, Vector2Int tilePos)
	{
		return (Vector3Int)tilePos + tilemap.tileAnchor;
	}

	#endregion


	public static Vector2 RandomRange(float minInclusive, float maxInclusive)
	{
		return new(Random.Range(minInclusive, maxInclusive), Random.Range(minInclusive, maxInclusive));
	}

	public static Vector2Int RandomRange(int minInclusive, int maxExclusive)
	{
		return new(Random.Range(minInclusive, maxExclusive), Random.Range(minInclusive, maxExclusive));
	}

	public class TileData
	{
		public SO_Ore oreSO;
		public int quantity;
	}

	public class MinedTile
	{
		public Vector2Int tilePos;
		public TileData tileData;
		public int health;
		public float healthRatio => (float)health / tileData.oreSO.hardness;

		public MinedTile(Vector2Int pos, TileData data)
		{
			tilePos = pos;
			tileData = data;
			ResetHealth();
		}

		public void ResetHealth() { health = tileData.oreSO.hardness; }
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(
			new Vector3(_playerChunkPos.x * CHUNK_SIZE + CHUNK_SIZE / 2f, _playerChunkPos.y * CHUNK_SIZE + CHUNK_SIZE / 2f),
			new Vector3(CHUNK_SIZE, CHUNK_SIZE)
		);
	}
}