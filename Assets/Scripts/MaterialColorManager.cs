using System.Collections.Generic;
using UnityEngine;

public class MaterialColorManager : MonoBehaviour
{
    private static MaterialColorManager _instance;
    public static MaterialColorManager Instance => _instance;
        
    [SerializeField] private Color[] playerColors;
    private Dictionary<(int, Material), Material> _materialCache = new();

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    public Material ColorizeMaterial(Material material, int playerIndex)
    {
        //TODO Remove braces on single line condition blocks
        
        if (playerIndex < 0 || playerIndex >= playerColors.Length)
        {
            playerIndex = -1;
        }

        playerIndex++;

        if (_materialCache.ContainsKey((playerIndex, material)))
        {
            return _materialCache[(playerIndex, material)];
        }
            
        var colorized = new Material(material)
        {
            color = playerColors[playerIndex]
        };
        _materialCache.Add((playerIndex, material), colorized);
        return colorized;
    }
}