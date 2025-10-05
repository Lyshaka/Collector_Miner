using System;
using UnityEngine;

public class PlayerMining : MonoBehaviour
{
	public Action OnMine;

	public void Mine()
	{
		OnMine?.Invoke();
	}
}
