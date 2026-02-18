using UnityEngine;

namespace WFC
{
    public class TestWfcBlockCreator : MonoBehaviour
    {
        [SerializeField] private WfcConfig config;
        [SerializeField] private PlayerInput input;

        private GameObject _spawnedBlock;

        private const float SUB_BLOCK_SIZE = 1f;

        private void OnEnable()
        {
            input.OnMouseClicked += BuildNewBlock;
        }
        
        private void OnDisable()
        {
            input.OnMouseClicked -= BuildNewBlock;
        }
        
        private void BuildNewBlock()
        {
            if (_spawnedBlock)
                Destroy(_spawnedBlock);
            
            _spawnedBlock = new GameObject();
            _spawnedBlock.transform.position = Vector3.one * SUB_BLOCK_SIZE;
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    for (int z = -1; z < 2; z++)
                    {
                        var coordinates = new Vector3Int(x, y, z);
                        if (coordinates == Vector3Int.zero || coordinates == Vector3Int.down)
                            continue;
                        
                        var subBlockType = SubBlockUtils.GetTypeFromCoordinate(coordinates);
                        var subBlockPrefab = config.GetSubBlockPrefabForType(subBlockType);
                        var newSubBlock = Instantiate(subBlockPrefab, _spawnedBlock.transform);
                        newSubBlock.transform.localPosition = (Vector3)coordinates * SUB_BLOCK_SIZE;
                        newSubBlock.transform.localRotation = SubBlockUtils.GetRotationFromCoordinate(coordinates);
                    }
                }   
            }
        }
    }
}