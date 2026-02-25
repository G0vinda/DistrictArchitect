using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;

namespace WFC
{
    [CreateAssetMenu(fileName = "WFCConfig", menuName = "WFCConfig", order = 0)]
    public class WfcConfig : ScriptableObject, ISerializationCallbackReceiver
    {
        public DataTable SubBlockTable { get; private set; } = new();
        
        public Action SubBlockTableUpdated;

        [SerializeField] private int[] serializedIds;
        [SerializeField] private BuildingType[] serializedBuildingTypes;
        [SerializeField] private SubBlockType[] serializedTypes;
        [SerializeField] private GameObject[] serializedPrefabs;
        [SerializeField] private float[] serializedProbabilities;
        [SerializeField] private SerializedAllowedNeighborArrays[] serializedNeighborArrays;
        
        private bool _subBlockTableIsDirty = true;

        public const int EMPTY_SUB_BLOCK_ID = -1;
        
        public const int ID_COLUMN_INDEX = 0;
        public const int BUILDING_COLUMN_INDEX = 1;
        public const int TYPE_COLUMN_INDEX = 2;
        public const int PREFAB_COLUMN_INDEX = 3;
        public const int PROBABILITY_COLUMN_INDEX = 4;

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

        public int GetSubBlockCount() => SubBlockTable != null ? SubBlockTable.Rows.Count : 0;

        public void AddSubBlock(BuildingType buildingType, SubBlockType newSubBlockType, GameObject prefab, float probability)
        {
            var newSubBlockRow = SubBlockTable.NewRow();
            newSubBlockRow[BUILDING_COLUMN_INDEX] = buildingType;
            newSubBlockRow[TYPE_COLUMN_INDEX] = newSubBlockType;
            newSubBlockRow[PREFAB_COLUMN_INDEX] = prefab;
            newSubBlockRow[PROBABILITY_COLUMN_INDEX] = probability;
            SubBlockTable.Rows.Add(newSubBlockRow);
            var newSubBlockId = (int)newSubBlockRow[ID_COLUMN_INDEX]; 
                
            var enumerableSubBlockRows = SubBlockTable.AsEnumerable();
            var allDirections = Vector3Int.zero.Neighbours();
            foreach (var direction in allDirections)
            {
                newSubBlockRow[NEIGHBOR_COLUMN_INDEX_BY_DIRECTION[direction]] = new[] { EMPTY_SUB_BLOCK_ID };
                // var neighborTypeInDirection = newSubBlockType.GetNeighborTypeInDirection(direction);
                // var subBlocksWithMatchingType = enumerableSubBlockRows
                //     .Where(row => (SubBlockType)row[TYPE_COLUMN_INDEX] == neighborTypeInDirection)
                //     .Select(row => (int)row[ID_COLUMN_INDEX]).ToArray();
                // newSubBlockRow[NEIGHBOR_COLUMN_INDEX_BY_DIRECTION[direction]] = subBlocksWithMatchingType;
                //
                // var oppositeDirection = direction * -1;
                // if (neighborTypeInDirection.GetNeighborTypeInDirection(oppositeDirection) != newSubBlockType)
                //     continue;
                //
                // foreach (var neighborId in subBlocksWithMatchingType)
                // {
                //     if (neighborId == newSubBlockId)
                //         continue;
                //     
                //     var neighborRow = SubBlockTable.Rows.Find(neighborId);
                //
                //     var neighborsAllowedNeighbors =
                //         (int[])neighborRow[NEIGHBOR_COLUMN_INDEX_BY_DIRECTION[oppositeDirection]];
                //     var extendedAllowedNeighbors = new int[neighborsAllowedNeighbors.Length + 1];
                //     Array.Copy(neighborsAllowedNeighbors, extendedAllowedNeighbors, neighborsAllowedNeighbors.Length);
                //     
                //     extendedAllowedNeighbors[^1] = newSubBlockId;
                //     neighborRow[NEIGHBOR_COLUMN_INDEX_BY_DIRECTION[oppositeDirection]] = extendedAllowedNeighbors;
                // }
            }
            
            MarkTableAsDirty();
        }

        public void RemoveSubBlock(int id)
        {
            var rowToRemove = SubBlockTable.Rows.Find(id);
            var typeOfSubBlockToRemove = (SubBlockType)rowToRemove[TYPE_COLUMN_INDEX];
            var allDirections = Vector3Int.zero.Neighbours();
            foreach (var direction in allDirections)
            {
                var neighborTypeInDirection = typeOfSubBlockToRemove.GetNeighborTypeInDirection(direction);
                if (neighborTypeInDirection.GetNeighborTypeInDirection(-direction) != typeOfSubBlockToRemove)
                    continue;
                
                var allowedNeighborsInDirectionRowValue = rowToRemove[NEIGHBOR_COLUMN_INDEX_BY_DIRECTION[direction]];
                if (allowedNeighborsInDirectionRowValue == null)
                    continue;
                
                var allowedNeighborsInDirection = (int[])allowedNeighborsInDirectionRowValue;
                foreach (var allowedNeighborId in allowedNeighborsInDirection)
                {
                    RemoveAllowedNeighborUnidirectional(id, allowedNeighborId, -direction);
                }
            }
            SubBlockTable.Rows.Remove(rowToRemove);
            
            MarkTableAsDirty();
        }

        public void RemoveAllowedNeighbor(int idToRemove, int idToRemoveFrom, Vector3Int directionToRemoveFrom)
        {
            RemoveAllowedNeighborUnidirectional(idToRemove, idToRemoveFrom, directionToRemoveFrom);
            
            if (idToRemove != idToRemoveFrom && idToRemove >= 0)
            {
                var oppositeDirection = -directionToRemoveFrom;
                var typeToRemove = (SubBlockType)SubBlockTable.Rows.Find(idToRemove)[TYPE_COLUMN_INDEX];
                var typeToRemoveFrom = (SubBlockType)SubBlockTable.Rows.Find(idToRemoveFrom)[TYPE_COLUMN_INDEX];
                if (typeToRemove.GetNeighborTypeInDirection(oppositeDirection) == typeToRemoveFrom)
                {
                    RemoveAllowedNeighborUnidirectional(idToRemoveFrom, idToRemove, oppositeDirection);
                }
            }
            
            MarkTableAsDirty(false);
        }

        public void AddAllowedNeighbor(int idToAdd, int idToAddTo, Vector3Int directionToAddTo)
        {
            AddAllowedNeighborUnidirectional(idToAdd, idToAddTo, directionToAddTo);
            if (idToAdd != idToAddTo && idToAdd >= 0)
            {
                var oppositeDirection = -directionToAddTo;
                var typeToAdd = (SubBlockType)SubBlockTable.Rows.Find(idToAdd)[TYPE_COLUMN_INDEX];
                var typeToAddTo = (SubBlockType)SubBlockTable.Rows.Find(idToAddTo)[TYPE_COLUMN_INDEX];
                if (typeToAdd.GetNeighborTypeInDirection(oppositeDirection) == typeToAddTo)
                {
                    AddAllowedNeighborUnidirectional(idToAddTo, idToAdd, oppositeDirection);
                }
            }
            
            MarkTableAsDirty(false);
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

        public GameObject GetSubBlockPrefabForType(SubBlockType type)
        {
            return (GameObject)SubBlockTable.AsEnumerable().First(row => (SubBlockType)row[TYPE_COLUMN_INDEX] == type)[PREFAB_COLUMN_INDEX];
        }

        public bool DoesPrefabExistInDatabase(GameObject prefab)
        {
            return SubBlockTable.AsEnumerable().Any(row => (GameObject)row[PREFAB_COLUMN_INDEX] == prefab);
        }
        
        [ContextMenu("Print All SubBlocks")]
        private void PrintAllSubBlocks()
        {
            foreach (DataRow row in SubBlockTable.Rows)
            {
                Debug.Log($"{row[ID_COLUMN_INDEX]}, {(SubBlockType)row[TYPE_COLUMN_INDEX]}: {((GameObject)row[PREFAB_COLUMN_INDEX]).name}");
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
            
            var prefabColumn = AddNewColumnToSubBlockTable("Prefab", typeof(GameObject), PREFAB_COLUMN_INDEX);
            prefabColumn.ReadOnly = true;
            
            AddNewColumnToSubBlockTable("Probability", typeof(float), PROBABILITY_COLUMN_INDEX);
            
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
            serializedPrefabs = new GameObject[rowCount];
            serializedProbabilities = new float[rowCount];
            serializedNeighborArrays = new SerializedAllowedNeighborArrays[rowCount];
            
            for (var i = 0; i < SubBlockTable.Rows.Count; i++)
            {
                serializedIds[i] = (int)SubBlockTable.Rows[i][ID_COLUMN_INDEX];
                serializedBuildingTypes[i] = (BuildingType)SubBlockTable.Rows[i][BUILDING_COLUMN_INDEX];
                serializedTypes[i] = (SubBlockType)SubBlockTable.Rows[i][TYPE_COLUMN_INDEX];
                serializedPrefabs[i] = (GameObject)SubBlockTable.Rows[i][PREFAB_COLUMN_INDEX];
                serializedProbabilities[i] = (float)SubBlockTable.Rows[i][PROBABILITY_COLUMN_INDEX];
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
                row[PREFAB_COLUMN_INDEX] = serializedPrefabs[i];
                row[PROBABILITY_COLUMN_INDEX] = serializedProbabilities[i];
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


