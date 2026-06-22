using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCam != null)
        {
            // Match camera rotation to make 2D object always face camera
            transform.rotation = mainCam.transform.rotation;
        }
    }

    public void OnValidate()
    {
        if (mainCam == null)
        {
            GameObject camObj = GameObject.FindWithTag("MainCamera");
            if (camObj != null)
                mainCam = camObj.GetComponent<Camera>();

        }
    }
}
