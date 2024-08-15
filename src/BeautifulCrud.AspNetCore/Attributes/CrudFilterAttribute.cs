using Microsoft.AspNetCore.Mvc;

namespace BeautifulCrud.AspNetCore.Attributes;

public abstract class CrudFilterAttribute<TActionFilter> : ServiceFilterAttribute, IHasType
{
	public Type? Type { get; }

	protected CrudFilterAttribute(Type type) : base(typeof(TActionFilter))
	{ 
		Type = type;
		IsReusable = true;
	}

	protected CrudFilterAttribute() : base(typeof(TActionFilter)) => IsReusable = true;
}