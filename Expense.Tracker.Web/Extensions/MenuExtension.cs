using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Html;


public class MenuGroup : IDisposable
{
    private readonly TextWriter textWriter;

    public MenuGroup(TextWriter writer)
    {
        this.textWriter = writer;
    }

    public void Dispose()
    {
        this.textWriter.Write("</ul>");
        this.textWriter.Write("</li>");
    }
}
public static class MenuExtension
{

    public static MvcHtmlString MenuHeader(this HtmlHelper helper, string menuText)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("<li class=\"heading\">");
        builder.Append("<h3 class=\"uppercase\">");
        builder.Append(menuText);
        builder.Append("</h3>");
        builder.Append("</li>");
        return new MvcHtmlString(builder.ToString());
    }

    public static MvcHtmlString MenuActionLink(this HtmlHelper helper, string linkText, string action, string controller,
                                    string ancherClass, string icon,
                                     int? badgeValue, string badgeStyle)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<li class=\"nav-item start\" for=\"{0}-{1}\">", controller, action);
        builder.AppendFormat("<a href=\"/{0}/{1}\" class=\"{2}\">", controller, action, ancherClass);
        builder.AppendFormat("<i class=\"{0}\"></i>", icon);
        builder.AppendFormat("<span class=\"title\">{0}</span>", linkText);
        builder.Append("<span class=\"selected\"></span>");
        builder.Append("</a>");
        builder.Append("</li>");
        return new MvcHtmlString(builder.ToString());
    }

    public static IDisposable MenuGroupActionLink(this HtmlHelper helper, string linkText, string anchorClass, string icon)
    {
        var writer = helper.ViewContext.Writer;

        StringBuilder builder = new StringBuilder();
        builder.Append("<li class=\"nav-item start \">");
        builder.AppendFormat("<a href=\"#\" class=\"{0}\">", anchorClass);
        if (!String.IsNullOrEmpty(icon))
            builder.AppendFormat("<i class=\"{0}\"></i>", icon);
        builder.AppendFormat("<span class=\"title\">{0}</span>", linkText);
        builder.Append("<span class=\"selected\"></span>");
        builder.Append("</a>");
        builder.Append("<ul class=\"sub-menu\">");
        writer.Write(builder.ToString());

        return new MenuGroup(writer);
    }

}
