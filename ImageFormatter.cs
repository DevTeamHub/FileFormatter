using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;

namespace DevTeam.ImageFormatter
{
    public class ImageFormatter : MediaTypeFormatter
    {
        public ImageFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("image/jpeg"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("image/jpg"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("image/png"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("multipart/form-data"));
        }

        public override bool CanReadType(Type type)
        {
            return typeof(ImageContentList).IsAssignableFrom(type);
        }

        public override bool CanWriteType(Type type)
        {
            return false;
        }

        public async override Task<object> ReadFromStreamAsync(Type type, Stream stream, HttpContent content, IFormatterLogger logger)
        {
            if (!content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = new MultipartMemoryStreamProvider();
            var formData = await content.ReadAsMultipartAsync(provider);

            var imageContent = formData.Contents
                .Where(c => SupportedMediaTypes.Contains(c.Headers.ContentType))
                .Select(i => ReadContent(i).Result)
                .ToList();

            var jsonContent = formData.Contents
                .Where(c => !SupportedMediaTypes.Contains(c.Headers.ContentType))
                .Select(j => ReadJson(j).Result)
                .ToDictionary(x => x.Key, x => x.Value);

            var json = JsonConvert.SerializeObject(jsonContent);
            var model = JsonConvert.DeserializeObject(json, type) as ImageContentList;

            if (model == null)
            {
                throw new HttpResponseException(HttpStatusCode.NoContent);
            }

            model.Images = imageContent.Select(i => i.Value).ToList();
            //imageContent.ForEach(i => model.GetType().GetProperty(i.Key).SetValue(model, i.Value, null));

            return model;
        }

        private async Task<KeyValuePair<string, ImageContent>> ReadContent(HttpContent content)
        {
            var name = content.Headers.ContentDisposition.Name.Replace("\"", string.Empty);
            var data = await content.ReadAsByteArrayAsync();
            var image = new ImageContent
            {
                Content = data,
                ContentType = content.Headers.ContentType.MediaType,
                Name = content.Headers.ContentDisposition.FileName
            };
            return new KeyValuePair<string, ImageContent>(name, image);
        }

        private async Task<KeyValuePair<string, object>> ReadJson(HttpContent content)
        {
            var name = content.Headers.ContentDisposition.Name.Replace("\"", string.Empty);
            var value = await content.ReadAsStringAsync();

            if (value.ToLower() == "null")
                value = null;

            return new KeyValuePair<string, object>(name, value);
        }
    }
}