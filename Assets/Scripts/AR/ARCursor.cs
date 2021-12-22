using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.Netcode;

public class ARCursor : NetworkBehaviour
{
    public GameObject cursorChildObject;
    public NetworkObject objectToPlace;
    public ARRaycastManager raycastManager;

    public bool useCursor = true;

    // Start is called before the first frame update
    void Start()
    {
        cursorChildObject.SetActive(useCursor);
        raycastManager = FindObjectOfType<ARRaycastManager>().GetComponent<ARRaycastManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (useCursor)
        {
            UpdateCursor();
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (useCursor)
            {
                SpawnCapsuleServerRpc(transform.position, transform.rotation);
            }
            else
            {
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                raycastManager.Raycast(Input.GetTouch(0).position, hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes);
                if (hits.Count > 0)
                {
                    SpawnCapsuleServerRpc(hits[0].pose.position, hits[0].pose.rotation);
                }
            }
        }
    }

    [ServerRpc]
    public void SpawnCapsuleServerRpc(Vector3 position, Quaternion rotation)
    {
        NetworkObject objectToPlaceInstance = Instantiate(objectToPlace, position, rotation);
        objectToPlaceInstance.Spawn();
    }

    void UpdateCursor()
    {
        Vector2 screenPosition = Camera.main.ViewportToScreenPoint(new Vector2(0.5f, 0.5f));
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        raycastManager.Raycast(screenPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes);

        if (hits.Count > 0)
        {
            transform.position = hits[0].pose.position;
            transform.rotation = hits[0].pose.rotation;
        }
    }
}
