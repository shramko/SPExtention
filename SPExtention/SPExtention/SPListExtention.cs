using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.SharePoint;

namespace SPExtention
{
    // ReSharper disable once InconsistentNaming
    public abstract class SPListExtention<T> where T : SPListExtention<T>
    {
        private readonly SPWeb _spWeb;
        private SPList _spList;

        #region Constructor
        protected SPListExtention() { }

        protected SPListExtention(SPWeb spWeb)
        {
            _spWeb = spWeb;
            PropertyInfo[] properties = typeof (T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
            foreach (PropertyInfo prop in properties)
            {
                prop.SetValue(this, prop.Name);
            }
        }
        #endregion

        #region Public instance

        public object Create()
        {
            return Create(_spWeb);
        }

        public SPList GetSPList()
        {
            return GetSPList(_spWeb);
        }

        #endregion

        #region Public static
        public static string ListDisplayName
        {
            get
            {
                DisplayNameAttribute dAttribute =
                    (DisplayNameAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(DisplayNameAttribute));
                if (dAttribute != null)
                    return dAttribute.Name;
                return string.Empty;
            }
        }

        public static string ListInternalName
        {
            get
            {
                InternalNameAttribute iAttribute =
                    (InternalNameAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(InternalNameAttribute));
                if (iAttribute != null)
                    return iAttribute.Name;
                return string.Empty;
            }

        }

        public static string ListDescription
        {
            get
            {
                DescriptionAttribute descriptionAttribute =
                    (DescriptionAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(DescriptionAttribute));
                if (descriptionAttribute != null)
                    return descriptionAttribute.Description;
                return string.Empty;
            }

        }

        public static object Create(SPWeb spWeb)
        {
            if (spWeb == null) 
                return new Exception("SPWeb not defined");

            if (string.IsNullOrEmpty(ListDisplayName) || string.IsNullOrEmpty(ListInternalName))
                return new Exception("Display name or internal name not defined");

            var addLResult = AddListToWeb(spWeb);
            if (addLResult is Exception)
                return addLResult;

            var addFResult = AddFieldsToList(addLResult as SPList);
            if (addFResult is Exception)
                return addFResult;

            return addLResult as SPList;
        }

        public static object UpdateFields(SPWeb spWeb)
        {
            if (string.IsNullOrEmpty(ListDisplayName) || string.IsNullOrEmpty(ListInternalName))
                return new Exception("Display name or internal name not defined");

            var existList = GetSPList(spWeb);
            if (existList == null)
                return new Exception(string.Format("List with name {0} NOT exist", ListDisplayName + " || " + ListInternalName));
            return UpdateListFields(existList);
        }

        public static SPList GetSPListByInternalName(SPWeb spWeb)
        {
            if (string.IsNullOrEmpty(ListInternalName))
                return null;
            SPList spList = (from SPList l in spWeb.Lists
                             where l.RootFolder.Name.Equals(ListInternalName, StringComparison.InvariantCulture)
                             select l).FirstOrDefault();
            return spList;
        }

        public static SPList GetSPListByDisplayName(SPWeb spWeb)
        {
            if (string.IsNullOrEmpty(ListDisplayName))
                return null;
            var spList = spWeb.Lists.TryGetList(ListDisplayName);
            return spList;
        }

        public static SPList GetSPList(SPWeb spWeb)
        {
            if(spWeb == null) return null;
            return GetSPListByInternalName(spWeb) ?? GetSPListByDisplayName(spWeb);
        }
        
        public static List<SPField> GetCustomFields(SPWeb spWeb)
        {
            return GetCustomFieldList(GetSPList(spWeb));
        }

        #endregion

        #region Private

        private static object AddListToWeb(SPWeb spWeb)
        {
            if (GetSPListByDisplayName(spWeb) != null)
                return new Exception(string.Format("List with name {0} exist", ListDisplayName));
            if (GetSPListByInternalName(spWeb) != null)
                return new Exception(string.Format("List with internal name {0} exist", ListInternalName));

            Guid listGuid = spWeb.Lists.Add(ListInternalName, ListDescription, SPListTemplateType.GenericList);
            SPList list = spWeb.Lists[listGuid];
            list.Title = ListDisplayName;
            list.Update();
            return list;
        }

        private static object AddFieldsToList(SPList spList)
        {
            try
            {
                PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
                foreach (PropertyInfo prop in properties)
                {
                    var dispName = prop.GetCustomAttribute<DisplayNameAttribute>();
                    var fieldType = prop.GetCustomAttribute<FieldTypeAttribute>();
                    var internalXml = prop.GetCustomAttribute<InternalFieldXmlAttribute>();
                    var additionalAttr = prop.GetCustomAttributes<AdditionalFieldAttrAttribute>();
                    var toDefaultView = prop.GetCustomAttribute<DefaultViewAttribute>() != null;
                    var isRequired = prop.GetCustomAttribute<RequiredAttribute>() != null;
                    string parsedAdAttr = ParseAdditionalAttributes(additionalAttr);
                    if (fieldType == null) continue;
                    AddFieldToList( spList, 
                                    fieldType.Type, 
                                    prop.Name, 
                                    dispName != null ? dispName.Name : null,
                                    parsedAdAttr,
                                    internalXml != null ? internalXml.InternalXml : string.Empty,
                                    toDefaultView,
                                    isRequired);
                }
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private const string FieldXmlFormat =
            "<Field DisplayName='{0}' StaticName='{1}' Name='{1}' ID='{{{2}}}' Type='{3}' {4}>{5}</Field>";
        private static SPField AddFieldToList(SPList spList, string type, string internalName, string displayName, string additionalAttr, string innerXml, bool defaultView, bool isRequired)
        {
            if (isRequired)
                additionalAttr += " Required = 'TRUE' ";

            var fieldXml = string.Format(FieldXmlFormat,
                                        !string.IsNullOrEmpty(displayName) ? displayName:internalName, 
                                        internalName, 
                                        Guid.NewGuid(), 
                                        type, 
                                        additionalAttr,
                                        innerXml);
            var strInternalName = spList.Fields.AddFieldAsXml(fieldXml, defaultView, SPAddFieldOptions.AddFieldInternalNameHint);
            var field = spList.Fields.GetFieldByInternalName(strInternalName);
            field.Update();
            return field;
        }

        private static string ParseAdditionalAttributes(IEnumerable<AdditionalFieldAttrAttribute> additionalFieldAttr)
        {
            if (additionalFieldAttr == null) return string.Empty;
            StringBuilder sb = new StringBuilder();
            foreach (AdditionalFieldAttrAttribute attr in additionalFieldAttr)
            {
                sb.AppendFormat("{0} ='{1}' ", attr.AttributeName, attr.Value);
            }
            return sb.ToString();
        }

        private static object UpdateListFields(SPList spList)
        {
            try
            {
                List<string> existFields = GetCustomFieldInternalNameList(spList);
                List<string> propNames = new List<string>();
                PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
                foreach (PropertyInfo prop in properties)
                {
                    var dispName = prop.GetCustomAttribute<DisplayNameAttribute>();
                    var fieldType = prop.GetCustomAttribute<FieldTypeAttribute>();
                    var internalXml = prop.GetCustomAttribute<InternalFieldXmlAttribute>();
                    var additionalAttr = prop.GetCustomAttributes<AdditionalFieldAttrAttribute>();
                    var toDefaultView = prop.GetCustomAttribute<DefaultViewAttribute>() != null;
                    var isRequired = prop.GetCustomAttribute<RequiredAttribute>() != null;
                    if (fieldType == null) continue;

                    var additionalFieldAttr = additionalAttr as IList<AdditionalFieldAttrAttribute> ?? additionalAttr.ToList();
                    
                    if (existFields.Contains(prop.Name))
                    {
                        AddFieldAttribute(spList, prop.Name, "DisplayName", dispName.Name);
                        AddFieldAttribute(spList, prop.Name, "Type", fieldType.Type);
                        if (additionalAttr != null && additionalFieldAttr.Any())
                            AddFieldAttribute(spList, prop.Name, additionalFieldAttr);
                    }
                    else
                    {
                        string attributes = ParseAdditionalAttributes(additionalFieldAttr);
                        AddFieldToList( spList, 
                                        fieldType.Type, 
                                        prop.Name,
                                        dispName != null ? dispName.Name : null, 
                                        attributes,
                                        internalXml != null ? internalXml.InternalXml : string.Empty,
                                        toDefaultView,
                                        isRequired);
                    }
                    propNames.Add(prop.Name);
                }
                
                if (propNames.Count == existFields.Count) return null;
                foreach (string ef in existFields.Where(ef => !propNames.Contains(ef)))
                    DeleteFields(spList, ef);

                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
        
        private static object AddFieldAttribute(SPList spList, string fieldInternalName, IEnumerable<AdditionalFieldAttrAttribute> additionalFieldAttr)
        {
            SPField spField = spList.Fields.TryGetFieldByStaticName(fieldInternalName);
            if (spField == null)
                return new Exception(string.Format("Field with name {0} NOT exist", fieldInternalName));
            foreach (var attr in additionalFieldAttr)
            {
                AddFieldAttribute(spList, fieldInternalName, attr.AttributeName, attr.Value);
            }
            return null;
        }

        private static object AddFieldAttribute(SPList spList, string fieldInternalName, string key, string value)
        {
            SPField spField = spList.Fields.TryGetFieldByStaticName(fieldInternalName);
            if (spField == null)
                return new Exception(string.Format("Field with name {0} NOT exist", fieldInternalName));
            return AddFieldAttribute(spField, key, value);
        }

        private static object AddFieldAttribute(SPField spField, string key, string value)
        {
            try
            {
                var fieldSchema = XDocument.Parse(spField.SchemaXml);
                var tabAttribute = fieldSchema.Element(@"Field").Attribute(key);
                if (tabAttribute == null)
                    fieldSchema.Element("Field").Add(new XAttribute(key, value));
                else
                    tabAttribute.Value = value;
                spField.SchemaXml = fieldSchema.ToString();
                spField.PushChangesToLists = true;
                spField.Update();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private static void DeleteFields(SPList spList, string internalFieldName)
        {
            spList.Fields.Delete(internalFieldName);
        }

        private static List<string> GetCustomFieldInternalNameList(SPList spList)
        {
            if (spList == null) return null;
            return (spList.Fields.Cast<SPField>()
                .Where(field => !SPBuiltInFieldId.Contains(field.Id) && !field.SourceId.StartsWith("http://"))
                .Select(field => field.StaticName)).ToList();
        }


        private static List<SPField> GetCustomFieldList(SPList spList)
        {
            if(spList == null) return null;
            return (spList.Fields.Cast<SPField>()
                .Where(field => !SPBuiltInFieldId.Contains(field.Id) && !field.SourceId.StartsWith("http://"))).ToList();
        }

        #endregion
    }


}
