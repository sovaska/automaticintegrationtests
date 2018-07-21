using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Countries.Web.Contracts;
using Countries.Web.Models;

namespace Countries.Web.Services
{
    public class MetadataService : IMetadataService
    {
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

        public MetadataService(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
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

        private static Action ParseAction(Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor actionDescriptor)
        {
            var action = new Action
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
    }
}