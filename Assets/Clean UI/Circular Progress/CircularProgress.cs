using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]

public class CircularProgress : MonoBehaviour {

    [SerializeField]
    Image progressImage;

    [SerializeField]
    Text progressText;

    [SerializeField][Range(0,1)]
    float progress = 0.5f;

    public float Progress { get { return progress; } private set { value = Mathf.Clamp(value, 0, 1); progress = value; ; UpdateProgress(); } }

	void Start () {
        progressImage.fillAmount = progress;
	}
	
	void Update () {
        if (Application.isEditor)
        {
            UpdateProgress();
        }
    }

    void UpdateProgress()
    {
        if (!progressImage)
            return;

        progressImage.fillAmount = progress;

        if (!progressText)
            return;

        progressText.text = progress * 100 + "%";
    }
}
