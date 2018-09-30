using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Countries.Web.Contracts;
using Countries.Web.Models;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Countries.Web.Services
{
    public class MetadataService : IMetadataService
    {
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
        private readonly IServiceCollection _serviceCollection;

        public MetadataService(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            IServiceCollection serviceCollection)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            _serviceCollection = serviceCollection;
        }

        public Metadata GetMetadata()
        {
            var result = new Metadata();

            var items = _actionDescriptorCollectionProvider.ActionDescriptors.Items;
            for (var i = 0; i < items.Count; i++)
            {
                var actionDescriptor = items[i];
                if (!(actionDescriptor is ControllerActionDescriptor))
                {
                    continue;
                }

                var action = ParseAction(actionDescriptor);
                result.Actions.Add(action);
            }

            return result;
        }

        private static Models.Action ParseAction(Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor actionDescriptor)
        {
            var action = new Models.Action
            {
                Template = actionDescriptor.AttributeRouteInfo.Template,
                Parameters = actionDescriptor.Parameters
                    .Select(p => new Parameter { Name = p.Name, Type = p.ParameterType.FullName })
                    .ToList()
            };

            if (actionDescriptor.ActionConstraints != null)
            {
                for (var i = 0; i < actionDescriptor.ActionConstraints.Count; i++)
                {
                    var actionDescriptorActionConstraint = actionDescriptor.ActionConstraints[i];
                    if (actionDescriptorActionConstraint is HttpMethodActionConstraint httpMethodActionConstraint)
                    {
                        action.Methods.AddRange(httpMethodActionConstraint.HttpMethods);
                    }
                }
            }

            return action;
        }

        public DIMetadata GetDependencyInjectionProblems(IServiceProvider serviceProvider)
        {
            var result = new DIMetadata();

            foreach (var service in _serviceCollection)
            {
                var serviceType = service.ServiceType as System.Type;
                try
                {
                    if (serviceType.ContainsGenericParameters)
                    {
                        result.NotValidatedTypes.Add(new ServiceMetadata { ServiceType = serviceType.ToString(), Reason = "Type ContainsGenericParameters == true" });
                        continue;
                    }
                    var x = serviceProvider.GetService(service.ServiceType);
                    result.ValidTypes.Add(serviceType.ToString());
                }
                catch (Exception e)
                {
                    result.Problems.Add(new ServiceMetadata { ServiceType = serviceType.ToString(), Reason = e.Message });
                }
            }

            return result;
        }
    }
}