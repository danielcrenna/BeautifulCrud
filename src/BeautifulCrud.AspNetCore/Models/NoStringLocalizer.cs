using Microsoft.Extensions.Localization;

namespace BeautifulCrud.AspNetCore.Models;

internal sealed class NoStringLocalizer<T> : IStringLocalizer<T>
{
    public LocalizedString this[string name] => new(name, name);
    public LocalizedString this[string name, params object[] arguments] => new(name, name);
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
}