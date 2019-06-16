using Audio;
using UnityEngine;
using Utils;

namespace Game.Frontend
{
    public class ClientFrontend : MonoBehaviour
    {
        public enum MenuShowing
        {
            None,
            Main,
            InGame
        }

        private readonly Interpolator _menuFader = new Interpolator(0.0f, Interpolator.CurveType.SmoothStep);
        private MenuShowing _menuShowing;
        [SerializeField] private SoundDef uiCloseSound;
        [SerializeField] private SoundDef uiSelectLightSound;

        public void ShowMenu(MenuShowing show, float fadeDuration = 0)
        {
            if (_menuShowing == show)
                return;
            _menuShowing = show;
            _menuFader.MoveTo(show != MenuShowing.None ? 1.0f : 0.0f, fadeDuration);
            Main.GameRoot.SoundSystem.Play(_menuShowing != MenuShowing.None ? uiSelectLightSound : uiCloseSound);
        }
    }
}