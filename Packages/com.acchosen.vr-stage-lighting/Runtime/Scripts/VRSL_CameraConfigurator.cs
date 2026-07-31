using UnityEngine;

#if UDONSHARP
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;
#endif

#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;

#if UDONSHARP
using UdonSharpEditor;
#endif
#endif

namespace VRSL.EditorScripts
{
#if UDONSHARP
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VRSL_CameraConfigurator : UdonSharpBehaviour
#else
    public class VRSL_CameraConfigurator : MonoBehaviour
#endif
    {
        public Camera camObj;
        public float defaultSize = 9.6f;
        public float vertDefaultSize = 5.4f;
        public float percentageReduction = 0.6667f;
        public float percentageFurtherReduction = 0.4444f;



        public const float vertXMin1080p = -4.427f;
        public const float vertXMax1080p  = 4.478f;
        public const float vertXMin720p = -4.6189f;
        public const float vertXMax720p = 4.659f;
        public const float vertXMin480p= -4.7393f;
        public const float vertXMax480p= 4.7789f;
        public const float horizXMin1080p = 0.02f;
        public const float horizXMax1080p = 0.02f;
        public const float horizXMin720p= -1.648f;
        public const float horizXMax720p = 1.684f;
        public const float horizXMin480p = -2.755f;
        public const float horizXMax480p = 2.798f;


        
        public const float vertYMin1080p = 0.25f;
        public const float vertYMax1080p  = 0.25f;
        public const float vertYMin720p = -1.416f;
        public const float vertYMax720p = 1.916f;
        public const float vertYMin480p= -2.528f;
        public const float vertYMax480p= 3.028f;
        public const float horizYMin1080p = -3.79f;
        public const float horizYMax1080p = 4.3f;
        public const float horizYMin720p= -4.108f;
        public const float horizYMax720p = 4.606f;
        public const float horizYMin480p = -4.322f;
        public const float horizYMax480p = 4.822f;








        public const int TEN80p = 0;
        public const int SEVEN20p = 1;
        public const int FOUR80p = 2;

        [SerializeField, FieldChangeCallback(nameof(YPos))]
        private float yPos = -3.79f;
        public float YPos
        {
            get => yPos;
            set
            {
                yPos = value;
                _UpdateCameraPosition();
            }
        }
        [SerializeField, FieldChangeCallback(nameof(XPos))]
        private float xPos = 0.02f;
        public float XPos
        {
            get => xPos;
            set
            {
                xPos = value;
                _UpdateCameraPosition();
            }
        }

        [SerializeField, FieldChangeCallback(nameof(IsHorizontal))]
        private bool isHorizontal = true;

        public bool IsHorizontal{
            get => isHorizontal;
            set
            {
                isHorizontal = value;
                xPos = 0.0f;
                _UpdateCameraPosition();
            }
        }

        [SerializeField ,FieldChangeCallback(nameof(Resolution))]
        private int resolution = 0;

        public int Resolution{
            get => resolution;
            set
            {
                resolution = value;
                _UpdateCameraPosition();
            }
        }

        public int scaleDefine;
        void Start()
        {
            _TryGetCam();
        }
        public void _UpdateCameraPosition()
        {

            if(IsHorizontal)
            {
                switch(resolution)
                {
                    case TEN80p:
                        yPos = Mathf.Clamp(yPos, horizYMin1080p, horizYMax1080p);
                        xPos = Mathf.Clamp(xPos, horizXMin1080p,horizXMax1080p);
                        break;
                    case SEVEN20p:
                        yPos = Mathf.Clamp(yPos, horizYMin720p,horizYMax720p);
                        xPos = Mathf.Clamp(xPos, horizXMin720p,horizXMax720p);
                        break;
                    case FOUR80p:
                        yPos = Mathf.Clamp(yPos, horizYMin480p, horizYMax480p);
                        xPos = Mathf.Clamp(xPos, horizXMin480p,horizXMax480p);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch(resolution)
                {
                    case TEN80p:
                        yPos = Mathf.Clamp(yPos, vertYMin1080p, vertYMax1080p);
                        xPos = Mathf.Clamp(xPos,vertXMin1080p ,vertXMax1080p);
                        break;
                    case SEVEN20p:
                        yPos = Mathf.Clamp(yPos, vertYMin720p, vertYMax720p);
                        xPos = Mathf.Clamp(xPos,vertXMin720p ,vertXMax720p);
                        break;
                    case FOUR80p:
                        yPos = Mathf.Clamp(yPos, vertYMin480p, vertYMax480p);
                        xPos = Mathf.Clamp(xPos,vertXMin480p ,vertXMax480p);
                        break;
                    default:
                        break;
                }
            }

            camObj.transform.localPosition = new Vector3(xPos,yPos, camObj.transform.localPosition.z);
        }
        public void _TryGetCam()
        {
            if(camObj == null)
            {
                camObj = GetComponent<Camera>();
            }
        }
    }
}
