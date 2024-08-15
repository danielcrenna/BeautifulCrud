using BeautifulCrud.AspNetCore.ActionFilters;

namespace BeautifulCrud.AspNetCore.Attributes;

public class ItemQueryAttribute : CrudFilterAttribute<ProjectActionFilter>;

public class ItemQueryAttribute<T>() : CrudFilterAttribute<ProjectActionFilter>(typeof(T));