namespace BeautifulCrud;

[Flags]
public enum Features
{
	Controllers = 1,
	MinimalApis = 2,
	OpenApi = 4,
	All = Controllers | MinimalApis | OpenApi
}