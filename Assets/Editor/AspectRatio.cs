namespace ParkitectAssetEditor
{
    /// <summary>
    /// Which sides of a wall are blocked.
    /// </summary>
    public enum AspectRatio
    {
	    OneOne,
		TwoOne,
		OneTwo
	}

    public static class AspectRatioUtility {
	    public static string[] aspectRatioNames = {"1:1", "2:1", "1:2"};
    }
}
