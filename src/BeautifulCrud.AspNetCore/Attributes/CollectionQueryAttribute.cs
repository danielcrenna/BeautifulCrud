using BeautifulCrud.AspNetCore.ActionFilters;

namespace BeautifulCrud.AspNetCore.Attributes;

public class CollectionQueryAttribute : CrudFilterAttribute<CollectionQueryActionFilter>
{
    public CollectionQueryAttribute() { }
    public CollectionQueryAttribute(Type type) : base(type) { }
}

public class CollectionQueryAttribute<T>() : CollectionQueryAttribute(typeof(T));