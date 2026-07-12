/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Linq;
using UnityEngine;
using TouchScript.Gestures.TransformGestures;
using TouchScript.Pointers;

namespace TouchScript.Examples.CameraControl
{
    /// <exclude />
    public class CameraController : MonoBehaviour
    {
        public ScreenTransformGesture TwoFingerMoveGesture;
        public float PanSpeed = 200f;
        public float RotationSpeed = 200f;
        public float ZoomSpeed = 10f;

        private Transform pivot;
        private Transform cam;
        private Vector3 pos;

        private void Awake()
        {
            pivot = transform.Find("Pivot");
            cam = transform.Find("Pivot/Camera");
            
        }

        private void OnEnable()
        {
            if (TouchManager.Instance != null)
            {
                TouchManager.Instance.PointersAdded += pointersAddedHandler;
            }

            TwoFingerMoveGesture.Transformed += twoFingerTransformHandler;
           
        }

        private void OnDisable()
        {
            if (TouchManager.Instance != null)
            {
                TouchManager.Instance.PointersAdded -= pointersAddedHandler;
            }
            TwoFingerMoveGesture.Transformed -= twoFingerTransformHandler;
           
        }


        private void pointersAddedHandler(object sender, PointerEventArgs e)
        {
            var count = e.Pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = e.Pointers[i];
                // Don't show internal pointers
                if ((pointer.Flags & Pointer.FLAG_INTERNAL) > 0) continue;

               
                switch (pointer.Type)
                {
                    case Pointer.PointerType.Object:
                        if (pointer.ObjectId > 2)
                        {
                            Debug.Log("ich habe ID 3 oder höher erkannt");
                        }
                        else
                        {
                            Debug.Log("ich habe ID 0/1/2  erkannt");
                            continue;

                        }
                        
                        break;
                    default:
                        continue;
                }
            }
        }
     
        
        private void twoFingerTransformHandler(object sender, System.EventArgs e)
        {
                
      
                pos = new Vector3(-TwoFingerMoveGesture.DeltaPosition.x, 0f, -TwoFingerMoveGesture.DeltaPosition.y);
                pivot.localPosition += pos*PanSpeed; 
                Debug.Log(" Scale " + TwoFingerMoveGesture.DeltaScale);
                cam.GetComponent<Camera>().fieldOfView += (1-TwoFingerMoveGesture.DeltaRotation)*ZoomSpeed;


        }
    }
}