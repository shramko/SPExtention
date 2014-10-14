using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace SPExtention
{

    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class InternalNameAttribute : System.Attribute
    {
        private string name;
        public InternalNameAttribute(string name)
        {
            this.name = name;
        }
        public virtual string Name
        {
            get { return name; }
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class DescriptionAttribute : System.Attribute
    {
        private string description;
        public DescriptionAttribute(string description)
        {
            this.description = description;
        }
        public virtual string Description
        {
            get { return description; }
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class DisplayNameAttribute : System.Attribute
    {
        private string name;
        public DisplayNameAttribute(string name)
        {
            this.name = name;
        }
        public virtual string Name
        {
            get { return name; }
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class FieldTypeAttribute : System.Attribute
    {
        private string _fieldType;
        public FieldTypeAttribute(SPFieldType fieldType)
        {
            this._fieldType = fieldType.ToString("G");
        }

        public FieldTypeAttribute(string customFieldTypeName)
        {
            this._fieldType = customFieldTypeName;
        }

        public virtual string Type
        {
            get { return _fieldType; }
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class AdditionalFieldAttrAttribute : System.Attribute
    {
        private string _attrName;
        private string _value;
        public AdditionalFieldAttrAttribute(string attrName, string value)
        {
            this._attrName = attrName;
            this._value = value;
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

    [System.AttributeUsage(System.AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class InternalFieldXmlAttribute : System.Attribute
    {
        private string _internalFieldXml;
        public InternalFieldXmlAttribute(string internalFieldXml)
        {
            this._internalFieldXml = internalFieldXml;
        }
        public virtual string InternalXml
        {
            get { return _internalFieldXml; }
        }

    }
}
