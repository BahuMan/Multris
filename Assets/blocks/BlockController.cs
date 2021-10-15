using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class BlockController : MonoBehaviour {

    public float FadeDuration = 1f;
    public Color _color;
    [SerializeField] ParticleSystem ParticleExplosion;

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

    float DestructionTime = 0f;
    //when a block gets destroyed, this function gets called every frame
    //until it returns true (signalling destruction is complete)
    public bool UpdateDestructionDone()
    {
        if (DestructionTime == 0f)
        {
            Destroy(this.GetComponent<BoxCollider>());
            Destroy(this.GetComponentInChildren<MeshRenderer>());
            DestructionTime = Time.time + Random.Range(0.1f, .3f);
            return false;
        }
        else if (Time.time > DestructionTime)
        {
            if (this.ParticleExplosion != null)
            {
                Instantiate<ParticleSystem>(this.ParticleExplosion).transform.position = this.transform.position;
            }
            Destroy(this.gameObject);
            return true;
        }
        else
        {
            return false;
        }
    }

    //called when a full line was removed below this block, so this one has to drop 1 line
    //first call will specify amout of lines to drop (as a float) and will be negative.
    //subsequent calls will be 0, UNLESS additional lines have been deleted.
    public bool UpdateDownDone(float AmountToDrop)
    {
        transform.position += new Vector3(0, AmountToDrop, 0);
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
