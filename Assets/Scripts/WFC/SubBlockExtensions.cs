using System;
using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    public static class SubBlockExtensions
    {
        public static SubBlockType GetNeighborTypeInDirection(this SubBlockType originType, Vector3Int direction)
        {
            var coordinateInDirection = originType.GetDefaultCoordinate() + direction;
            var wrappedCoordinateInDirection = coordinateInDirection.GetWrappedNeg1To1();
            return SubBlockUtils.GetTypeFromCoordinates(wrappedCoordinateInDirection);
        }
        
        public static Vector3Int GetDefaultCoordinate(this SubBlockType type)
        {
            return type switch
            {
                SubBlockType.TopCorner => new Vector3Int(1, 1, 1),
                SubBlockType.BottomCorner => new Vector3Int(1, -1, 1),
                SubBlockType.TopEdge => new Vector3Int(1, 1, 0),
                SubBlockType.MiddleEdge => new Vector3Int(1, 0, 1),
                SubBlockType.BottomEdge => new Vector3Int(1, -1, 0),
                SubBlockType.TopFace => new Vector3Int(0, 1, 0),
                SubBlockType.MiddleFace => new Vector3Int(1, 0, 0),
                SubBlockType.BottomFace => new Vector3Int(0, -1, 0),
                SubBlockType.Center => Vector3Int.zero,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        
        public static List<Vector3Int> GetOutwardFacingDirections(this SubBlockType type)
        {
            var outwardFacingDirections = new List<Vector3Int>();
            var allDirections = Vector3Int.zero.Neighbours();
            var defaultTypeCoordinate = type.GetDefaultCoordinate();
            
            foreach (var direction in allDirections)
            {
                var extensionInDirection = defaultTypeCoordinate + direction;
                if (Math.Abs(extensionInDirection.x) > 1 || Math.Abs(extensionInDirection.y) > 1 || Math.Abs(extensionInDirection.z) > 1)
                    outwardFacingDirections.Add(direction);
            }
            
            return outwardFacingDirections;
        }

        public static string GetAbbreviation(this SubBlockType type)
        {
            return type switch
            {
                SubBlockType.TopCorner => "TC",
                SubBlockType.BottomCorner => "BC",
                SubBlockType.TopEdge => "TE",
                SubBlockType.MiddleEdge => "ME",
                SubBlockType.BottomEdge => "BE",
                SubBlockType.TopFace => "TF",
                SubBlockType.MiddleFace => "MF",
                SubBlockType.BottomFace => "BF",
                SubBlockType.Center => "C",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}