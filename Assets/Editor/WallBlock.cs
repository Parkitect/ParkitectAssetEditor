namespace ParkitectAssetEditor
{
    /// <summary>
    /// Which sides of a wall are blocked.
    /// </summary>
    enum WallBlock
    {
	    Back = 1 << 0,
	    Right = 1 << 1,
	    Forward = 1 << 2,
	    Left = 1 << 3
	}
}
