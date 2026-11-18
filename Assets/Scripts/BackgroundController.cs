using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    private float startPosX;
    private float startPosY;
    private float length;

    public GameObject cam;
    public float parallaxEffectX;
    public float parallaxEffectY;

    void Start()
    {
        startPosX = transform.position.x;
        startPosY = transform.position.y;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void FixedUpdate()
    {
        float distanceX = cam.transform.position.x * parallaxEffectX;
        float distanceY = cam.transform.position.y * parallaxEffectY;
        float movement = cam.transform.position.x * (1 - parallaxEffectX);

        transform.position = new Vector3(startPosX + distanceX, startPosY + distanceY, transform.position.z);

        if (movement > startPosX + length) startPosX += length;
        else if (movement < startPosX - length) startPosX -= length;
    }
}
