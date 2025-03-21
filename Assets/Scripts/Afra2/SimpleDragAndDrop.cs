using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleDragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rt; // RectTransform of the dragged letter
    private Vector3 originalPosition; // Original position of the letter
    private Transform originalParent; // The original parent of the letter
    
    public string correctLetter; // The correct letter for this drag object
    
    // Called when dragging starts
    public void OnBeginDrag(PointerEventData eventData)
    {
        rt = GetComponent<RectTransform>();
        originalPosition = rt.position; // Store original position
        originalParent = transform.parent; // Store the original parent
    }
    
    // Called while dragging
    public void OnDrag(PointerEventData eventData)
    {
        rt.position = Input.mousePosition; // Move letter with mouse position
    }
    
    // Called when drag ends (On Drop is handled elsewhere)
    public void OnEndDrag(PointerEventData eventData)
    {
        // No need to handle anything here, it's done in OnDrop in SimpleDropZone
    }
    
    // Method to reset the position of the letter when the answer is wrong
    public void ResetPosition()
    {
        rt.position = originalPosition; // Reset the position to the original one
        transform.SetParent(originalParent); // Reset the parent to its original parent
    }
}