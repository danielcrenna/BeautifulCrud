using BeautifulCrud.AspNetCore.ActionFilters;

namespace BeautifulCrud.AspNetCore.Attributes;

public class ItemQueryAttribute : CrudFilterAttribute<ProjectActionFilter>
{
    public ItemQueryAttribute() { }

    public ItemQueryAttribute(Type type) : base(type) { }
}

public class ItemQueryAttribute<T>() : ItemQueryAttribute(typeof(T));