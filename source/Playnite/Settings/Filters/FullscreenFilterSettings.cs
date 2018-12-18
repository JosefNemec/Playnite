namespace Playnite
{
    public enum FullscreenFilterEnum
    {
        All,
        Installed,
        Uninstalled
    }

    public class FullscreenFilterSettings: FilterSettings
    {
        private FullscreenFilterEnum fullscreenFilterEnum;

        public FullscreenFilterSettings()
        {
            Favorite = ThreeStateFilterEnum.EnableInclusive;
            Hidden = ThreeStateFilterEnum.Disable;
            fullscreenFilterEnum = FullscreenFilterEnum.All;

            OnFilterToggle();
        }

        public void ToggleFilter()
        {
            switch (fullscreenFilterEnum)
            {
                case FullscreenFilterEnum.All:
                    fullscreenFilterEnum = FullscreenFilterEnum.Installed;
                    break;
                case FullscreenFilterEnum.Installed:
                    fullscreenFilterEnum = FullscreenFilterEnum.Uninstalled;
                    break;
                case FullscreenFilterEnum.Uninstalled:
                    fullscreenFilterEnum = FullscreenFilterEnum.All;
                    break;
            }

            OnFilterToggle();
        }

        private void OnFilterToggle()
        {
            switch (fullscreenFilterEnum)
            {
                case FullscreenFilterEnum.Installed:
                    IsInstalled = ThreeStateFilterEnum.EnableExclusive;
                    IsUnInstalled = ThreeStateFilterEnum.Disable;
                    break;
                case FullscreenFilterEnum.Uninstalled:
                    IsInstalled = ThreeStateFilterEnum.Disable;
                    IsUnInstalled = ThreeStateFilterEnum.EnableExclusive;
                    break;
                default:
                case FullscreenFilterEnum.All:
                    IsInstalled = ThreeStateFilterEnum.EnableInclusive;
                    IsUnInstalled = ThreeStateFilterEnum.EnableInclusive;
                    break;
            }
        }
    }
}