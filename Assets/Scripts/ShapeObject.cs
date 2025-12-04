using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShapeObject : MonoBehaviour
{
    public Dictionary<Vector3Int, CellObject> CellsByCoordinate { get; private set; } = new();
    public List<CellObject> Cells => CellsByCoordinate.Values.ToList();
    public List<Vector3Int> CellCoordinates => CellsByCoordinate.Keys.ToList();

    private Quaternion _desiredRotation = Quaternion.identity;
    private Coroutine _rotationRoutine;

    public void SetMaterialAlpha(float newAlpha) => Cells.ForEach(c => c.SetAlpha(newAlpha)); 
    public void SetMaterialWhiteBlend(float newWhiteBlend) => Cells.ForEach(c => c.SetWhiteBlend(newWhiteBlend));
    public void EnableColliders() => Cells.ForEach(c => c.Collider.enabled = true);

    public void Rotate90(Vector3Int axis, int turnDirection)
    {
        Debug.Log("Rotate90 called on shapeObject");
        var rotatedCellsByCoordinates = new Dictionary<Vector3Int, CellObject>();
        foreach (var (coord, cell) in CellsByCoordinate)
        {
            rotatedCellsByCoordinates.Add(coord.Rotate90(axis, turnDirection), cell);
        }
        CellsByCoordinate = rotatedCellsByCoordinates;
        
        if (_rotationRoutine != null)
            StopCoroutine(_rotationRoutine);
        
        transform.rotation = _desiredRotation;

        var angle = turnDirection * 90f;
        var newRot = Quaternion.AngleAxis(angle, axis);
        _desiredRotation = newRot * _desiredRotation;
        _rotationRoutine = StartCoroutine(RotationRoutine());
    }

    public void Hide()
    {
        if (_rotationRoutine != null)
            StopCoroutine(_rotationRoutine);
        transform.rotation = _desiredRotation;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private IEnumerator RotationRoutine()
    {
        Debug.Log("RotationRoutine on shapeObject started");
        var t = 0f;
        var rotationSpeed = 3f;
        do
        {
            t += rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, _desiredRotation, t);
            yield return null;
        } while (t <= 1f);
        Debug.Log("RotationRoutine on shapeObject ended");
    }
}