using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


namespace ChobiAssets.PTM
{

    [System.Serializable]
    public class Position_Marker_Prop
    {
        public Image Marker_Image;
        public Transform Marker_Transform;
        public Transform Root_Transform;
        public Transform Body_Transform;
    }


    [DefaultExecutionOrder(+2)] // (Note.) This script is executed after the "Camera_Points_Manager_CS", in order to move the marker smoothly.
    public class UI_Position_Marker_Control_CS : MonoBehaviour
    {
        /*
		 * This script is attached to the top object of the tank.
		 * This script controls the 'Position_Marker' in the scene.
		 * This script works in combination with "ID_Settings_CS" and "AI_CS" in the tank.
		*/


        // User options >>
        public GameObject Marker_Prefab;
        public string Canvas_Name = "Canvas_Markers";
        public Color Friend_Color = Color.blue;
        public Color Hostile_Color = Color.red;
        public Color Landmark_Color = Color.green;
        public float Defensive_Alpha = 0.25f;
        public float Offensive_Alpha = 1.0f;
        public float Upper_Offset = 96.0f;
        public float Side_Offset = 96.0f;
        public float Bottom_Offset = 120.0f;
        public bool Show_Always = true;
        // << User options


        List<ID_Settings_CS> idScriptsList = new List<ID_Settings_CS>();
        Dictionary<ID_Settings_CS, Position_Marker_Prop> markerDictionary = new Dictionary<ID_Settings_CS, Position_Marker_Prop>();
        Canvas canvas;
        CanvasScaler canvasScaler;
        Quaternion leftRot = Quaternion.Euler(new Vector3(0.0f, 0.0f, -90.0f));
        Quaternion rightRot = Quaternion.Euler(new Vector3(0.0f, 0.0f, 90.0f));


        void Awake()
        { // This function must be exected before "Start()", because the Canvas is required in "Receive_ID_Script()" called in "Start()".
            Initialize();
        }


        void Initialize()
        {
            // Check the prefab.
            if (Marker_Prefab == null)
            {
                Debug.LogWarning("'Prefab for 'Position Maker' is not assigned.");
                Destroy(this);
                return;
            }

            // Check the canvas name.
            if (string.IsNullOrEmpty(Canvas_Name))
            {
                Debug.LogWarning("Canvas name for 'Position Maker' is not assigned.");
                Destroy(this);
                return;
            }

            // Get the canvas.
            var canvasObject = GameObject.Find(Canvas_Name);
            if (canvasObject)
            {
                canvas = canvasObject.GetComponent<Canvas>();
            }
            if (canvas == null)
            {
                Debug.LogWarning("Canvas for 'Position Maker' cannot be found.");
                Destroy(this);
                return;
            }
            canvasScaler = canvas.GetComponent<CanvasScaler>();
        }


        void Receive_ID_Script(ID_Settings_CS idScript)
        { // Called from "ID_Settings_CS" in tanks in the scene, when the tank is spawned.

            // Add the "ID_Settings_CS" to the list.
            idScriptsList.Add(idScript);

            // Create a new marker object.
            var markerObject = Instantiate(Marker_Prefab, canvas.transform);

            // Add the components to the dictionary.
            var newProp = new Position_Marker_Prop();
            newProp.Marker_Image = markerObject.GetComponent<Image>();
            newProp.Marker_Transform = newProp.Marker_Image.transform;
            newProp.Root_Transform = idScript.transform.root;
            newProp.Body_Transform = idScript.GetComponentInChildren<Rigidbody>().transform;
            markerDictionary.Add(idScript, newProp);
        }


        void LateUpdate()
        {
            Control_Markers();
        }


        void Control_Markers()
        {
            var mainCamera = Camera.main;
            var resolutionOffset = Screen.width / canvasScaler.referenceResolution.x;

            for (int i = 0; i < idScriptsList.Count; i++)
            {
                // Check the tank is selected now, or has been dead.
                if (idScriptsList[i].Is_Selected || markerDictionary[idScriptsList[i]].Root_Transform.tag == "Finish")
                {
                    markerDictionary[idScriptsList[i]].Marker_Image.enabled = false;
                    continue;
                }

                // Check the tank has been respawned.
                if (markerDictionary[idScriptsList[i]].Body_Transform == null)
                { // The reference to the MainBody has been lost. >> The tank has been respawned.
                    // Get the new references to the "MainBody" and "AI_CS".
                    markerDictionary[idScriptsList[i]].Body_Transform = idScriptsList[i].GetComponentInChildren<Rigidbody>().transform;
                }

                // Set the enabled and the color, according to the relationship and the AI condition.
                switch (idScriptsList[i].Relationship)
                {
                    case 0: // Friendly.
                        markerDictionary[idScriptsList[i]].Marker_Image.enabled = true;

                        markerDictionary[idScriptsList[i]].Marker_Image.enabled = Show_Always;
                        markerDictionary[idScriptsList[i]].Marker_Image.color = Friend_Color;
                        break;

                    case 1: // Hostile.

                        markerDictionary[idScriptsList[i]].Marker_Image.enabled = Show_Always;
                        markerDictionary[idScriptsList[i]].Marker_Image.color = Hostile_Color;
                        break;

                    case 2: // Landmark.
                        markerDictionary[idScriptsList[i]].Marker_Image.enabled = true;
                        markerDictionary[idScriptsList[i]].Marker_Image.color = Landmark_Color;
                        break;
                }
                if (markerDictionary[idScriptsList[i]].Marker_Image.enabled == false)
                {
                    continue;
                }

                // Calculate the position and rotation.
                var dist = Vector3.Distance(mainCamera.transform.position, markerDictionary[idScriptsList[i]].Body_Transform.position);
                var currentPos = mainCamera.WorldToScreenPoint(markerDictionary[idScriptsList[i]].Body_Transform.position);
                if (currentPos.z > 0.0f)
                { // In front of the camera.
                    currentPos.z = 100.0f;
                    if (currentPos.x < Side_Offset)
                    { // Over the left end.
                        currentPos.x = Side_Offset * resolutionOffset;
                        currentPos.y = Screen.height * Mathf.Lerp(0.2f, 0.9f, dist / 500.0f);
                        markerDictionary[idScriptsList[i]].Marker_Transform.localRotation = leftRot;
                    }
                    else if (currentPos.x > (Screen.width - Side_Offset))
                    { // Over the right end.
                        currentPos.x = Screen.width - (Side_Offset * resolutionOffset);
                        currentPos.y = Screen.height * Mathf.Lerp(0.2f, 0.9f, dist / 500.0f);
                        markerDictionary[idScriptsList[i]].Marker_Transform.localRotation = rightRot;
                    }
                    else
                    { // Within the screen.
                        currentPos.y = Screen.height - (Upper_Offset * resolutionOffset);
                        markerDictionary[idScriptsList[i]].Marker_Transform.localRotation = Quaternion.identity;
                    }
                }
                else
                { // Behind of the camera.
                    currentPos.z = -100.0f;
                    if (currentPos.x > (Screen.width - Side_Offset))
                    { // Over the left end.
                        currentPos.x = Side_Offset * resolutionOffset;
                        currentPos.y = Screen.height * Mathf.Lerp(0.2f, 0.9f, dist / 500.0f);
                        markerDictionary[idScriptsList[i]].Marker_Transform.localRotation = leftRot;
                    }
                    else if (currentPos.x < Side_Offset)
                    { // Over the right end.
                        currentPos.x = Screen.width - (Side_Offset * resolutionOffset);
                        currentPos.y = Screen.height * Mathf.Lerp(0.2f, 0.9f, dist / 500.0f);
                        markerDictionary[idScriptsList[i]].Marker_Transform.localRotation = rightRot;
                    }
                    else
                    { // Within the screen.
                        currentPos.x = Screen.width - currentPos.x;
                        currentPos.y = (Bottom_Offset * resolutionOffset);
                        markerDictionary[idScriptsList[i]].Marker_Transform.localRotation = Quaternion.identity;
                    }
                }

                // Set the position.
                markerDictionary[idScriptsList[i]].Marker_Transform.position = currentPos;
            }
        }


        void Remove_ID(ID_Settings_CS idScript)
        { // Called from "ID_Settings_CS", just before the tank is removed from the scene.

            // Destroy the marker.
            Destroy(markerDictionary[idScript].Marker_Image.gameObject);

            // Remove the "ID_Settings_CS" from the list.
            idScriptsList.Remove(idScript);

            // Remove the components from the dictionary.
            markerDictionary.Remove(idScript);
        }

    }

}