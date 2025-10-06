using UnityEngine;

[CreateAssetMenu(fileName = "SO_Upgrades", menuName = "Scriptable Objects/SO_Upgrades")]
public class SO_Upgrades : ScriptableObject
{
	public string upgradeName = "DEFAULT_NAME";
	public int costPerLevel = 100;
	public float baseValue = 1f;
	public float valuePerLevel = 1f;
	public int maxLevel = 10;
	public Type type = Type.Value;

	public enum Type
	{
		Value,
		Percentage,
		Seconds,
	}
}
