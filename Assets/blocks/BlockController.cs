using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class BlockController : MonoBehaviour {

    new private BoxCollider collider;
	
    // Use this for initialization
	void Start () {
        this.collider = GetComponent<BoxCollider>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //called when the group stops falling and every cube is fixed in the playing field
    public void wasFixed()
    {
        //eliminate rounding errors from previous rotations before fixing the block:
        transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z));
        transform.rotation = Quaternion.Euler(0, Mathf.Round(transform.rotation.eulerAngles.y / 90.0f)*90f, 0);
        this.collider.enabled = true;
    }

    public void removeLine()
    {
        //TODO: spectacular self destruction animation ;-)
        DestroyImmediate(this.gameObject);
    }

    //called when a full line was removed below this block, so this one has to drop 1 line
    public void MoveOneDown()
    {
        transform.position += new Vector3(0, -1, 0); ;
    }

    public void SetColor(Color c)
    {
        foreach (Transform t in transform)
        {
            t.gameObject.GetComponent<MeshRenderer>().material.color = c;
        }
    }
}
