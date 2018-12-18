namespace Playnite
{
    public static class ThreeStateFilterEvaluator
    {
        public static bool EvaluateFilter(ThreeStateFilterEnum filterState, bool gameFilterState)
        {
            switch (filterState)
            {
                case ThreeStateFilterEnum.Disable:
                    if (gameFilterState)
                    {
                        return false;
                    }
                    break;
                case ThreeStateFilterEnum.EnableExclusive:
                    if (!gameFilterState)
                    {
                        return false;
                    }
                    break;
                case ThreeStateFilterEnum.EnableInclusive:
                default:
                    return true;
            }

            return true;
        }
    }
}