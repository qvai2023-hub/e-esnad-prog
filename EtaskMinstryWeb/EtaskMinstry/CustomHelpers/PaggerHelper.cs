using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EtaskMinstry.CustomHelpers
{
    public static class PaggerHelper
    {
        public static MvcHtmlString Pager(this HtmlHelper helper, int currentPage, int pageSize, int totalItemCount, object routeValues)
        {
            // how many pages to display in each page group const   
            int cGroupSize = 5;
            var pageCount = (int)Math.Ceiling(totalItemCount / (double)pageSize);

            // cleanup any out bounds page number passed    
            currentPage = Math.Max(currentPage, 1);
            currentPage = Math.Min(currentPage, pageCount);

            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext, helper.RouteCollection);
            var container = new TagBuilder("div");
            container.AddCssClass("pager");
            var actionName = helper.ViewContext.RouteData.GetRequiredString("Action");

            // calculate the last page group number starting from the current page      
            // until we hit the next whole divisible number     
            var lastGroupNumber = currentPage;
            while ((lastGroupNumber % cGroupSize != 0)) lastGroupNumber++;

            // correct if we went over the number of pages      
            var groupEnd = Math.Min(lastGroupNumber, pageCount);

            // work out the first page group number, we use the lastGroupNumber instead of      
            // groupEnd so that we don't include numbers from the previous group if we went     
            // over the page count      
            var groupStart = lastGroupNumber - (cGroupSize - 1);

            // if we are past the first page    
            if (currentPage > 1)
            {
                var previous = new TagBuilder("a");
                previous.SetInnerText("<");
                previous.AddCssClass("previous");
                var routingValues = new RouteValueDictionary(routeValues);
                routingValues.Add("page", currentPage - 1);
                previous.MergeAttribute("href", urlHelper.Action(actionName, routingValues));
                container.InnerHtml += previous.ToString();
            }

            // if we have past the first page group     
            if (currentPage > cGroupSize)
            {
                var previousDots = new TagBuilder("a");
                previousDots.SetInnerText("...");
                previousDots.AddCssClass("previous-dots");
                var routingValues = new RouteValueDictionary(routeValues);
                routingValues.Add("page", groupStart - cGroupSize);
                previousDots.MergeAttribute("href", urlHelper.Action(actionName, routingValues));
                container.InnerHtml += previousDots.ToString();
            }

            for (var i = groupStart; i <= groupEnd; i++)
            {
                var pageNumber = new TagBuilder("a");
                pageNumber.AddCssClass(((i == currentPage)) ? "selected-page" : "page");
                pageNumber.SetInnerText((i).ToString());
                var routingValues = new RouteValueDictionary(routeValues);
                routingValues.Add("page", i);
                pageNumber.MergeAttribute("href", urlHelper.Action(actionName, routingValues));
                container.InnerHtml += pageNumber.ToString();
            }

            // if there are still pages past the end of this page group     
            if (pageCount > groupEnd)
            {
                var nextDots = new TagBuilder("a");
                nextDots.SetInnerText("...");
                nextDots.AddCssClass("next-dots");
                var routingValues = new RouteValueDictionary(routeValues);
                routingValues.Add("page", groupEnd + 1);
                nextDots.MergeAttribute("href", urlHelper.Action(actionName, routingValues));
                container.InnerHtml += nextDots.ToString();
            }

            // if we still have pages left to show      
            if (currentPage < pageCount)
            {
                var next = new TagBuilder("a");
                next.SetInnerText(">");
                next.AddCssClass("next");
                var routingValues = new RouteValueDictionary(routeValues);
                routingValues.Add("page", currentPage + 1);
                next.MergeAttribute("href", urlHelper.Action(actionName, routingValues));
                container.InnerHtml += next.ToString();
            }

            return MvcHtmlString.Create(container.ToString());
        }
    }
}