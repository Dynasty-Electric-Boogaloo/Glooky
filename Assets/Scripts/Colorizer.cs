using UnityEngine;

public class Colorizer : MonoBehaviour
{
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

    public void SetPlayerIndex(int playerIndex)
    {
        _playerIndex = playerIndex;
        
        for (var i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterial = MaterialColorManager.Instance.ColorizeMaterial(_baseMaterials[i], _playerIndex);
        }
    }
}