using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace DevTeam.ImageFormatter
{
    public class ImageAttribute : OrderedFilterAttribute.OrderedFilterAttribute
    {
        private Type ImageModelType { get; set; }
        public ImageAttribute(Type imageModelType)
        {
            ImageModelType = imageModelType;
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (!ImageModelType.IsSubclassOf(typeof(BaseImageModel)))
            {
                throw new ConfigurationErrorsException($"You must inheret your '{ImageModelType.FullName}' from 'DevTeam.ImageFormatter.BaseImageModel' class");
            }

            if (actionExecutedContext.Response == null) return;

            var model = actionExecutedContext.Response.Content as ObjectContent;
            if (model == null || !(model.Value.GetType() == ImageModelType)) return;

            var image = model.Value as BaseImageModel;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(image.Content)
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);

            actionExecutedContext.Response = response;

            base.OnActionExecuted(actionExecutedContext);
        }
    }
}
