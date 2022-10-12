using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using AutoMapper.Internal;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Subscription.Api
{
    public static class SwaggerSetup
    {
        public class AddCommonParamOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                if (operation.Parameters == null) operation.Parameters = new List<OpenApiParameter>();

                var descriptor = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;

                if (descriptor != null && !descriptor.ControllerName.StartsWith("Weather"))
                {
                    operation.Parameters.Add(new OpenApiParameter()
                    {
                        Name = "timestamp",
                        In = ParameterLocation.Query,
                        Description = "The timestamp of now",
                        Required = true
                    });

                }
            }
        }

        public class RemoveJsonIgnoreFromQueryOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                if (context.ApiDescription == null || operation.Parameters == null)
                    return;

                if (!context.ApiDescription.ParameterDescriptions.Any())
                    return;

                context.ApiDescription.ParameterDescriptions.Where(p => p.Source.Equals(BindingSource.Form)
                            && p.CustomAttributes().Any(p => p.GetType().Equals(typeof(JsonIgnoreAttribute))))
                    .ForAll(p => operation.RequestBody.Content.Values.Single(v => v.Schema.Properties.Remove(p.Name)));

                context.ApiDescription.ParameterDescriptions.Where(p => p.Source.Equals(BindingSource.Query)
                              && p.CustomAttributes().Any(p => p.GetType().Equals(typeof(JsonIgnoreAttribute))))
                    .ForAll(p => operation.Parameters.Remove(operation.Parameters.Single(w => w.Name.Equals(p.Name))));


            }
        }

    }
}
