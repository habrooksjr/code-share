using dvcsharp_core_api.Sanitization.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;

namespace dvcsharp_core_api.Sanitization.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class SanitizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sanitized = Sanitize(context);

            if (sanitized == false)
            {
                context.Result = new BadRequestObjectResult("Invalid input. Please review input for validity.");
            }

            base.OnActionExecuting(context);
        }

        private bool SanitationDisabled(object[] attributes)
        {
            return attributes != null && attributes.Any(x => x.GetType().Equals(typeof(DisableSanitationAttribute)));
        }

        private bool Sanitize(ActionExecutingContext context)
        {
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;

            if (descriptor != null)
            {
                if (SanitationDisabled(descriptor.ControllerTypeInfo.GetCustomAttributes(true))) return true;

                if (SanitationDisabled(descriptor.MethodInfo.GetCustomAttributes(true))) return true;
            }

            var values = new List<string>();

            if (context.ActionArguments != null && context.ActionArguments.Count > 0)
                values.AddRange(context.ActionArguments
                    .Where(x => x.Value != null)
                    .Select(x =>
                    {
                        if (x.Value.GetType() == typeof(string))
                        {
                            return x.Value.ToString();
                        }
                        else
                        {
                            var resolver = new SanitizeContractResolver();

                            var setting = new JsonSerializerSettings { ContractResolver = resolver };

                            return JsonConvert.SerializeObject(x.Value, setting);
                        }
                    }));

            //var body = GetBody(context);
            //if (string.IsNullOrEmpty(body) == false)
            //    values.Add(body);

            foreach (var value in values)
            {
                if (Validate(value) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private string GetBody(ActionExecutingContext context)
        {
            var req = context.HttpContext.Request;

            if (req.ContentLength == null) return null;

            if (req.ContentLength == 0) return null;

            if (req.Body.CanSeek == false) return null;

            req.EnableBuffering();

            string value = null;
            using (var stream = new StreamReader(req.Body))
            {
                value = stream.ReadToEnd();
            }

            req.Body.Seek(0, SeekOrigin.Begin);

            return value;
        }

        private bool IsJson(string value)
        {
            try
            {
                JObject.Parse(value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool Validate(string value)
        {
            string sanitizdValue = null;

            if (IsJson(value))
            {
                var settings = new JsonSerializerSettings
                {
                    StringEscapeHandling = StringEscapeHandling.EscapeHtml
                };

                sanitizdValue = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(value), settings);
            }
            else
            {
                sanitizdValue = WebUtility.HtmlEncode(value);
            }

            if (value != sanitizdValue)
            {
                return false;
            }

            return true;
        }
    }
}
