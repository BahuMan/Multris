using UnityEngine;
using System.Collections;

public class BlockController : MonoBehaviour {

    private static float DropDuration = 0.2f; //time it takes to drop 1 line when underlying line has been deleted
    public float FadeDuration = 1f;
    public Color _color;
    [SerializeField] ParticleSystem ParticleExplosion;

    public delegate bool isBlockDestructionDoneUpdate();
    public event isBlockDestructionDoneUpdate IsBlockDestructionDone;

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
            DestructionTime = Time.time + Random.Range(0f, .1f);
            return false;
        }
        else if (Time.time > DestructionTime)
        {

            if (IsBlockDestructionDone != null && !IsBlockDestructionDone())
            {
                //someone else is doing stuff that prevents this block from self-destruction. Hold on.
                return false;
            }

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
    //first call will specify amout of lines to drop (as a float) and must NOT be negative.
    //subsequent calls will be 0, UNLESS additional lines have been deleted.
    private float DropStartTime = 0f;
    private float DropEndTime = 0f;
    private float DropStartPos = float.NaN;
    private float DropEndPos = float.NaN;
    public bool UpdateDownDone(float AmountToDrop)
    {
        if (AmountToDrop > 0f) //a new line has been deleted; we need to drop more
        {
            if (float.IsNaN(DropStartPos))
            {
                //we were not busy dropping, so set a new DropEndTime:
                DropStartTime = Time.time; // + Random.Range(0f, DropDuration / 2f);
                DropEndTime = Time.time + DropDuration * AmountToDrop;
                DropStartPos = transform.position.y;
                DropEndPos = transform.position.y - AmountToDrop;
            }
            else
            {
                //we were in the middle of a drop, so simply add another:
                DropEndTime += DropDuration * AmountToDrop;
                DropEndPos -= AmountToDrop;
            }
            return false;
        }

        if (DropStartTime > Time.time) return false; //a little pauze before actually dropping

        if (Time.time > DropEndTime)
        {
            DropStartPos = float.NaN; //reset this so next time we drop, we know it's from a stable position
            transform.position = new Vector3(transform.position.x, Mathf.Round(DropEndPos), transform.position.z);
            return true;
        }
        else
        {
            //now drop:
            float timeLeft = (DropEndTime - Time.time);
            float pointintime = (Time.time - DropStartTime) / (DropEndTime - DropStartTime);
            float currentY = Mathf.Lerp(DropStartPos, DropEndPos, Mathf.Pow(pointintime, 4));
            transform.position = new Vector3(transform.position.x, currentY, transform.position.z);
            return false;
        }
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
