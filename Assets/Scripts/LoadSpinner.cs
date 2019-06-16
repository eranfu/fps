using Game.Main;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class LoadSpinner : MonoBehaviour
{
    [SerializeField] private RawImage spinnerElement;

    private readonly Interpolator _fadeInterpolator = new Interpolator(0, Interpolator.CurveType.SmoothStep);

    private void Update()
    {
        bool isLoading = GameRoot.gameRoot == null ||
                         GameRoot.gameRoot.LevelManager == null ||
                         GameRoot.gameRoot.LevelManager.IsLoadingLevel();
        if (isLoading && _fadeInterpolator.TargetValue < 1)
        {
            _fadeInterpolator.MoveTo(1, 0.5f);
        }
        else if (!isLoading && _fadeInterpolator.TargetValue > 0)
        {
            _fadeInterpolator.MoveTo(0, 0.5f);
        }

        float fadeValue = _fadeInterpolator.GetValue();
        if (fadeValue <= 0)
        {
            spinnerElement.enabled = false;
            return;
        }

        spinnerElement.enabled = true;

        var fadeColor = new Color(1, 1, 1, fadeValue);
        spinnerElement.color = fadeColor;

        spinnerElement.transform.Rotate(0, 0, -1.7f);
    }
}