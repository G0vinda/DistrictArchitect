using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using WFC.TestScene;
using static System.Int32;
using Random = UnityEngine.Random;

namespace WFC
{
    public class WfcBlockCreator : MonoBehaviour
    {
        [field: SerializeField] public WfcConfig Config { get; private set; }
        
        [SerializeField] private GameObject blockPrefab;
        [SerializeField] private BuildingSelection selection;

        private GameObject _spawnedBlock;
        
        private Dictionary<Vector3Int, GameObject> _subBlockGameObjectsBySmallCoordinate = new();
        private Dictionary<Vector3Int, int> _subBlockIdsBySmallCoordinate = new();
        private Dictionary<Vector3Int, GameObject> _blockObjectByBigCoordinate = new();
        private Vector3Int _lastHoveredCoordinate;

        private void Awake()
        {
            blockPrefab.GetComponent<BoxCollider>().size = 3f * Config.subBlockSize * Vector3.one;
        }

        public void Rebuild()
        {
            var buildingTypeByGlobalSmallCoordinate = new Dictionary<Vector3Int, BuildingType>();
            foreach (var (coordinate, id) in _subBlockIdsBySmallCoordinate)
            {
                var subBlockType = (BuildingType)Config.SubBlockTable.Rows.Find(id)[WfcConfig.BUILDING_COLUMN_INDEX];
                Destroy(_subBlockGameObjectsBySmallCoordinate[coordinate]);
                buildingTypeByGlobalSmallCoordinate.Add(coordinate, subBlockType);
            }
            _subBlockIdsBySmallCoordinate.Clear();
            _subBlockGameObjectsBySmallCoordinate.Clear();
            BuildNewShape(buildingTypeByGlobalSmallCoordinate);
        }

        public void BuildNewBlock(Vector3Int blockCoordinate)
        {
            var buildingTypeByGlobalSmallCoordinates = new Dictionary<Vector3Int, BuildingType>();
            
            for (var x = -1; x < 2; x++)
            {
                for (var y = -1; y < 2; y++)
                {
                    for (var z = -1; z < 2; z++)
                    {
                        var localSubBlockCoordinate = new Vector3Int(x, y, z);
                        var globalSubBlockCoordinate = GetGlobalSmallCoordinateFromLocal(localSubBlockCoordinate, blockCoordinate);
                        
                        buildingTypeByGlobalSmallCoordinates.Add(globalSubBlockCoordinate, selection.SelectedBuildingType);
                    }
                }   
            }
            
            foreach (var direction in Vector3IntUtils.Directions)
            {
                var neighborBigCoordinate = blockCoordinate + direction;
                if (!_blockObjectByBigCoordinate.ContainsKey(neighborBigCoordinate))
                    continue;

                var globalCoordinateOnNeighbourSide = GetGlobalSmallCoordinateFromLocal(direction * 2, blockCoordinate);
                var touchingNeighborBlockCoordinates = globalCoordinateOnNeighbourSide.GetSurrounding3x3Coordinates(direction);

                var neighborId = _subBlockIdsBySmallCoordinate[touchingNeighborBlockCoordinates.First()]; // Todo: find a better way to do this
                var neighborBuildingType = (BuildingType)Config.SubBlockTable.Rows.Find(neighborId)[WfcConfig.BUILDING_COLUMN_INDEX];
                
                foreach (var touchingNeighborBlockCoordinate in touchingNeighborBlockCoordinates)
                {
                    if (_subBlockIdsBySmallCoordinate.ContainsKey(touchingNeighborBlockCoordinate))
                    {
                        _subBlockIdsBySmallCoordinate.Remove(touchingNeighborBlockCoordinate);
                        var subBlockObject = _subBlockGameObjectsBySmallCoordinate[touchingNeighborBlockCoordinate];
                        Destroy(subBlockObject);
                        _subBlockGameObjectsBySmallCoordinate.Remove(touchingNeighborBlockCoordinate);
                    }
                    
                    buildingTypeByGlobalSmallCoordinates.Add(touchingNeighborBlockCoordinate, neighborBuildingType);
                }
            }
            
            BuildNewShape(buildingTypeByGlobalSmallCoordinates);
        }

        private void BuildNewShape(Dictionary<Vector3Int, BuildingType> buildingTypeByGlobalSmallCoordinates)
        {
            var coordinatesToBuild = buildingTypeByGlobalSmallCoordinates.Keys.ToList();
            var possibleSubBlockIdsByCoordinates = new Dictionary<Vector3Int, List<int>>();
            foreach (var subBlockCoordinates in buildingTypeByGlobalSmallCoordinates.Keys)
            {
                possibleSubBlockIdsByCoordinates.Add(subBlockCoordinates, null);
            }
            
            foreach (var (globalCoordinate, buildingType) in buildingTypeByGlobalSmallCoordinates)
            {
                SetInitialPossibleIdsForSubBlockCoordinate(globalCoordinate, buildingType, buildingTypeByGlobalSmallCoordinates, possibleSubBlockIdsByCoordinates);
            }

            while (coordinatesToBuild.Count > 0)
            {
                var coordinateWithTheLeastPossibilities = new Vector3Int(MaxValue, MaxValue, MaxValue);
                var leastPossibilities = MaxValue;
            
                foreach (var coordinate in coordinatesToBuild)
                {
                    var possibilityCount = possibleSubBlockIdsByCoordinates[coordinate].Count;
                    if (possibilityCount < leastPossibilities)
                    {
                        coordinateWithTheLeastPossibilities = coordinate;
                        leastPossibilities = possibilityCount;
                    }
                }
                
                var possibleIds = possibleSubBlockIdsByCoordinates[coordinateWithTheLeastPossibilities];
                if (possibleIds.Count == 0)
                {
                    var buildingType = buildingTypeByGlobalSmallCoordinates[coordinateWithTheLeastPossibilities];
                    var blockType =
                        SubBlockUtils.GetTypeFromCoordinates(
                            GetLocalSmallCoordinateFromGlobal(coordinateWithTheLeastPossibilities));
                    throw new Exception(
                        $"When trying to place a {buildingType} {blockType} SubBlock at {coordinateWithTheLeastPossibilities} there was no possible option!");
                }
                
                var idToPlace = SelectRandomSubBlockId(possibleIds);
            
                var parentBlockCoordinate = SmallCoordinateToBigCoordinate(coordinateWithTheLeastPossibilities);
                if (!_blockObjectByBigCoordinate.TryGetValue(parentBlockCoordinate, out var parentBlockObject))
                {
                    var parentBlockPosition = BigCoordinateToWorldPosition(parentBlockCoordinate);
                    parentBlockObject = Instantiate(blockPrefab, parentBlockPosition, Quaternion.identity);
                    _blockObjectByBigCoordinate.Add(parentBlockCoordinate, parentBlockObject);
                }

                var selectedSubBlockRow = Config.SubBlockTable.Rows.Find(idToPlace);
                var selectedSubBlockType = (SubBlockType)selectedSubBlockRow[WfcConfig.TYPE_COLUMN_INDEX];
                
                var localSubBlockCoordinate = GetLocalSmallCoordinateFromGlobal(coordinateWithTheLeastPossibilities);
                var subBlock90RotationsAroundY = selectedSubBlockType.GetDefaultCoordinate()
                    .Get90RotationsAroundYTo(localSubBlockCoordinate);

                var selectedPrefab = SelectRandomPrefabVariant(selectedSubBlockRow);

                var newSubBlock = Instantiate(selectedPrefab, parentBlockObject.transform);
                newSubBlock.transform.localPosition = (Vector3)localSubBlockCoordinate * Config.subBlockSize;
                newSubBlock.transform.rotation = Quaternion.Euler(0, subBlock90RotationsAroundY * 90, 0);
                _subBlockGameObjectsBySmallCoordinate.Add(coordinateWithTheLeastPossibilities, newSubBlock);
                _subBlockIdsBySmallCoordinate.Add(coordinateWithTheLeastPossibilities, idToPlace);
            
                foreach (var direction in Vector3IntUtils.Directions)
                {
                    var neighborCoordinate = coordinateWithTheLeastPossibilities + direction;
                    if(!possibleSubBlockIdsByCoordinates.ContainsKey(neighborCoordinate))
                        continue;

                    var rotatedDirection = direction.Rotate90(Vector3Int.up, -subBlock90RotationsAroundY);
                    var allowedNeighborIdsInDirection =
                        (int[])selectedSubBlockRow[WfcConfig.NEIGHBOR_COLUMN_INDEX_BY_DIRECTION[rotatedDirection]];
                    var possibleIdsInDirection = possibleSubBlockIdsByCoordinates[neighborCoordinate];

                    possibleSubBlockIdsByCoordinates[neighborCoordinate] = possibleIdsInDirection
                        .Where(id => Array.Exists(allowedNeighborIdsInDirection, allowedId => id == allowedId)).ToList();
                }
            
                coordinatesToBuild.Remove(coordinateWithTheLeastPossibilities);
                possibleSubBlockIdsByCoordinates.Remove(coordinateWithTheLeastPossibilities);
            }
        }

        private int SelectRandomSubBlockId(List<int> subBlockIdsToSelectFrom)
        {
            var probabilityThresholdsById = new Dictionary<int, float>();
            foreach (var id in subBlockIdsToSelectFrom)
            {
                probabilityThresholdsById.Add(id, ((float[])Config.SubBlockTable.Rows.Find(id)[WfcConfig.PROBABILITIES_COLUMN_INDEX]).Sum());
            }
            var maxProbabilityValue = probabilityThresholdsById.Values.Sum();
            var randomSelectionValue = Random.Range(0, maxProbabilityValue);
            var currentSelectionThreshold = .0f;
            foreach (var (id, probabilityThreshold) in probabilityThresholdsById)
            {
                currentSelectionThreshold += probabilityThreshold;
                if (currentSelectionThreshold >= randomSelectionValue - float.Epsilon)
                    return id;
            }

            throw new Exception("Failed selecting a random SubBlockId.");
        }

        private static GameObject SelectRandomPrefabVariant(DataRow selectedSubBlockRow)
        {
            var selectedPrefabVariants = (GameObject[])selectedSubBlockRow[WfcConfig.PREFAB_COLUMN_INDEX];
            var selectedPrefabProbabilities = (float[])selectedSubBlockRow[WfcConfig.PROBABILITIES_COLUMN_INDEX];
            var maxProbabilityValue = selectedPrefabProbabilities.Sum();
            var randomVariantSelectionValue = Random.Range(0, maxProbabilityValue);
            
            var selectionThreshold = .0f;
            for (var i = 0; i < selectedPrefabProbabilities.Length; i++)
            {
                selectionThreshold += selectedPrefabProbabilities[i];
                if (selectionThreshold >= randomVariantSelectionValue - float.Epsilon)
                {
                    return selectedPrefabVariants[i];
                }
            }
            
            throw new Exception(
                    $"Failed selecting random prefab variant for {selectedSubBlockRow[WfcConfig.BUILDING_COLUMN_INDEX]} {selectedSubBlockRow[WfcConfig.TYPE_COLUMN_INDEX]}-SubBlock.");
        }

        private void SetInitialPossibleIdsForSubBlockCoordinate(Vector3Int globalCoordinate, BuildingType buildingType, Dictionary<Vector3Int, BuildingType> plannedBuildingTypeByCoordinate, Dictionary<Vector3Int, List<int>> possibleIdsByCoordinate)
        {
            var localCoordinate = GetLocalSmallCoordinateFromGlobal(globalCoordinate);
            var subBlockType = SubBlockUtils.GetTypeFromCoordinates(localCoordinate);
            var subBlock90RotationsAroundY =
                subBlockType.GetDefaultCoordinate().Get90RotationsAroundYTo(localCoordinate);
            var possibleSubBlockRows = Config.SubBlockTable.AsEnumerable().Where(row =>
                (BuildingType)row[WfcConfig.BUILDING_COLUMN_INDEX] == buildingType &&
                (SubBlockType)row[WfcConfig.TYPE_COLUMN_INDEX] == subBlockType);
            
            foreach (var direction in Vector3IntUtils.Directions)
            {
                var neighborCoordinate = globalCoordinate + direction;
                
                if (_subBlockIdsBySmallCoordinate.TryGetValue(neighborCoordinate, out var neighborId))
                {
                    var oppositeDirection = -direction;
                    var neighborRow = Config.SubBlockTable.Rows.Find(neighborId);
                    var neighborType = (SubBlockType)neighborRow[WfcConfig.TYPE_COLUMN_INDEX];
                    var localNeighborCoordinates = GetLocalSmallCoordinateFromGlobal(neighborCoordinate);
                    var neighbor90RotationsAroundY =
                        neighborType.GetDefaultCoordinate().Get90RotationsAroundYTo(localNeighborCoordinates);

                    var rotatedOppositeDirection =
                        oppositeDirection.Rotate90(Vector3Int.up, -neighbor90RotationsAroundY);
                    var allowedIdsByNeighbor =
                        (int[])neighborRow[WfcConfig.NEIGHBOR_COLUMN_INDEX_BY_DIRECTION[rotatedOppositeDirection]];

                    possibleSubBlockRows = possibleSubBlockRows.Where(row => Array.Exists(allowedIdsByNeighbor,
                        allowedId => allowedId == (int)row[WfcConfig.ID_COLUMN_INDEX]));
                }
                else if (plannedBuildingTypeByCoordinate.TryGetValue(neighborCoordinate, out var plannedNeighborBuildingType))
                {
                    var rotatedDirection = direction.Rotate90(Vector3Int.up, -subBlock90RotationsAroundY);
                    possibleSubBlockRows = possibleSubBlockRows.Where(row =>
                        Array.Exists((int[])row[WfcConfig.NEIGHBOR_COLUMN_INDEX_BY_DIRECTION[rotatedDirection]],
                            allowedId =>
                            {
                                if (allowedId == WfcConfig.EMPTY_SUB_BLOCK_ID)
                                    return false;
                                
                                var allowedRow = Config.SubBlockTable.Rows.Find(allowedId);
                                var allowedBuildingType = (BuildingType)allowedRow[WfcConfig.BUILDING_COLUMN_INDEX];
                                return allowedBuildingType == plannedNeighborBuildingType;
                            }));
                }
                else
                {
                    var rotatedDirection = direction.Rotate90(Vector3Int.up, -subBlock90RotationsAroundY);
                    possibleSubBlockRows = possibleSubBlockRows.Where(row =>
                        Array.Exists((int[])row[WfcConfig.NEIGHBOR_COLUMN_INDEX_BY_DIRECTION[rotatedDirection]],
                            allowedId => allowedId == WfcConfig.EMPTY_SUB_BLOCK_ID));
                }
            }

            possibleIdsByCoordinate[globalCoordinate] =
                possibleSubBlockRows.Select(row => (int)row[WfcConfig.ID_COLUMN_INDEX]).ToList();
        }

        private Vector3Int WorldPositionToBigCoordinate(Vector3 worldPosition)
        {
            var dividedPosition = worldPosition / (Config.subBlockSize * 3f);
            return new Vector3Int(
                Mathf.RoundToInt(dividedPosition.x), 
                Mathf.RoundToInt(dividedPosition.y), 
                Mathf.RoundToInt(dividedPosition.z));
        }

        private Vector3 BigCoordinateToWorldPosition(Vector3Int bigCoordinate)
        {
            return (Vector3)bigCoordinate * (Config.subBlockSize * 3f);
        }

        private static Vector3Int SmallCoordinateToBigCoordinate(Vector3Int smallCoordinate)
        {
            return new Vector3Int(
                (smallCoordinate.x + (int)Mathf.Sign(smallCoordinate.x)) / 3,
                (smallCoordinate.y + (int)Mathf.Sign(smallCoordinate.y)) / 3,
                (smallCoordinate.z + (int)Mathf.Sign(smallCoordinate.z)) / 3
            );
        }

        private static Vector3Int GetGlobalSmallCoordinateFromLocal(Vector3Int localCoordinate,
            Vector3Int bigCoordinate)
        {
            return bigCoordinate * 3 + localCoordinate;
        }

        private static Vector3Int GetLocalSmallCoordinateFromGlobal(Vector3Int globalCoordinate)
        {
            var bigGlobalCoordinates = SmallCoordinateToBigCoordinate(globalCoordinate);
            return new Vector3Int(
                globalCoordinate.x - bigGlobalCoordinates.x * 3,
                globalCoordinate.y - bigGlobalCoordinates.y * 3,
                globalCoordinate.z - bigGlobalCoordinates.z * 3
            );
        }
    }
}