using UnityEngine;
using System.Collections;

public class DemoController : MonoBehaviour
{
	public GameObject[] Effects;
	private float timeTemp;
	public float SpawnRate = 0.01f;

	void Start ()
	{

	}

	// Update is called once per frame
	void Update ()
	{
		if (Effects.Length <= 0)
			return;

        if (Input.GetButton("Fire1") && Time.time >= timeTemp + SpawnRate)
        {
            var mousePos = Input.mousePosition;
            mousePos.z = 10;       // we want 2m away from the camera position
            var objectPos = Camera.main.ScreenToWorldPoint(mousePos);
            RaycastHit2D hit = Physics2D.Raycast(objectPos, Vector2.up);
            GameObject fx = Effects[Random.Range(0, Effects.Length)];
            GameObject spawned = Instantiate(fx, hit.point, Quaternion.identity);
            GameObject.Destroy(spawned, 2);
            timeTemp = Time.time;
        }
	}
}
