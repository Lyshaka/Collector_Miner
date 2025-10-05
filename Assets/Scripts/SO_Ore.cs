using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "SO_Ore", menuName = "Scriptable Objects/SO_Ore")]
public class SO_Ore : ScriptableObject
{
	[Title("Properties")]
	public string oreName = "";
	public Color color = Color.white;
	[Tooltip("Number of veins per chunk"), Range(1, 20)] public int maxVeinPerChunk = 1;
	[Tooltip("Number of ore per vein"), MinMaxSlider(1, 16)] public Vector2Int veinSize = new(1, 1);
	public int weight = 1;
	public int health = 1;
	public int hardness = 1;
	public int poorQuantity = 1;
	public int mediumQuantity = 2;
	public int richQuantity = 3;

	[Title("References")]
	public Tile[] poorTiles;
	public Tile[] mediumTiles;
	public Tile[] richTiles;
	public Sprite oreSprite;
}
