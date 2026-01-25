using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    [Header("References")] [SerializeField]
    BuildingPlacement buildingPlacement;

    [SerializeField] private AudioSource hoverSoundPlayer;

    [Header("Clips")] [SerializeField] private AudioClip[] buildingPlaceClips;
    [SerializeField] private AudioClip[] cellDestroyClips;
    [SerializeField] private AudioClip[] uiClickClips;
    [SerializeField] private AudioClip scoreClip;
    
    private AudioClip _lastBuildingPlaceClip;
    private AudioClip _lastCellDestroyClip;
    private AudioClip _lastUiClickClip;

    private const float MIN_HOVER_SOUND_PITCH = 0f;
    private const float MAX_HOVER_SOUND_PITCH = 0.3f;

    private void OnEnable()
    {
        buildingPlacement.PlacedBuilding += OnPlacedBuilding;
        Cell.CellObjectDestroyed += OnCellDestroyed;
        Cell.CellObjectScored += OnCellScored;
    }
    
    private void OnDisable()
    {
        buildingPlacement.PlacedBuilding -= OnPlacedBuilding;
        Cell.CellObjectDestroyed -= OnCellDestroyed;
    }

    public void OnCellDestroyed(Vector3 destroyPosition)
    {
        var destroyClip = GetRandomClipWithout(cellDestroyClips, _lastCellDestroyClip);
        AudioSource.PlayClipAtPoint(destroyClip, destroyPosition);
    }

    public void OnUIHovered()
    {
        // var pitch = Random.Range(MIN_HOVER_SOUND_PITCH, MAX_HOVER_SOUND_PITCH);
        // hoverSoundPlayer.pitch = pitch;
        // hoverSoundPlayer.Play();
    }

    public void OnUIClicked()
    {
        var uiClickClip = GetRandomClipWithout(uiClickClips, _lastUiClickClip);
        AudioSource.PlayClipAtPoint(uiClickClip, Camera.main.transform.position);
    }

    private void OnPlacedBuilding(Vector3 placePosition)
    {
        var buildingPlaceClip = GetRandomClipWithout(buildingPlaceClips, _lastBuildingPlaceClip);
        AudioSource.PlayClipAtPoint(buildingPlaceClip, placePosition);
    }

    private AudioClip GetRandomClipWithout(AudioClip[] clips, AudioClip withoutClip)
    {
        var clipList = clips.ToList();
        if (withoutClip)
            clipList.Remove(withoutClip);
        return clips[UnityEngine.Random.Range(0, clipList.Count)];
    }

    private void OnCellScored(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(scoreClip, position);
    }
}
