using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace L2
{
    public class DragGameObjectUGUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Vector3 startPos;
        private Vector3 distance;

        public void OnBeginDrag(PointerEventData eventData)
        {
            startPos = this.transform.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 mosPosition = startPos;
#if ENABLE_INPUT_SYSTEM
            mosPosition = Mouse.current.position.ReadValue();
#else
            mosPosition = Input.mousePosition;
#endif
            if (mosPosition.x > 0 && mosPosition.x < Screen.width && mosPosition.y > 0 && mosPosition.y < Screen.height)
            {
                distance = new Vector3(mosPosition.x, mosPosition.y, startPos.z) - startPos;
                transform.position = startPos + distance;
            }

        }

        public void OnEndDrag(PointerEventData eventData)
        {
            
        }
    }
}
