using BeautifulCrud.AspNetCore.ActionFilters;

namespace BeautifulCrud.AspNetCore.Attributes;

public class CollectionQueryAttribute : CrudFilterAttribute<CollectionQueryActionFilter>;

public class CollectionQueryAttribute<T>() : CrudFilterAttribute<CollectionQueryActionFilter>(typeof(T));