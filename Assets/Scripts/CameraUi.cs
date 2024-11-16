using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera camera1;
    public Camera camera2;
    public Camera camera3;

    private void Start()
    {
    
        SetActiveCamera(1);
    }

    public void SwitchToCamera1()
    {
        SetActiveCamera(1);
    }

    public void SwitchToCamera2()
    {
        SetActiveCamera(2);
    }

    public void SwitchToCamera3()
    {
        SetActiveCamera(3);
    }

    private void SetActiveCamera(int cameraNumber)
    {
        camera1.gameObject.SetActive(false);
        camera2.gameObject.SetActive(false);
        camera3.gameObject.SetActive(false);
        
        if (cameraNumber == 1)
        {
            camera1.gameObject.SetActive(true);
        }
        else if (cameraNumber == 2)
        {
            camera2.gameObject.SetActive(true);
        }
        else if (cameraNumber == 3)
        {
            camera3.gameObject.SetActive(true);
        }
    }
}
