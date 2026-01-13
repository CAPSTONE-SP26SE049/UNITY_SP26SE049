using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SideScrollingCamera : MonoBehaviour
{
    public Transform trackedObject;
    public float height = 6.5f;
    public float undergroundHeight = -9.5f;
    public float undergroundThreshold = 0f;

    private void Awake()
    {
        // Tự động tìm MainChar nếu trackedObject chưa được gán
        if (trackedObject == null)
        {
            // Tìm MainChar trước
            GameObject mainChar = GameObject.FindGameObjectWithTag("Player");
            if (mainChar != null)
            {
                // Kiểm tra xem có phải MainChar không (có MainCharMovement component)
                if (mainChar.GetComponent<MainCharMovement>() != null)
                {
                    trackedObject = mainChar.transform;
                }
                // Nếu không phải MainChar, tìm Player (Mario)
                else if (mainChar.GetComponent<PlayerMovement>() != null)
                {
                    trackedObject = mainChar.transform;
                }
            }
            
            // Nếu vẫn không tìm thấy, tìm bằng tên
            if (trackedObject == null)
            {
                GameObject mainCharObj = GameObject.Find("MainChar");
                if (mainCharObj != null)
                {
                    trackedObject = mainCharObj.transform;
                }
            }
        }
    }

    private void LateUpdate()
    {
        // Kiểm tra null để tránh UnassignedReferenceException
        if (trackedObject == null)
        {
            // Thử tìm lại nếu bị mất reference
            GameObject mainChar = GameObject.FindGameObjectWithTag("Player");
            if (mainChar != null && mainChar.GetComponent<MainCharMovement>() != null)
            {
                trackedObject = mainChar.transform;
            }
            
            if (trackedObject == null)
            {
                return;
            }
        }

        Vector3 cameraPosition = transform.position;
        cameraPosition.x = Mathf.Max(cameraPosition.x, trackedObject.position.x);
        transform.position = cameraPosition;
    }

    public void SetUnderground(bool underground)
    {
        Vector3 cameraPosition = transform.position;
        cameraPosition.y = underground ? undergroundHeight : height;
        transform.position = cameraPosition;
    }

}
