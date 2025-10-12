using System.Collections.Generic;
using UnityEngine;

/// Singleton component that holds player colors and handles Material recoloring.
public class MaterialColorManager : MonoBehaviour
{
    private static MaterialColorManager _instance;
    public static MaterialColorManager Instance => _instance;
    
    /// Player colors, valid players start at 1 here, and the invalid color is at 0
    /// @note Other code outside this class offset those indices by -1, so players start at 0 and invalid is -1.
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

    /// Create a copy of the passed Material, recolored to the player index's color.
    /// Results are saved, so repeat calls return the same Material instance.
    /// <param name="material">Base Material to be recolored.</param>
    /// <param name="playerIndex">Player Index to select the correct color.</param>
    /// @note Do not pass colorized Materials! This would create a copy each time.
    /// Back up and reuse the base Material if you need to colorize more than once.
    public Material ColorizeMaterial(Material material, int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerColors.Length)
            playerIndex = -1;

        playerIndex++;

        if (_materialCache.ContainsKey((playerIndex, material)))
            return _materialCache[(playerIndex, material)];
            
        var colorized = new Material(material)
        {
            color = playerColors[playerIndex]
        };
        _materialCache.Add((playerIndex, material), colorized);
        return colorized;
    }
}