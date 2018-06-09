using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Core.DomainModel;
using System.ComponentModel;
using System.Web.Mvc;

namespace Store.Core
{
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class RenderModeAttribute : System.Attribute
    {
        public RenderMode RenderMode { get; set; }

        public RenderModeAttribute(RenderMode renderMode)
        {
            RenderMode = renderMode;
        }
    }

    public enum RenderMode
    {
        Any,
        None,
        EditModeOnly,
        DisplayModeOnly
    }


   public class CustomModelMetadataProvider : DataAnnotationsModelMetadataProvider
    {
        protected override ModelMetadata CreateMetadata(System.Collections.Generic.IEnumerable<System.Attribute> attributes, System.Type containerType, System.Func<object> modelAccessor, System.Type modelType, string propertyName)
        {
            var metadata = base.CreateMetadata(attributes, containerType, modelAccessor, modelType, propertyName);

            var renderModeAttribute = attributes.OfType<RenderModeAttribute>();
            if (renderModeAttribute.Any())
            {
                var renderMode = renderModeAttribute.First().RenderMode;
                switch (renderMode)
                {
                    case RenderMode.DisplayModeOnly:
                        metadata.ShowForDisplay = true;
                        metadata.ShowForEdit = false;
                        break;

                    case RenderMode.EditModeOnly:
                        metadata.ShowForDisplay = false;
                        metadata.ShowForEdit = true;
                        break;

                    case RenderMode.None:
                        metadata.ShowForDisplay = false;
                        metadata.ShowForEdit = false;
                        break;

                    case RenderMode.Any:
                        metadata.ShowForDisplay = true;
                        metadata.ShowForEdit = true;
                        break;

                }
            }

            return metadata;
        }
    }}
