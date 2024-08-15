using BeautifulCrud.AspNetCore.Models;
using Microsoft.AspNetCore.Builder;

namespace BeautifulCrud.AspNetCore;

public static class Use
{
    public static WebApplication UseBeautifulCrud(this WebApplication app)
    {
        app.UseMiddleware<NextLinkMiddleware>();
        return app;
    }
}