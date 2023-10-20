using ETag.Demo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ETag.Demo.Api.Filters
{
    /// <summary>
    /// <see cref="https://referbruv.com/blog/how-to-build-a-simple-etag-in-aspnet-core/"/>
    /// <see cref="https://github.com/KevinDockx/HttpCacheHeaders"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class ETagAttribute : ActionFilterAttribute, IAsyncActionFilter
    {
        public override async Task OnActionExecutionAsync(
            ActionExecutingContext executingContext,
            ActionExecutionDelegate next)
        {
            var request = executingContext.HttpContext.Request;

            var executedContext = await next();

            var response = executedContext.HttpContext.Response;

            // Computing ETags for Response Caching on GET requests
            if (request.Method == HttpMethod.Get.Method
                && response.StatusCode == (int)HttpStatusCode.OK)
            {
                ValidateETagForResponseCaching(executedContext);
            }
        }

        private static void ValidateETagForResponseCaching(ActionExecutedContext executedContext)
        {
            if (executedContext.Result == null)
            {
                return;
            }

            var request = executedContext.HttpContext.Request;
            var response = executedContext.HttpContext.Response;
            var result = (executedContext.Result as ObjectResult)?.Value;

            // generates ETag from the entire response Content
            var content = JsonSerializer.Serialize(result);
            var eTag = ETagGenerator.GetETag(request.Path.ToString(), Encoding.UTF8.GetBytes(content));

            if (request.Headers.ContainsKey(HeaderNames.IfNoneMatch))
            {
                // fetch etag from the incoming request header
                var incomingEtag = request.Headers[HeaderNames.IfNoneMatch].ToString();

                // if both the etags are equal
                // raise a 304 Not Modified Response
                if (ETagsMatch(eTag, incomingEtag, false))
                {
                    executedContext.Result = new StatusCodeResult((int)HttpStatusCode.NotModified);
                }
            }

            // add ETag response header
            response.Headers.Add(HeaderNames.ETag, new[] { eTag.ToString() });
        }

        private static bool ETagsMatch(
            Models.ETag eTag,
            string eTagToCompare,
            bool useStrongComparisonFunction)
        {
            // for If-None-Match (cache) checks, weak comparison should be used.
            // for If-Match (concurrency) check, strong comparison should be used.

            //The example below shows the results for a set of entity-tag pairs and
            //both the weak and strong comparison function results:

            //+--------+--------+-------------------+-----------------+
            //| ETag 1 | ETag 2 | Strong Comparison | Weak Comparison |
            //+--------+--------+-------------------+-----------------+
            //| W/"1"  | W/"1"  | no match          | match           |
            //| W/"1"  | W/"2"  | no match          | no match        |
            //| W/"1"  | "1"    | no match          | match           |
            //| "1"    | "1"    | match             | match           |
            //+--------+--------+-------------------+-----------------+

            if (useStrongComparisonFunction)
            {
                // to match, both eTags must be strong & be an exact match.
                var eTagToCompareIsStrong = !eTagToCompare.StartsWith("W/");

                return eTagToCompareIsStrong &&
                       eTag.ETagType == ETagType.Strong &&
                       string.Equals(eTag.ToString(), eTagToCompare, StringComparison.OrdinalIgnoreCase);
            }

            // for weak comparison, we only compare the parts of the eTags after the "W/"
            var firstValueToCompare = eTag.ETagType == ETagType.Weak ? eTag.ToString()[2..] : eTag.ToString();
            var secondValueToCompare = eTagToCompare.StartsWith("W/") ? eTagToCompare[2..] : eTagToCompare;

            return string.Equals(firstValueToCompare, secondValueToCompare, StringComparison.OrdinalIgnoreCase);
        }
    }
}
