using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ShapeManager : MonoBehaviour
{
    private List<List<Vector3Int>> _shapes = new();

    [SerializeField] private Building[] buildings;
    [SerializeField] private Building[] fillerBuildings;
    [SerializeField] private float[] fillerBuildingQuota;
    
    private void Awake()
    {
        var iShape = new List<Vector3Int>() { new(0, 0, 0), new(1, 0, 0), new(2, 0, 0), new(3, 0, 0) };
        var oShape = new List<Vector3Int>() { new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(1, 1, 0) };
        var lShape = new List<Vector3Int>() { new(0, 0, 0), new(1, 0, 0), new(2, 0, 0), new(0, 1, 0) };
        var tShape = new List<Vector3Int>() { new(0, 0, 0), new(1, 0, 0), new(-1, 0, 0), new(0, 1, 0) };
        var nShape = new List<Vector3Int>() { new(0, 0, 0), new(1, 0, 0), new(2, 1, 0), new(1, 1, 0) };
        var towerRightShape = new List<Vector3Int>() { new(0, 0, 0), new(1, 0, 0), new(1, 0, 1), new(1, 1, 1) };
        var towerLeftShape = new List<Vector3Int>() { new(0, 0, 0), new(1, 0, 0), new(0, 0, 1), new(0, 1, 1) };
        var tripodShape = new List<Vector3Int>() { new(0, 0, 0), new(0, 1, 0), new(0, 0, 1), new(1, 0, 0) };
        _shapes.Add(iShape);
        _shapes.Add(oShape);
        _shapes.Add(lShape);
        _shapes.Add(tShape);
        _shapes.Add(nShape);
        _shapes.Add(towerRightShape);
        _shapes.Add(towerLeftShape);
        _shapes.Add(tripodShape);
    }

    public Dictionary<Vector3Int, Building> GetRandomShapeDefinition()
    {
        var buildingList = buildings.ToList();
        var building1 = buildingList[Random.Range(0, buildingList.Count)];
        buildingList.Remove(building1);
        var building2 = buildingList[Random.Range(0, buildingList.Count)];
        var cellCoordinates = new List<Vector3Int>(_shapes[Random.Range(0, _shapes.Count)]);
        
        var buildingByCoordinates = new Dictionary<Vector3Int, Building>();
        var coords = cellCoordinates[Random.Range(0, cellCoordinates.Count)];
        cellCoordinates.Remove(coords);
        buildingByCoordinates.Add(coords, building1);
        coords = cellCoordinates[Random.Range(0, cellCoordinates.Count)];
        cellCoordinates.Remove(coords);
        buildingByCoordinates.Add(coords, building2);

        while (cellCoordinates.Count > 0)
        {
            coords = cellCoordinates[Random.Range(0, cellCoordinates.Count)];
            cellCoordinates.Remove(coords);
            buildingByCoordinates.Add(coords, Random.Range(0, 2) == 0 ? building1 : building2);
        }

        return buildingByCoordinates;
    }

    public Dictionary<Vector3Int, Building> GetInitialFillerShapeDefinition()
    {
        //populate positions to pick from
        var availablePositions = new List<Vector3Int>();
        for (var x = 0; x < Grid.MAP_SIZE; x++)
        {
            for (var z = 0; z < Grid.MAP_SIZE; z++)
                availablePositions.Add(new Vector3Int(x, 0, z));
        }
        
        var shapeDefinition = new Dictionary<Vector3Int, Building>();
        
        for (var i = 0; i < fillerBuildings.Length; i++)
        {
            var fillerBuilding = fillerBuildings[i];
            var quota = fillerBuildingQuota[i];
            var nBuildingsToPlace = Mathf.FloorToInt(Grid.MAP_SIZE * Grid.MAP_SIZE * quota);

            for (var j = 0; j < nBuildingsToPlace; j++)
            {
                var position = availablePositions[Random.Range(0, availablePositions.Count)];
                availablePositions.Remove(position);
                shapeDefinition.Add(position, fillerBuilding);
            }
        }
        
        return shapeDefinition;
    }
}