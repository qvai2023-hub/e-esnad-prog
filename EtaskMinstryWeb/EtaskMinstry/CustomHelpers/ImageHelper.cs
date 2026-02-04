using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EtaskMinstry.CustomHelpers
{
    public static class ImageHelper
    {


        public static MvcHtmlString NewImage(this HtmlHelper htmlHelper, string strPath, string src, object htmlAttributes)
        {
            string htmlAttributesString = string.Empty;
            TagBuilder tagBuilder = new TagBuilder("img");
            tagBuilder.MergeAttribute("alt", "d");
            tagBuilder.MergeAttribute("name", "d");
            tagBuilder.MergeAttribute("src", strPath + src);
            //html attributes
            if (htmlAttributes != null)
            {
                RouteValueDictionary d = new RouteValueDictionary(htmlAttributes);
                for (int i = 0; i < d.Keys.Count; i++)
                {
                    //    htmlAttributesString += " " + d.Keys.ElementAt(i) + "=" + d.Values.ElementAt(i);
                    tagBuilder.MergeAttribute(d.Keys.ElementAt(i).ToString(), d.Values.ElementAt(i).ToString());
                }
            }

            return MvcHtmlString.Create(tagBuilder.ToString(TagRenderMode.SelfClosing));
        }

        public static MvcHtmlString Image<TModel, TProperty>
        (this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string strPath, string defaultImage, object htmlAttributes)
        {
            var name = ExpressionHelper.GetExpressionText(expression);
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            if (metadata.Model == null)
            {
                strPath = string.Empty;
                metadata.Model = defaultImage;
            }
            return NewImage(htmlHelper, strPath, metadata.Model as string, htmlAttributes);
        }


    }

}