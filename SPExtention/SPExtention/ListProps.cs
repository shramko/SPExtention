using System;

namespace SPExtention
{
    public abstract class ListProps
    {
        public virtual string DisplayName => "";

        public virtual string InternalName => "";

        public virtual string Description => "";

        public virtual string ContentTypeId => "";

        public virtual string ContentTypeName => "";

        public virtual bool IsHiddenList => false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="L">Class marked with attributes</typeparam>
    public class ListPropsFromAttributes<L> : ListProps
    {
        public override string DisplayName
        {
            get
            {
                DisplayNameAttribute dAttribute =
                    (DisplayNameAttribute)Attribute.GetCustomAttribute(typeof(L), typeof(DisplayNameAttribute));
                if (dAttribute != null)
                    return dAttribute.Name;
                return string.Empty;
            }
        }

        public override string InternalName
        {
            get
            {
                InternalNameAttribute iAttribute =
                    (InternalNameAttribute)Attribute.GetCustomAttribute(typeof(L), typeof(InternalNameAttribute));
                if (iAttribute != null)
                    return iAttribute.Name;
                return string.Empty;
            }

        }

        public override string Description
        {
            get
            {
                DescriptionAttribute descriptionAttribute =
                    (DescriptionAttribute)Attribute.GetCustomAttribute(typeof(L), typeof(DescriptionAttribute));
                if (descriptionAttribute != null)
                    return descriptionAttribute.Description;
                return string.Empty;
            }

        }

        public override string ContentTypeId
        {
            get
            {
                ContentTypeIdAttribute ct =
                    (ContentTypeIdAttribute)Attribute.GetCustomAttribute(typeof(L), typeof(ContentTypeIdAttribute));
                if (ct != null)
                    return ct.ContentTypeId;
                return string.Empty;
            }
        }

        public override string ContentTypeName
        {
            get
            {
                ContentTypeNameAttribute ct =
                    (ContentTypeNameAttribute)Attribute.GetCustomAttribute(typeof(L), typeof(ContentTypeNameAttribute));
                if (ct != null)
                    return ct.ContentTypeName;
                return string.Empty;
            }
        }

        public override bool IsHiddenList
        {
            get
            {
                HiddenListAttribute hl =
                    (HiddenListAttribute)Attribute.GetCustomAttribute(typeof(L), typeof(HiddenListAttribute));
                if (hl != null)
                    return true;
                return false;
            }
        }
    }
}
