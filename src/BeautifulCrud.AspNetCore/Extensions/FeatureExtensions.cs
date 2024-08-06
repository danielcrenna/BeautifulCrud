namespace BeautifulCrud.AspNetCore.Extensions;

internal static class FeatureExtensions
{
	public static bool HasFlagFast(this Features value, Features flag)
	{
		return (value & flag) != 0;
	}
}