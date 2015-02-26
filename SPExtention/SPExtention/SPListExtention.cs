/*
 Created by Aleksandr Shramko (ashramko@live.com)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Microsoft.SharePoint;

namespace SPExtention
{
    // ReSharper disable once InconsistentNaming
    public abstract class SPListExtention<T> where T : SPListExtention<T>
    {
        private readonly SPWeb _spWeb;

    #region Constructor

        /// <summary>
        /// This constuctor set string value to property = field internal name 
        /// </summary>
        protected SPListExtention()
        {
            foreach (PropertyInfo prop in PropertyFields)
            {
                prop.SetValue(this, prop.Name);
            }
        }

        /// <summary>
        /// This constuctor set SPWeb for instance methods 
        /// </summary>
        /// <param name="spWeb"></param>
        protected SPListExtention(SPWeb spWeb):this()
        {
            _spWeb = spWeb;
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

        public object UpdateFields(bool removeOldFields = false)
        {
            return UpdateFields(_spWeb, removeOldFields);
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

            return addLResult as SPList;
        }

        public static object UpdateFields(SPWeb spWeb, bool removeOldFields = false)
        {
            if (string.IsNullOrEmpty(ListDisplayName) || string.IsNullOrEmpty(ListInternalName))
                return new Exception("Display name or internal name not defined");

            var existList = GetSPList(spWeb);
            if (existList == null)
                return new Exception(string.Format("List with name {0} NOT exist", ListDisplayName + " || " + ListInternalName));
            return UpdateListFields(existList, removeOldFields);
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

        public static object SaveAsTemplate(SPWeb spWeb, string templateName, string description = "",
            bool saveData = false)
        {
            SPList spList = GetSPList(spWeb);
            if (spList == null)
                return new Exception(string.Format("Can't create template. List instance {0} {1} not found", ListInternalName, ListDisplayName));
            string fileName = templateName.Replace(" ", "") + ".stp";
            try
            {
                spList.SaveAsTemplate(fileName, templateName, description, saveData);
                spList.Update();
            }
            catch (Exception ex)
            {
                return ex;
            }
            return null;
        }

        public static List<SPField> GetCustomFields(SPWeb spWeb)
        {
            return GetCustomFieldList(GetSPList(spWeb));
        }

    #endregion

    #region Private

        private static PropertyInfo[] PropertyFields
        {
            get
            {
                return typeof (T).GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.GetProperty);
            }
        }


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
            var result = !string.IsNullOrEmpty(ContentTypeId) 
                ? AddContentTypeToList(list) 
                : AddFieldsToList(list);

            if (result is Exception) 
                return result as Exception;

            return list;
        }

        private static object AddFieldsToList(SPList spList)
        {
            try
            {
                foreach (PropertyInfo prop in PropertyFields)
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

        private static object UpdateListFields(SPList spList, bool removeOldField)
        {
            if (string.IsNullOrEmpty(ContentTypeId))
                return UpdateListInstanceFields(spList, removeOldField);
            UpdateContentTypeListFields(spList, removeOldField);
            return null;
        }

        private static object UpdateListInstanceFields(SPList spList, bool removeOldFields)
        {
            try
            {
                List<string> existFields = GetCustomFieldInternalNameList(spList);
                List<string> propNames = new List<string>();
                foreach (PropertyInfo prop in PropertyFields)
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
                        AddFieldToList(spList,
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

                if (!removeOldFields) return null;
                
                foreach (string ef in existFields.Where(ef => !propNames.Contains(ef)))
                    DeleteFields(spList, ef);
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private static void UpdateContentTypeListFields(SPList spList, bool removeOldField)
        {
            SPContentType newContentType = spList.ParentWeb.AvailableContentTypes[new SPContentTypeId(ContentTypeId)];
            SPContentType listContentType = spList.ContentTypes.Cast<SPContentType>().FirstOrDefault(c => c.Id.ToString().StartsWith(ContentTypeId));
            if (newContentType == null || listContentType==null) return;

            //add new fields and update exist
            foreach (SPField field in newContentType.Fields)
            {
                listContentType.FieldLinks.Delete(field.Id);
                listContentType.Update();
                listContentType.FieldLinks.Add(new SPFieldLink(field));
                listContentType.Update();
            }
            
            //remove old fields
            foreach (SPField field in listContentType.Fields)
            {
                if (!newContentType.Fields.Contains(field.Id))
                {
                    listContentType.FieldLinks.Delete(field.Id);
                    listContentType.Update();
                    if (removeOldField) spList.Fields[field.Id].Delete();
                }
            }
            spList.Update();
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
                    fieldSchema.Element(@"Field").Add(new XAttribute(key, value));
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
            spList.Update();
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

        private static object AddContentTypeToList(SPList spList)
        {
            if (string.IsNullOrEmpty(ContentTypeId))
                return new Exception(string.Format("Content type id for list {0} not set", ListDisplayName));

            SPContentType ct = spList.ParentWeb.AvailableContentTypes[new SPContentTypeId(ContentTypeId)];
            if (ct == null)
                return new Exception(string.Format("Content type with id: {0} not exist", ContentTypeId));

            spList.ContentTypesEnabled = true;
            if (!spList.IsContentTypeAllowed(ct))
                return new Exception(string.Format("Content type with id: {0} not allow for list '{1}'", ContentTypeId, ListDisplayName));
            if (spList.ContentTypes[ct.Name] != null) 
                return null;
            
            spList.ContentTypes.Add(ct);
            try //try delete default CT
            {
                SPContentTypeId listItemContentTypeId =
                    spList.ContentTypes[spList.ParentWeb.ContentTypes[SPBuiltInContentTypeId.Item].Name].Id;
                spList.ContentTypes.Delete(listItemContentTypeId);
            }
            catch{ }
            spList.Update();

            SPView view = spList.DefaultView;
            foreach (SPField field in ct.Fields)
            {
                view.ViewFields.Add(field);
            }
            view.Update();
            return null;
        }

        private static string ContentTypeId
        {
            get
            {
                ContentTypeAttribute ct =
                    (ContentTypeAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(ContentTypeAttribute));
                if (ct != null)
                    return ct.ContentTypeId;
                return string.Empty;
            }
        }

    #endregion
    }

    
}
