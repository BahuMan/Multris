using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class BlockController : MonoBehaviour {

    public float FadeDuration = 1f;
    public Color _color;

    //called when the group stops falling and every cube is fixed in the playing field
    public void wasFixed()
    {
        //eliminate rounding errors from previous rotations before fixing the block:
        transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z));
        transform.rotation = Quaternion.Euler(0, Mathf.Round(transform.rotation.eulerAngles.y / 90.0f)*90f, 0);
        StartCoroutine(FadeWhiteToColor(_color));
    }

    IEnumerator FadeWhiteToColor(Color c)
    {
        float FadeStart = Time.time;
        while (Time.time < (FadeStart + FadeDuration))
        {
            SetColor(Color.Lerp(Color.white, c, (Time.time-FadeStart)/FadeDuration));
            yield return null;
        }
        SetColor(c);
    }

    //when a block gets destroyed, this function gets called every frame
    //until it returns true (signalling destruction is complete)
    public bool UpdateDestructionDone()
    {
        //TODO: spectacular self destruction animation ;-)
        DestroyImmediate(this.gameObject);
        return true;
    }

    //called when a full line was removed below this block, so this one has to drop 1 line
    public bool UpdateDownDone()
    {
        transform.position += new Vector3(0, -1, 0);
        return true;
    }

    public void SetColor(Color c)
    {
        _color = c;
        foreach (Transform t in transform)
        {
            t.gameObject.GetComponent<MeshRenderer>().material.color = c;
        }
    }
}
