using System;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    private static EconomyManager instance;

    public static EconomyManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<EconomyManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("[EconomyManager]");
                    instance = go.AddComponent<EconomyManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    public int CurrentGold { get; private set; } = 0;
    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddGold(int amount = 1)
    {
        CurrentGold += amount;
        OnGoldChanged?.Invoke(CurrentGold);
    }

    public void ResetGold()
    {
        CurrentGold = 0;
        OnGoldChanged?.Invoke(CurrentGold);
    }
}
