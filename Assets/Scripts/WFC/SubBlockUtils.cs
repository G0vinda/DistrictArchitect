using System;
using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    public static class SubBlockUtils
    {
        public static SubBlockType GetTypeFromCoordinate(Vector3Int coordinate)
        {
            switch (coordinate)
            {
                case {x: 0, y: 0, z: 0}:
                    return SubBlockType.Center;
                case {x: 0, y: 1, z: 0}:
                    return SubBlockType.TopFace;
                case {x: 0, y: 0, z: 1}:
                case {x: 0, y: 0, z: -1}:
                case {x: 1, y: 0, z: 0}:
                case {x: -1, y: 0, z: 0}:
                    return SubBlockType.MiddleFace;
                case {x: 0, y: -1, z: 0}:
                    return SubBlockType.BottomFace;
                case {x: 1, y: 1, z: 1}:
                case {x: 1, y: 1, z: -1}:
                case {x: -1, y: 1, z: -1}:
                case {x: -1, y: 1, z: 1}:
                    return SubBlockType.TopCorner;
                case {x: 1, y: -1, z: -1}:
                case {x: 1, y: -1, z: 1}:
                case {x: -1, y: -1, z: -1}:
                case {x: -1, y: -1, z: 1}:
                    return SubBlockType.BottomCorner;
                case {x: 0, y: 1, z: -1}:
                case {x: 0, y: 1, z: 1}:
                case {x: 1, y: 1, z: 0}:
                case {x: -1, y: 1, z: 0}:
                    return SubBlockType.TopEdge;
                case {x: 1, y: 0, z: -1}:
                case {x: 1, y: 0, z: 1}:
                case {x: -1, y: 0, z: -1}:
                case {x: -1, y: 0, z: 1}:
                    return SubBlockType.MiddleEdge;
                case {x: 0, y: -1, z: -1}:
                case {x: 0, y: -1, z: 1}:
                case {x: 1, y: -1, z: 0}:
                case {x: -1, y: -1, z: 0}:
                    return SubBlockType.BottomEdge;
                default:
                    throw new ArgumentOutOfRangeException(nameof(coordinate), coordinate, null);
            }
        }
        
        public static Quaternion GetRotationFromCoordinate(Vector3Int coordinate)
        {
            switch (coordinate)
            {
                case { x: 0, z: 0 }:
                case { x: 1, z: 1 }:
                case { x: 1, z: 0 }:
                    return Quaternion.identity;
                case { x: -1, z: 1 }:
                case { x: 0, z: 1 }:
                    return Quaternion.Euler(0, -90, 0);
                case { x: -1, z: -1 }:
                case { x: -1, z: 0 }:
                    return Quaternion.Euler(0, 180, 0);
                case { x: 1, z: -1 }:
                case { x: 0, z: -1 }:
                    return Quaternion.Euler(0, 90, 0);
                default:
                    throw new ArgumentOutOfRangeException(nameof(coordinate), coordinate, null);
            }
        }

        public static SubBlockType GetNeighborTypeInDirection(this SubBlockType originType, Vector3Int direction)
        {
            return originType switch
            {
                SubBlockType.TopCorner => GetNeighborTypeInDirectionForTopCorner(direction),
                SubBlockType.BottomCorner => GetNeighborTypeInDirectionForBottomCorner(direction),
                SubBlockType.TopEdge => GetNeighborTypeInDirectionForTopEdge(direction),
                SubBlockType.MiddleEdge => GetNeighborTypeInDirectionForMiddleEdge(direction),
                SubBlockType.BottomEdge => GetNeighborTypeInDirectionForBottomEdge(direction),
                SubBlockType.TopFace => GetNeighborTypeInDirectionForTopFace(direction),
                SubBlockType.MiddleFace => GetNeighborTypeInDirectionForMiddleFace(direction),
                SubBlockType.BottomFace => GetNeighborTypeInDirectionForBottomFace(direction),
                SubBlockType.Center => GetNeighborTypeInDirectionForCenter(direction),
                _ => throw new ArgumentOutOfRangeException(nameof(originType), originType, null)
            };
        }
        
        private static SubBlockType GetNeighborTypeInDirectionForTopCorner(Vector3Int direction)
        {
            return direction switch
            {
                { x: 1, y: 0, z: 0 } => SubBlockType.TopCorner,
                { x: 0, y: 0, z: 1 } => SubBlockType.TopCorner,
                { x: -1, y: 0, z: 0 } => SubBlockType.TopEdge,
                { x: 0, y: 0, z: -1 } => SubBlockType.TopEdge,
                { x: 0, y: 1, z: 0 } => SubBlockType.BottomCorner,
                { x: 0, y: -1, z: 0 } => SubBlockType.MiddleEdge,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        
        private static SubBlockType GetNeighborTypeInDirectionForBottomCorner(Vector3Int direction)
        {
            return direction switch
            {
                { x: 1, y: 0, z: 0 } => SubBlockType.BottomCorner,
                { x: 0, y: 0, z: 1 } => SubBlockType.BottomCorner,
                { x: -1, y: 0, z: 0 } => SubBlockType.BottomEdge,
                { x: 0, y: 0, z: -1 } => SubBlockType.BottomEdge,
                { x: 0, y: 1, z: 0 } => SubBlockType.MiddleEdge,
                { x: 0, y: -1, z: 0 } => SubBlockType.TopCorner,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        
        private static SubBlockType GetNeighborTypeInDirectionForTopEdge(Vector3Int direction)
        {
            return direction switch
            {
                { x: 1, y: 0, z: 0 } => SubBlockType.TopEdge,
                { x: 0, y: 0, z: 1 } => SubBlockType.TopCorner,
                { x: -1, y: 0, z: 0 } => SubBlockType.TopFace,
                { x: 0, y: 0, z: -1 } => SubBlockType.TopCorner,
                { x: 0, y: 1, z: 0 } => SubBlockType.BottomEdge,
                { x: 0, y: -1, z: 0 } => SubBlockType.MiddleFace,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        
        private static SubBlockType GetNeighborTypeInDirectionForMiddleEdge(Vector3Int direction)
        {
            return direction switch
            {
                { x: 1, y: 0, z: 0 } => SubBlockType.MiddleEdge,
                { x: 0, y: 0, z: 1 } => SubBlockType.MiddleEdge,
                { x: -1, y: 0, z: 0 } => SubBlockType.MiddleFace,
                { x: 0, y: 0, z: -1 } => SubBlockType.MiddleFace,
                { x: 0, y: 1, z: 0 } => SubBlockType.TopCorner,
                { x: 0, y: -1, z: 0 } => SubBlockType.BottomCorner,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        
        private static SubBlockType GetNeighborTypeInDirectionForBottomEdge(Vector3Int direction)
        {
            return direction switch
            {
                { x: 1, y: 0, z: 0 } => SubBlockType.BottomEdge,
                { x: 0, y: 0, z: 1 } => SubBlockType.BottomCorner,
                { x: -1, y: 0, z: 0 } => SubBlockType.BottomFace,
                { x: 0, y: 0, z: -1 } => SubBlockType.BottomCorner,
                { x: 0, y: 1, z: 0 } => SubBlockType.MiddleFace,
                { x: 0, y: -1, z: 0 } => SubBlockType.TopEdge,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        
        private static SubBlockType GetNeighborTypeInDirectionForTopFace(Vector3Int direction)
        {
            return direction switch
            {
                { x: 1, y: 0, z: 0 } => SubBlockType.TopEdge,
                { x: 0, y: 0, z: 1 } => SubBlockType.TopEdge,
                { x: -1, y: 0, z: 0 } => SubBlockType.TopEdge,
                { x: 0, y: 0, z: -1 } => SubBlockType.TopEdge,
                { x: 0, y: 1, z: 0 } => SubBlockType.BottomFace,
                { x: 0, y: -1, z: 0 } => SubBlockType.Center,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        
        private static SubBlockType GetNeighborTypeInDirectionForMiddleFace(Vector3Int direction)
        {
            return direction switch
            {
                { x: 1, y: 0, z: 0 } => SubBlockType.MiddleFace,
                { x: 0, y: 0, z: 1 } => SubBlockType.MiddleEdge,
                { x: -1, y: 0, z: 0 } => SubBlockType.Center,
                { x: 0, y: 0, z: -1 } => SubBlockType.MiddleEdge,
                { x: 0, y: 1, z: 0 } => SubBlockType.TopEdge,
                { x: 0, y: -1, z: 0 } => SubBlockType.BottomEdge,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        
        private static SubBlockType GetNeighborTypeInDirectionForBottomFace(Vector3Int direction)
        {
            return direction switch
            {
                { x: 1, y: 0, z: 0 } => SubBlockType.BottomEdge,
                { x: 0, y: 0, z: 1 } => SubBlockType.BottomEdge,
                { x: -1, y: 0, z: 0 } => SubBlockType.BottomEdge,
                { x: 0, y: 0, z: -1 } => SubBlockType.BottomEdge,
                { x: 0, y: 1, z: 0 } => SubBlockType.Center,
                { x: 0, y: -1, z: 0 } => SubBlockType.TopFace,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        
        private static SubBlockType GetNeighborTypeInDirectionForCenter(Vector3Int direction)
        {
            return direction switch
            {
                { x: 1, y: 0, z: 0 } => SubBlockType.MiddleFace,
                { x: 0, y: 0, z: 1 } => SubBlockType.MiddleFace,
                { x: -1, y: 0, z: 0 } => SubBlockType.MiddleFace,
                { x: 0, y: 0, z: -1 } => SubBlockType.MiddleFace,
                { x: 0, y: 1, z: 0 } => SubBlockType.TopFace,
                { x: 0, y: -1, z: 0 } => SubBlockType.BottomFace,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        public static bool TryExtractSubBlockTypeFromName(string name, out SubBlockType subBlockType)
        {
            var abbreviationPairs = new Dictionary<string, SubBlockType>
            {
                { "_TC", SubBlockType.TopCorner },
                { "_BC", SubBlockType.BottomCorner },
                { "_TE", SubBlockType.TopEdge },
                { "_ME", SubBlockType.MiddleEdge },
                { "_BE", SubBlockType.BottomEdge },
                { "_TF", SubBlockType.TopFace },
                { "_MF", SubBlockType.MiddleFace },
                { "_BF", SubBlockType.BottomFace },
                { "_C", SubBlockType.Center }
            };

            foreach (var (abbreviation, type) in abbreviationPairs)
            {
                if (name.Contains(abbreviation))
                {
                    subBlockType = type;
                    return true;
                }
            }
            
            subBlockType = default;
            return false;
        }
    }
}