/*
 Created by Aleksandr Shramko (ashramko@live.com)
 */

using System;
using Microsoft.SharePoint;

namespace SPExtention
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class InternalNameAttribute : Attribute
    {
        private readonly string _name;
        public InternalNameAttribute(string name)
        {
            _name = name;
        }
        public virtual string Name
        {
            get { return _name; }
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class DescriptionAttribute : Attribute
    {
        private readonly string description;
        public DescriptionAttribute(string description)
        {
            this.description = description;
        }
        public virtual string Description
        {
            get { return description; }
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class DisplayNameAttribute : Attribute
    {
        private readonly string _name;
        public DisplayNameAttribute(string name)
        {
            _name = name;
        }
        public virtual string Name
        {
            get { return _name; }
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class FieldTypeAttribute : Attribute
    {
        private readonly string _fieldType;
        public FieldTypeAttribute(SPFieldType fieldType)
        {
            _fieldType = fieldType.ToString("G");
        }

        public FieldTypeAttribute(string customFieldTypeName)
        {
            _fieldType = customFieldTypeName;
        }

        public virtual string Type
        {
            get { return _fieldType; }
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class AdditionalFieldAttrAttribute : Attribute
    {
        private readonly string _attrName;
        private readonly string _value;
        public AdditionalFieldAttrAttribute(string attrName, string value)
        {
            _attrName = attrName;
            _value = value;
        }
        public virtual string AttributeName
        {
            get { return _attrName; }
        }

        public virtual string Value
        {
            get { return _value; }
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class InternalFieldXmlAttribute : Attribute
    {
        private readonly string _internalFieldXml;
        public InternalFieldXmlAttribute(string internalFieldXml)
        {
            _internalFieldXml = internalFieldXml;
        }
        public virtual string InternalXml
        {
            get { return _internalFieldXml; }
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class DefaultViewAttribute : Attribute
    {
        public DefaultViewAttribute() { }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class RequiredAttribute : Attribute
    {
        public RequiredAttribute() { }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class ContentTypeIdAttribute : Attribute
    {
        private readonly string _contentTypeId;

        public ContentTypeIdAttribute(string contentTypeId)
        {
            _contentTypeId = contentTypeId;
        }

        public virtual string ContentTypeId
        {
            get { return _contentTypeId; }
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class ContentTypeNameAttribute : Attribute
    {
        private readonly string _contentTypeName;

        public ContentTypeNameAttribute(string contentTypeId)
        {
            _contentTypeName = contentTypeId;
        }

        public virtual string ContentTypeName
        {
            get { return _contentTypeName; }
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class HiddenListAttribute : Attribute
    {
        public HiddenListAttribute() { }
    }
}
