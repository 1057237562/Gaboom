using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gaboom.Util
{
    public class GameLogic
    {
        public static bool inputingKey = false;

        public static bool IsPointerOverGameObject()
        {
            if (inputingKey) return false;
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;

        }
    }
}
