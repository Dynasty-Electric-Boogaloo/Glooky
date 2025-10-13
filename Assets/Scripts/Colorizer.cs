using UnityEngine;

/// Allows recoloring the first Material of the provided Renderer array according to a player index.
public class Colorizer : MonoBehaviour
{
    /// Renderers whose first material color will be replaced by a player color.
    [Tooltip("Renderers whose first material color will be replaced by a player color.")]
    [SerializeField] private Renderer[] renderers;
    private Material[] _baseMaterials;
    private int _playerIndex;
    
    private void Awake()
    {
        _baseMaterials = new Material[renderers.Length];
        
        for (var i = 0; i < renderers.Length; i++)
        {
            _baseMaterials[i] = renderers[i].sharedMaterial;
        }
    }

    /// Update the Renderers' first Material color according to playerIndex
    /// <param name="playerIndex">Identifies which player the renderers should take their color from,
    ///     -1 is considered as "No Player" and will be assigned a default Color.</param>
    /// <seealso cref="MaterialColorManager"/>
    public void SetPlayerIndex(int playerIndex)
    {
        _playerIndex = playerIndex;
        
        for (var i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterial = MaterialColorManager.Instance.ColorizeMaterial(_baseMaterials[i], _playerIndex);
        }
    }
}