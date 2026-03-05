using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WFC
{
    [CreateAssetMenu(fileName = "WFCConfig", menuName = "WFCConfig", order = 0)]
    public class WfcConfig : ScriptableObject, ISerializationCallbackReceiver
    {
        public DataTable SubBlockTable { get; private set; } = new();
        
        public int SubBlockCount => SubBlockTable != null ? SubBlockTable.Rows.Count : 0;
        
        public Action SubBlockTableUpdated;

        [SerializeField] private int[] serializedIds;
        [SerializeField] private BuildingType[] serializedBuildingTypes;
        [SerializeField] private SubBlockType[] serializedTypes;
        [SerializeField] private SerializedPrefabArray[] serializedPrefabs;
        [SerializeField] private SerializedProbabilityArray[] serializedProbabilities;
        [SerializeField] private SerializedAllowedNeighborArrays[] serializedNeighborArrays;
        
        private bool _subBlockTableIsDirty = true;

        public const int EMPTY_SUB_BLOCK_ID = -1;
        
        public const int ID_COLUMN_INDEX = 0;
        public const int BUILDING_COLUMN_INDEX = 1;
        public const int TYPE_COLUMN_INDEX = 2;
        public const int PREFAB_COLUMN_INDEX = 3;
        public const int PROBABILITIES_COLUMN_INDEX = 4;

        public static readonly Dictionary<Vector3Int, int> NEIGHBOR_COLUMN_INDEX_BY_DIRECTION = new()
        {
            { Vector3Int.forward, 5 },
            { Vector3Int.back, 6 },
            { Vector3Int.right, 7 },
            { Vector3Int.left, 8 },
            { Vector3Int.up, 9 },
            { Vector3Int.down, 10 },
        };
        
        private void OnEnable()
        {
            if (SubBlockTable.Columns.Count == 0)
                SetupSubBlockTable();
        }

        public void AddSubBlock(BuildingType newSubBlockBuildingType, SubBlockType newSubBlockType, GameObject prefab, float probability)
        {
            var newSubBlockRow = SubBlockTable.NewRow();
            newSubBlockRow[BUILDING_COLUMN_INDEX] = newSubBlockBuildingType;
            newSubBlockRow[TYPE_COLUMN_INDEX] = newSubBlockType;
            newSubBlockRow[PREFAB_COLUMN_INDEX] = new[] { prefab };
            newSubBlockRow[PROBABILITIES_COLUMN_INDEX] = new[] { probability };
            SubBlockTable.Rows.Add(newSubBlockRow);
            var newSubBlockId = (int)newSubBlockRow[ID_COLUMN_INDEX]; 
                
            var enumerableSubBlockRows = SubBlockTable.AsEnumerable();
            var outwardFacingDirections = newSubBlockType.GetOutwardFacingDirections();
            foreach (var direction in Vector3IntUtils.Directions)
            {
                var neighborTypeInDirection = newSubBlockType.GetNeighborTypeInDirection(direction);
                int[] matchingNeighborSubBlockIds;
                if (outwardFacingDirections.Contains(direction))
                {
                    var matchingNeighborSubBlockIdsList = enumerableSubBlockRows
                        .Where(row => (SubBlockType)row[TYPE_COLUMN_INDEX] == neighborTypeInDirection)
                        .Select(row => (int)row[ID_COLUMN_INDEX]).ToList();
                    matchingNeighborSubBlockIdsList.Add(EMPTY_SUB_BLOCK_ID);
                    matchingNeighborSubBlockIds = matchingNeighborSubBlockIdsList.ToArray();
                }
                else
                {
                    matchingNeighborSubBlockIds = enumerableSubBlockRows
                        .Where(row => (SubBlockType)row[TYPE_COLUMN_INDEX] == neighborTypeInDirection && (BuildingType)row[BUILDING_COLUMN_INDEX] == newSubBlockBuildingType )
                        .Select(row => (int)row[ID_COLUMN_INDEX]).ToArray();
                }
                
                newSubBlockRow[NEIGHBOR_COLUMN_INDEX_BY_DIRECTION[direction]] = matchingNeighborSubBlockIds;
            }
            
            AdjustExistingNeighborsToNewSubBlock(newSubBlockBuildingType, newSubBlockType, newSubBlockId);
            
            MarkTableAsDirty();
        }

        public void RemoveSubBlock(int idToRemove)
        {
            var rowToRemove = SubBlockTable.Rows.Find(idToRemove);
            var enumerableSubBlockRows = SubBlockTable.AsEnumerable();
            var typeOfSubBlockToRemove = (SubBlockType)rowToRemove[TYPE_COLUMN_INDEX];
            foreach (SubBlockType subBlockType in Enum.GetValues(typeof(SubBlockType)))
            {
                EnumerableRowCollection<DataRow> rowsWithSubBlockType = null;
                foreach (var direction in Vector3IntUtils.Directions)
                {
                    if (subBlockType.GetNeighborTypeInDirection(direction) != typeOfSubBlockToRemove)
                        continue;
                    
                    rowsWithSubBlockType ??=
                        enumerableSubBlockRows.Where(row => (SubBlockType)row[TYPE_COLUMN_INDEX] == subBlockType);
                    
                    foreach (var neighborRow in rowsWithSubBlockType)
                    {
                        var allowedNeighbors = ((int[])neighborRow[NEIGHBOR_COLUMN_INDEX_BY_DIRECTION[direction]]).ToList();
                        allowedNeighbors.Remove(idToRemove);
                        neighborRow[NEIGHBOR_COLUMN_INDEX_BY_DIRECTION[direction]] = allowedNeighbors.ToArray();
                    }
                }
            }
            SubBlockTable.Rows.Remove(rowToRemove);
            
            MarkTableAsDirty();
        }

        public void AddSubBlockVariant(int idToAddTo, GameObject variantPrefab, float probability)
        {
            var subBlockToAddTo = SubBlockTable.Rows.Find(idToAddTo);
                
            var prefabList = ((GameObject[])subBlockToAddTo[PREFAB_COLUMN_INDEX]).ToList();
            prefabList.Add(variantPrefab);
            subBlockToAddTo[PREFAB_COLUMN_INDEX] = prefabList.ToArray();

            var probabilityList = ((float[])subBlockToAddTo[PROBABILITIES_COLUMN_INDEX]).ToList();
            probabilityList.Add(probability);
            subBlockToAddTo[PROBABILITIES_COLUMN_INDEX] = probabilityList.ToArray();

            MarkTableAsDirty();
        }

        public void RemoveSubBlockVariant(int idToRemoveFrom, int variantIndexToRemove)
        {
            var subBlockToRemoveFrom = SubBlockTable.Rows.Find(idToRemoveFrom);
            
            var prefabList = ((GameObject[])subBlockToRemoveFrom[PREFAB_COLUMN_INDEX]).ToList();
            prefabList.RemoveAt(variantIndexToRemove);
            subBlockToRemoveFrom[PREFAB_COLUMN_INDEX] = prefabList.ToArray();

            var probabilityList = ((float[])subBlockToRemoveFrom[PROBABILITIES_COLUMN_INDEX]).ToList();
            probabilityList.RemoveAt(variantIndexToRemove);
            subBlockToRemoveFrom[PROBABILITIES_COLUMN_INDEX] = probabilityList.ToArray();
            
            MarkTableAsDirty();
        }

        public void ChangeSubBlockVariantProbability(int subBlockIdToChange, int variantIndexToChange,
            float newProbability)
        {
            var subBlockToChange = SubBlockTable.Rows.Find(subBlockIdToChange);
            ((float[])subBlockToChange[PROBABILITIES_COLUMN_INDEX])[variantIndexToChange] = newProbability;
            
            MarkTableAsDirty(false);
        }
        
        public void AddAllowedNeighbor(int idToAdd, int idToAddTo, Vector3Int directionToAddTo)
        {
            AddAllowedNeighborUnidirectional(idToAdd, idToAddTo, directionToAddTo);
            if (idToAdd != idToAddTo && idToAdd >= 0)
            {
                var typeToAdd = (SubBlockType)SubBlockTable.Rows.Find(idToAdd)[TYPE_COLUMN_INDEX];
                var typeToAddTo = (SubBlockType)SubBlockTable.Rows.Find(idToAddTo)[TYPE_COLUMN_INDEX];
                
                var allowedNeighborCoordinate = (typeToAddTo.GetDefaultCoordinate() + directionToAddTo).GetWrappedNeg1To1();
                var allowedNeighbor90Rotations =
                    typeToAdd.GetDefaultCoordinate().Get90RotationsAroundYTo(allowedNeighborCoordinate);
                var allowedNeighborDirectionToOrigin = -directionToAddTo.Rotate90(Vector3Int.up, -allowedNeighbor90Rotations);
                
                AddAllowedNeighborUnidirectional(idToAddTo, idToAdd, allowedNeighborDirectionToOrigin);
            }
            
            MarkTableAsDirty(false);
        }

        public void RemoveAllowedNeighbor(int idToRemove, int idToRemoveFrom, Vector3Int directionToRemoveFrom)
        {
            RemoveAllowedNeighborUnidirectional(idToRemove, idToRemoveFrom, directionToRemoveFrom);
            
            if (idToRemove != idToRemoveFrom && idToRemove >= 0)
            {
                var typeToRemove = (SubBlockType)SubBlockTable.Rows.Find(idToRemove)[TYPE_COLUMN_INDEX];
                var typeToRemoveFrom = (SubBlockType)SubBlockTable.Rows.Find(idToRemoveFrom)[TYPE_COLUMN_INDEX];
                
                var allowedNeighborCoordinate = (typeToRemoveFrom.GetDefaultCoordinate() + directionToRemoveFrom).GetWrappedNeg1To1();
                var allowedNeighbor90Rotations =
                    typeToRemove.GetDefaultCoordinate().Get90RotationsAroundYTo(allowedNeighborCoordinate);
                var allowedNeighborDirectionToOrigin = -directionToRemoveFrom.Rotate90(Vector3Int.up, -allowedNeighbor90Rotations);
                
                RemoveAllowedNeighborUnidirectional(idToRemoveFrom, idToRemove, allowedNeighborDirectionToOrigin);
            }
            
            MarkTableAsDirty(false);
        }
        
        public bool DoesPrefabExistInDatabase(GameObject prefab)
        {
            return SubBlockTable.AsEnumerable().Any(row => Array.Exists((GameObject[])row[PREFAB_COLUMN_INDEX],p => p == prefab));
        }
        
        private void AddAllowedNeighborUnidirectional(int idToAdd, int idToAddTo, Vector3Int directionToAddTo)
        {
            var rowToAddTo = SubBlockTable.Rows.Find(idToAddTo);
            var neighborColumnId = NEIGHBOR_COLUMN_INDEX_BY_DIRECTION[directionToAddTo];
            var originalNeighborsRowValue = rowToAddTo[neighborColumnId]; 
            var originalNeighbors = originalNeighborsRowValue != null ? ((int[])originalNeighborsRowValue).ToList() : new List<int>();
            var newNeighbors = originalNeighbors.ToList();
            newNeighbors.Add(idToAdd);
            rowToAddTo[neighborColumnId] = newNeighbors.ToArray();
        }

        private void RemoveAllowedNeighborUnidirectional(int idToRemove, int idToRemoveFrom,
            Vector3Int directionToRemoveFrom)
        {
            var rowToRemoveFrom = SubBlockTable.Rows.Find(idToRemoveFrom);
            var neighborColumnId = NEIGHBOR_COLUMN_INDEX_BY_DIRECTION[directionToRemoveFrom];
            var originalNeighbors = (int[])rowToRemoveFrom[neighborColumnId];
            var newNeighbors = new List<int>();
            foreach (var neighborId in originalNeighbors)
            {
                if (neighborId == idToRemove)
                    continue;
                
                newNeighbors.Add(neighborId);
            }
            
            rowToRemoveFrom[neighborColumnId] = newNeighbors.ToArray();
            MarkTableAsDirty(false);
        }
        
        private void AdjustExistingNeighborsToNewSubBlock(BuildingType newSubBlockBuildingType, SubBlockType newSubBlockType,
            int newSubBlockId)
        {
            var enumerableSubBlockRows = SubBlockTable.AsEnumerable();
            foreach (SubBlockType subBlockType in Enum.GetValues(typeof(SubBlockType)))
            {
                List<DataRow> matchingTypeRows = null;
                var neighborOutwardFacingDirections = subBlockType.GetOutwardFacingDirections();
                foreach (var direction in Vector3IntUtils.Directions)
                {
                    if (subBlockType.GetNeighborTypeInDirection(direction) != newSubBlockType)
                        continue;

                    var directionIsFacingOutwards = neighborOutwardFacingDirections.Contains(direction);

                    matchingTypeRows ??=
                        enumerableSubBlockRows.Where(row => (SubBlockType)row[TYPE_COLUMN_INDEX] == subBlockType).ToList();
                    
                    foreach (var matchingTypeRow in matchingTypeRows)
                    {
                        if (!directionIsFacingOutwards && (BuildingType)matchingTypeRow[BUILDING_COLUMN_INDEX] != newSubBlockBuildingType)
                            continue;
                        
                        AddAllowedNeighborUnidirectional(newSubBlockId, (int)matchingTypeRow[ID_COLUMN_INDEX], direction);
                    }
                }   
            }
        }
        
        private void SetupSubBlockTable()
        {
            SubBlockTable = new DataTable();
            var idColumn = AddNewColumnToSubBlockTable("Id", typeof(int), ID_COLUMN_INDEX);
            idColumn.ReadOnly = true;
            idColumn.Unique = true;
            idColumn.AutoIncrement = true;
            SubBlockTable.PrimaryKey = new[] { idColumn };
            
            var buildingColumn = AddNewColumnToSubBlockTable("BuildingType", typeof(BuildingType), BUILDING_COLUMN_INDEX);
            buildingColumn.ReadOnly = true;

            var typeColumn = AddNewColumnToSubBlockTable("Type", typeof(SubBlockType), TYPE_COLUMN_INDEX);
            typeColumn.ReadOnly = true;
            
            AddNewColumnToSubBlockTable("Prefabs", typeof(GameObject[]), PREFAB_COLUMN_INDEX);
            AddNewColumnToSubBlockTable("Probabilities", typeof(float[]), PROBABILITIES_COLUMN_INDEX);
            
            foreach (var (direction, allowedNeighborColumnIndex) in NEIGHBOR_COLUMN_INDEX_BY_DIRECTION)
            {
                AddNewColumnToSubBlockTable($"Allowed Neighbors {direction}", typeof(int[]),
                    allowedNeighborColumnIndex);
            }
        }

        private DataColumn AddNewColumnToSubBlockTable(string columnName, Type dataType, int ordinal)
        {
            var newColumn = new DataColumn();
            newColumn.ColumnName = columnName;
            newColumn.DataType = dataType;
            SubBlockTable.Columns.Add(newColumn);
            newColumn.SetOrdinal(ordinal);
            return newColumn;
        }
        
        public void OnBeforeSerialize()
        {
            if (SubBlockTable.Columns.Count == 0 || !_subBlockTableIsDirty)
                return;
                
            var rowCount = SubBlockTable.Rows.Count;
            serializedIds = new int[rowCount];
            serializedBuildingTypes = new BuildingType[rowCount];
            serializedTypes = new SubBlockType[rowCount];
            serializedPrefabs = new SerializedPrefabArray[rowCount];
            serializedProbabilities = new SerializedProbabilityArray[rowCount];
            serializedNeighborArrays = new SerializedAllowedNeighborArrays[rowCount];
            
            for (var i = 0; i < SubBlockTable.Rows.Count; i++)
            {
                serializedIds[i] = (int)SubBlockTable.Rows[i][ID_COLUMN_INDEX];
                serializedBuildingTypes[i] = (BuildingType)SubBlockTable.Rows[i][BUILDING_COLUMN_INDEX];
                serializedTypes[i] = (SubBlockType)SubBlockTable.Rows[i][TYPE_COLUMN_INDEX];
                serializedPrefabs[i] = new SerializedPrefabArray((GameObject[])SubBlockTable.Rows[i][PREFAB_COLUMN_INDEX]);
                serializedProbabilities[i] = new SerializedProbabilityArray((float[])SubBlockTable.Rows[i][PROBABILITIES_COLUMN_INDEX]);
                serializedNeighborArrays[i] = new SerializedAllowedNeighborArrays();
                foreach (var (direction, columnIndex) in NEIGHBOR_COLUMN_INDEX_BY_DIRECTION)
                {
                    serializedNeighborArrays[i][direction] = (int[])SubBlockTable.Rows[i][columnIndex];
                }
            }

            _subBlockTableIsDirty = false;
        }

        public void OnAfterDeserialize()
        {
            if (SubBlockTable.Columns.Count == 0)
                SetupSubBlockTable();
            
            if (AreAnySerializedArraysNull())
            {
                Debug.Log("On Deserialization at least one of the serialized arrays was null.");
                return;
            }
            
            if (!AreAllSerializedArraysTheSameLength())
            {
                Debug.LogError("On Deserialization the serialized arrays did not have the same length.");
                return;
            }
            
            var rowCount = serializedIds.Length;
            for (var i = 0; i < rowCount; i++)
            {
                var row = SubBlockTable!.NewRow();
                row[ID_COLUMN_INDEX] = serializedIds[i];
                row[BUILDING_COLUMN_INDEX] = serializedBuildingTypes[i];
                row[TYPE_COLUMN_INDEX] = serializedTypes[i];
                row[PREFAB_COLUMN_INDEX] = serializedPrefabs[i].value;
                row[PROBABILITIES_COLUMN_INDEX] = serializedProbabilities[i].value;
                foreach (var (direction, columnIndex) in NEIGHBOR_COLUMN_INDEX_BY_DIRECTION)
                {
                    row[columnIndex] = serializedNeighborArrays[i][direction];
                }
                SubBlockTable.Rows.Add(row);
            }
        }
        
        private void MarkTableAsDirty(bool forceUIRefresh = true)
        {
            _subBlockTableIsDirty = true;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            
            if (forceUIRefresh)
                SubBlockTableUpdated?.Invoke();
        }
        
        private bool AreAnySerializedArraysNull()
        {
            return serializedIds == null ||
                   serializedBuildingTypes == null ||
                   serializedTypes == null ||
                   serializedPrefabs == null ||
                   serializedProbabilities == null ||
                   serializedNeighborArrays == null;
        }

        private bool AreAllSerializedArraysTheSameLength()
        {
            return serializedIds.Length == serializedBuildingTypes.Length &&
                   serializedIds.Length == serializedTypes.Length &&
                   serializedIds.Length == serializedPrefabs.Length &&
                   serializedIds.Length == serializedProbabilities.Length &&
                   serializedIds.Length == serializedNeighborArrays.Length;
        }

        [Serializable]
        private class SerializedPrefabArray
        {
            public GameObject[] value;

            public SerializedPrefabArray(GameObject[] prefabs)
            {
                value = prefabs;
            }
        }

        [Serializable]
        private class SerializedProbabilityArray
        {
            public float[] value;

            public SerializedProbabilityArray(float[] probabilities)
            {
                value = probabilities;
            }
        }

        [Serializable]
        private class SerializedAllowedNeighborArrays
        {
            public int[] positiveZ;
            public int[] negativeZ;
            public int[] positiveX;
            public int[] negativeX;
            public int[] positiveY;
            public int[] negativeY;

            public int[] this[Vector3Int direction]
            {
                get
                {
                    return direction switch
                    {
                        { x: 0, y: 0, z: 1 } => positiveZ,
                        { x: 0, y: 0, z: -1 } => negativeZ,
                        { x: 1, y: 0, z: 0 } => positiveX,
                        { x: -1, y: 0, z: 0 } => negativeX,
                        { x: 0, y: 1, z: 0 } => positiveY,
                        { x: 0, y: -1, z: 0 } => negativeY,
                        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
                    };
                }

                set
                {
                    switch (direction)
                    {
                        case { x: 0, y: 0, z: 1 }:
                            positiveZ = value;
                            break;
                        case { x: 0, y: 0, z: -1 }:
                            negativeZ = value;
                            break;
                        case { x: 1, y: 0, z: 0 }:
                            positiveX = value;
                            break;
                        case { x: -1, y: 0, z: 0 }:
                            negativeX = value;
                            break;
                        case { x: 0, y: 1, z: 0 }:
                            positiveY = value;
                            break;
                        case { x: 0, y: -1, z: 0 }:
                            negativeY = value;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                    }
                }
            }
        }
    }
}


