using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.SharePoint;

namespace SPExtention
{
    public abstract class SPListExtention<T> where T : SPListExtention<T>
    {
        public readonly SPWeb Web;
        private const string FieldXmlFormat = "<Field DisplayName='{0}' StaticName='{1}' Name='{1}' ID='{{{2}}}' Type='{3}' {4}>{5}</Field>";

        #region Constructor
        /// <summary>
        /// This constuctor sets string value to property = field internal name 
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
        /// <param name="web"></param>
        protected SPListExtention(SPWeb web)
            : this()
        {
            if (web == null)
                throw new Exception("SPListExtention: SPWeb is null");
            Web = web;
        }
        #endregion

        #region Public instance

        /// <summary>
        /// Create list in specific web
        /// </summary>
        /// <returns></returns>
        public SPList Create()
        {
            return Create(Web);
        }

        /// <summary>
        /// Create list from specific web
        /// </summary>
        /// <returns></returns>
        public void Delete()
        {
            Delete(Web);
        }

        /// <summary>
        /// Get SPList object
        /// </summary>
        /// <returns></returns>
        public SPList GetSPListByInternalOrDisplayName()
        {
            return GetSPListByInternalOrDisplayName(Web);
        }

        /// <summary>
        /// Update fields in list from class-wrapper
        /// </summary>
        /// <param name="removeOldFields">true if delete fields that not specified in class wrapper</param>
        /// <returns></returns>
        public void UpdateFields(bool removeOldFields = false)
        {
            UpdateFields(Web, removeOldFields);
        }

        /// <summary>
        /// Save exists list as list template
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="description"></param>
        /// <param name="saveData"></param>
        /// <returns></returns>
        public void SaveAsTemplate(string templateName, string description = "", bool saveData = false)
        {
            SaveAsTemplate(Web, templateName, description, saveData);
        }

        /// <summary>
        /// Create list if not exist or update if exist
        /// </summary>
        public SPList CreateOrUpdate()
        {
            var l = GetSPListByInternalName(Web);
            if (l == null)
                return Create(Web);
            UpdateListInfo(Web);
            UpdateFields(Web, true);
            return l;
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

        public static string ContentTypeId
        {
            get
            {
                ContentTypeIdAttribute ct =
                    (ContentTypeIdAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(ContentTypeIdAttribute));
                if (ct != null)
                    return ct.ContentTypeId;
                return string.Empty;
            }
        }

        public static string ContentTypeName
        {
            get
            {
                ContentTypeNameAttribute ct =
                    (ContentTypeNameAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(ContentTypeNameAttribute));
                if (ct != null)
                    return ct.ContentTypeName;
                return string.Empty;
            }
        }

        public static bool IsHiddenList
        {
            get
            {
                HiddenListAttribute hl =
                    (HiddenListAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(HiddenListAttribute));
                if (hl != null)
                    return true;
                return false;
            }
        }

        public static SPList Create(SPWeb spWeb)
        {
            if (spWeb == null)
                throw new Exception("Create: SPWeb is null");

            if (string.IsNullOrEmpty(ListDisplayName) || string.IsNullOrEmpty(ListInternalName))
                throw new Exception("Create: Display name or internal name not defined");

            var addLResult = AddListToWeb(spWeb);
            if (addLResult is Exception)
                throw addLResult as Exception;

            return addLResult as SPList;
        }

        public static void CreateOrUpdate(SPWeb spWeb, bool removeOldFields = false, bool removeTitle = true)
        {
            if (GetSPListByInternalOrDisplayName(spWeb) == null)
            {
                var addLResult = AddListToWeb(spWeb, removeTitle);
                if (addLResult is Exception)
                    throw addLResult as Exception;
            }
            else
            {
                UpdateListInfo(spWeb);
                UpdateFields(spWeb, removeOldFields);
            }                
        }

        public static void Delete(SPWeb spWeb)
        {
            if (spWeb == null)
                throw new Exception("Delete: SPWeb is null");

            var spList = spWeb.Lists.TryGetList(ListDisplayName);
            if (spList != null)
                spWeb.Lists.Delete(spList.ID);
        }

        public static void UpdateListInfo(SPWeb spWeb)
        {
            var spList = GetSPListByInternalName(spWeb);
            if (spList == null)
                throw new Exception("UpdateListInfo: list not found");

            spList.Title = ListDisplayName;
            spList.Hidden = IsHiddenList;
            spList.Description = ListDescription;
            spList.Update();
        }

        public static void UpdateFields(SPWeb spWeb, bool removeOldFields = false)
        {
            if (spWeb == null)
                throw new Exception("UpdateFields: SPWeb is null");

            if (string.IsNullOrEmpty(ListDisplayName) || string.IsNullOrEmpty(ListInternalName))
                throw new Exception("UpdateFields: Display name or internal name not defined");

            var existList = GetSPListByInternalOrDisplayName(spWeb);
            if (existList == null)
                throw new Exception(string.Format("UpdateFields: List with name {0} NOT exist", ListDisplayName + " || " + ListInternalName));

            UpdateListFields(existList, removeOldFields);
        }

        public static SPList GetSPListByInternalName(SPWeb spWeb)
        {
            if (spWeb == null)
                throw new Exception("GetSPListByInternalName: SPWeb is null");

            if (string.IsNullOrEmpty(ListInternalName))
                return null;
            SPList spList = (from SPList l in spWeb.Lists
                             where l.RootFolder.Name.Equals(ListInternalName, StringComparison.InvariantCulture)
                             select l).FirstOrDefault();
            return spList;
        }

        public static SPList GetSPListByDisplayName(SPWeb spWeb)
        {
            if (spWeb == null)
                throw new Exception("GetSPListByDisplayName: SPWeb is null");

            if (string.IsNullOrEmpty(ListDisplayName))
                return null;
            var spList = spWeb.Lists.TryGetList(ListDisplayName);
            return spList;
        }

        public static SPList GetSPListByInternalOrDisplayName(SPWeb spWeb)
        {
            if (spWeb == null)
                throw new Exception("GetSPListByInternalOrDisplayName: SPWeb is null");

            return GetSPListByInternalName(spWeb) ?? GetSPListByDisplayName(spWeb);
        }

        public static void SaveAsTemplate(SPWeb spWeb, string templateName, string description = "",
            bool saveData = false)
        {
            if (spWeb == null)
                throw new Exception("SaveAsTemplate: SPWeb is null");

            SPList spList = GetSPListByInternalOrDisplayName(spWeb);
            if (spList == null)
                throw new Exception(string.Format("SaveAsTemplate: Can't create template. List instance {0} {1} not found", ListInternalName, ListDisplayName));

            string fileName = templateName.Replace(" ", "") + ".stp";

            SPList gallery = spWeb.Lists["List Template Gallery"];
            foreach (SPListItem template in gallery.GetItems(new SPQuery()))
            {
                if (template.Title.Equals(templateName))
                {
                    template.Delete();
                    gallery.Update();
                    break;
                }
            }
            spList.SaveAsTemplate(fileName, templateName, description, saveData);
            spList.Update();
        }

        public static List<SPField> GetCustomFields(SPWeb spWeb)
        {
            return GetCustomFieldList(GetSPListByInternalOrDisplayName(spWeb));
        }

        public static SPContentType CreateContentType(SPWeb spWeb, bool deleteTitle = true, string groupName = "Custom Content Types")
        {
            SPContentType ct = null;
            bool ctIdExist = !string.IsNullOrEmpty(ContentTypeId);

            if (ctIdExist)
                ct = spWeb.AvailableContentTypes[new SPContentTypeId(ContentTypeId)] ?? spWeb.AvailableContentTypes[ContentTypeName];

            if (ct != null)
                throw new Exception(string.Format("Content type with ID or name exist: {0}", string.IsNullOrEmpty(ContentTypeId) ? ContentTypeName : ContentTypeId));

            //TODO:check below string for using SPBuiltInContentTypeId.Item
            ct = CreateSiteContentType(spWeb, ContentTypeName, !string.IsNullOrEmpty(ContentTypeId) ? new SPContentTypeId(ContentTypeId) : SPBuiltInContentTypeId.Item, groupName);

            var siteColumns = CreateSiteColumns(spWeb);
            AddFieldsToContentType(spWeb, siteColumns, ct, false);

            if (deleteTitle)
                RemoveTitle(spWeb, ct.Id);

            return ct;
        }

        public static SPContentType UpdateContentType(SPWeb spWeb, bool updateChild = true)
        {
            SPContentType ct = null;
            bool ctIdExist = !string.IsNullOrEmpty(ContentTypeId);
            if (ctIdExist)
                ct = spWeb.AvailableContentTypes[new SPContentTypeId(ContentTypeId)] ?? spWeb.AvailableContentTypes[ContentTypeName];

            var siteColumns = CreateSiteColumns(spWeb);

            AddFieldsToContentType(spWeb, siteColumns, ct, updateChild);
            RemoveNotExistFieldsFromContentType(spWeb, siteColumns, ct, updateChild);
            return ct;
        }

        public static void CreateOrUpdateContentType(SPWeb spWeb, bool updateChild = true, bool deleteTitle = true)
        {
            SPContentType ct = null;
            bool ctIdExist = !string.IsNullOrEmpty(ContentTypeId);
            if (ctIdExist)
                ct = spWeb.AvailableContentTypes[new SPContentTypeId(ContentTypeId)] ?? spWeb.AvailableContentTypes[ContentTypeName];
            if (ct != null)
            {
                UpdateContentType(spWeb, updateChild);
            }
            else
            {
                CreateContentType(spWeb, deleteTitle);
            }
        }

        #endregion

        #region Private

        private static PropertyInfo[] PropertyFields
        {
            get
            {
                return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.GetProperty);
            }
        }

        private static List<SPField> CreateSiteColumns(SPWeb spWeb)
        {
            List<SPField> listFields = new List<SPField>();
            foreach (PropertyInfo prop in PropertyFields)
            {
                BaseFieldInfo bfi = new BaseFieldInfo(prop);
                var siteColumn = CreateSiteColumn(spWeb, bfi);
                listFields.Add(siteColumn);
            }
            return listFields;
        }

        private static SPContentType CreateSiteContentType(SPWeb spWeb, string contentTypeName, SPContentTypeId contentTypeId, string groupName)
        {
            try
            {
                if (spWeb.AvailableContentTypes[contentTypeName] == null)
                {
                    //SPContentType itemCType = spWeb.AvailableContentTypes[contentTypeId];
                    //SPContentType contentType = new SPContentType(itemCType, spWeb.ContentTypes, contentTypeName)
                    SPContentType contentType = new SPContentType(contentTypeId, spWeb.ContentTypes, contentTypeName)
                    {
                        Group = groupName
                    };
                    spWeb.ContentTypes.Add(contentType);
                    contentType.Update();
                    return contentType;
                }
                return spWeb.ContentTypes[contentTypeName];
            }
            catch (Exception ex)
            {
                throw new Exception("CreateSiteContentType: " + ex.Message);
            }
        }

        private static void AddFieldToContentType(SPWeb spWeb, SPContentTypeId contentTypeId, SPField field, bool updateChild)
        {
            try
            {
                SPContentType contentType = spWeb.ContentTypes[contentTypeId];
                if (contentType == null) return;
                if (contentType.FieldLinks.Cast<SPFieldLink>().Any(x => x.Name == field.InternalName)) return;
                field = spWeb.Fields.GetFieldByInternalName(field.InternalName);
                SPFieldLink fieldLink = new SPFieldLink(field);
                fieldLink.Hidden = false;
                fieldLink.Required = field.Required;
                contentType.FieldLinks.Add(fieldLink);
                contentType.Update(updateChild);
            }
            catch (Exception ex)
            {
                throw new Exception("AddFieldToContentType: " + ex.Message);
            }
        }

        private static void AddFieldsToContentType(SPWeb spWeb, List<SPField> fields, SPContentType contentType, bool updateChild)
        {
            foreach (SPField field in fields)
                AddFieldToContentType(spWeb, contentType.Id, field, updateChild);
        }

        private static void RemoveNotExistFieldsFromContentType(SPWeb spWeb, List<SPField> fields, SPContentType contentType, bool updateChild)
        {
            SPContentType ct = spWeb.ContentTypes[contentType.Id];
            foreach (SPField field in ct.Fields)
            {
                var n = field.InternalName;
                if (fields.Exists(x => x.InternalName == field.InternalName)) continue;
                ct.FieldLinks.Delete(field.InternalName);
            }
            ct.Update(updateChild);
        }

        private static object AddListToWeb(SPWeb spWeb, bool removeTitle = true)
        {
            if (GetSPListByInternalName(spWeb) != null)
                return new Exception(string.Format("AddListToWeb: List with internal name {0} exist", ListInternalName));

            if (GetSPListByDisplayName(spWeb) != null)
                return new Exception(string.Format("AddListToWeb: List with name {0} exist", ListDisplayName));
            try
            {
                Guid listGuid = spWeb.Lists.Add(ListInternalName, ListDescription, SPListTemplateType.GenericList);
                SPList list = spWeb.Lists[listGuid];
                list.Title = ListDisplayName;
                list.Hidden = IsHiddenList;
                list.Update();

                if (!string.IsNullOrEmpty(ContentTypeId))
                {
                    AddContentTypeToList(list);
                }
                else
                {
                    AddFieldsToList(list);
                }

                if (removeTitle)
                {
                    SPField titleField = list.Fields[SPBuiltInFieldId.Title];
                    titleField.Hidden = true;
                    titleField.Required = false;
                    titleField.Update(true);
                }
                return GetSPListByInternalName(spWeb);
            }
            catch (Exception ex)
            {
                throw new Exception("AddListToWeb: " + ex.Message);
            }
        }

        private static void AddFieldsToList(SPList spList)
        {
            try
            {
                foreach (PropertyInfo prop in PropertyFields)
                {
                    BaseFieldInfo bfi = new BaseFieldInfo(prop);
                    if (string.IsNullOrEmpty(bfi.FieldType)) continue;
                    AddFieldToList(spList, bfi);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("AddFieldsToList: " + ex.Message);
            }
        }


        private static SPField AddFieldToList(SPList spList, BaseFieldInfo baseInfo)
        {
            var fieldXml = GetFieldXml(baseInfo);

            if (baseInfo.FieldType == SPFieldType.Lookup.ToString("G"))
                fieldXml = ParseLookupXml(fieldXml, spList.ParentWeb);

            var strInternalName = spList.Fields.AddFieldAsXml(fieldXml, baseInfo.DefaultView, SPAddFieldOptions.AddFieldInternalNameHint);
            var field = spList.Fields.GetFieldByInternalName(strInternalName);
            field.Update();
            return field;
        }

        private static SPField CreateSiteColumn(SPWeb spWeb, BaseFieldInfo baseInfo)
        {
            if (spWeb.Fields.ContainsField(baseInfo.InternalName))
                return spWeb.Fields.GetFieldByInternalName(baseInfo.InternalName);

            var fieldXml = GetFieldXml(baseInfo);
            var strInternalName = spWeb.Fields.AddFieldAsXml(fieldXml, baseInfo.DefaultView,
                SPAddFieldOptions.AddFieldInternalNameHint);
            var field = spWeb.Fields.GetFieldByInternalName(strInternalName);

            if (baseInfo.FieldType == SPFieldType.Lookup.ToString("G"))
                UpdateLookupField(spWeb, field);

            field.Update();
            return field;
        }

        private static void UpdateLookupField(SPWeb spWeb, SPField lookupField)
        {
            //SPField lookupField = spWeb.Fields.TryGetFieldByStaticName(baseInfo.InternalName);

            if (lookupField != null)
            {
                var parsedXml = ParseLookupXml(lookupField.SchemaXml, spWeb);
                lookupField.SchemaXml = parsedXml;
            }
        }

        private static string ParseLookupXml(string fieldSchemaString, SPWeb spWeb)
        {
            // Getting Schema of field
            XDocument fieldSchema = XDocument.Parse(fieldSchemaString);
            // Get the root element of the field definition
            XElement root = fieldSchema.Root;
            // Check if list definition exits exists
            if (root.Attribute("List") != null)
            {
                // Getting value of list url
                string listurl = root.Attribute("List").Value;

                // Get the correct folder for the list
                SPFolder listFolder = spWeb.GetFolder(listurl);
                if (listFolder != null && listFolder.Exists == true)
                {
                    // Setting the list id of the schema
                    XAttribute attrList = root.Attribute("List");
                    if (attrList != null)
                    {
                        // Replace the url wit the id
                        attrList.Value = listFolder.ParentListId.ToString();
                    }

                    // Setting the souce id of the schema
                    XAttribute attrWeb = root.Attribute("SourceID");
                    if (attrWeb != null)
                    {
                        // Replace the sourceid with the correct webid
                        attrWeb.Value = spWeb.ID.ToString();
                    }
                }
            }
            return fieldSchema.ToString();
        }

        private static string GetFieldXml(BaseFieldInfo baseInfo)
        {
            return string.Format(FieldXmlFormat,
                !string.IsNullOrEmpty(baseInfo.DisplayName) ? baseInfo.DisplayName : baseInfo.InternalName,
                baseInfo.InternalName,
                Guid.NewGuid(),
                baseInfo.FieldType,
                baseInfo.Requred ? baseInfo.AdditionalAttributeString + " Required = 'TRUE' " : baseInfo.AdditionalAttributeString,
                baseInfo.XmlAttribute);
        }

        private static string EscapeXMLValue(string xmlString)
        {
            if (string.IsNullOrWhiteSpace(xmlString)) return xmlString;
            return xmlString.Replace("'", "&apos;").Replace("\"", "&quot;").Replace(">", "&gt;").Replace("<", "&lt;").Replace("&", "&amp;");
        }

        private static string GetAdditionalAttributesString(IEnumerable<AdditionalFieldAttrAttribute> additionalFieldAttr)
        {
            if (additionalFieldAttr == null) return string.Empty;
            StringBuilder sb = new StringBuilder();
            foreach (AdditionalFieldAttrAttribute attr in additionalFieldAttr)
            {
                sb.AppendFormat("{0} ='{1}' ", attr.AttributeName, attr.Value);
            }
            return sb.ToString();
        }

        private static void UpdateListFields(SPList spList, bool removeOldField)
        {
            if (string.IsNullOrEmpty(ContentTypeId))
            {
                UpdateListInstanceFields(spList, removeOldField);
            }
            else
            {
                //todo:this dont requred when CT updated with updateChild
                //UpdateContentTypeListFields(spList, removeOldField);
            }
        }

        private static void UpdateListInstanceFields(SPList spList, bool removeOldFields)
        {
            try
            {
                List<string> existFields = GetCustomFieldInternalNameList(spList);
                List<string> propNames = new List<string>();
                foreach (PropertyInfo prop in PropertyFields)
                {
                    BaseFieldInfo bfi = new BaseFieldInfo(prop);
                    if (string.IsNullOrEmpty(bfi.FieldType)) continue;

                    if (existFields.Contains(prop.Name))
                    {
                        AddFieldAttribute(spList, prop.Name, "DisplayName", bfi.DisplayName);
                        AddFieldAttribute(spList, prop.Name, "Type", bfi.FieldType);
                        if (bfi.AdditionalAttributes != null && bfi.AdditionalAttributes.Any())
                            AddFieldAttributes(spList, prop.Name, bfi.AdditionalAttributes);
                    }
                    else
                    {
                        AddFieldToList(spList, bfi);
                    }
                    propNames.Add(prop.Name);
                }

                if (!removeOldFields) return;

                foreach (string ef in existFields.Where(ef => !propNames.Contains(ef)))
                    DeleteFields(spList, ef);
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateListInstanceFields: " + ex.Message);
            }
        }

        private static void UpdateContentTypeListFields(SPList spList, bool removeOldField)
        {
            try
            {
                SPContentType newContentType =
                    spList.ParentWeb.AvailableContentTypes[new SPContentTypeId(ContentTypeId)];
                SPContentType listContentType =
                    spList.ContentTypes.Cast<SPContentType>()
                        .FirstOrDefault(c => c.Id.ToString().StartsWith(ContentTypeId));
                if (newContentType == null || listContentType == null) return;

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
                        //if (removeOldField) spList.Fields[field.Id].Delete();
                    }
                }
                spList.Update();
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateContentTypeListFields: " + ex.Message);
            }
        }

        private static void AddFieldAttributes(SPList spList, string fieldInternalName, IList<AdditionalFieldAttrAttribute> additionalFieldAttr)
        {
            SPField spField = spList.Fields.TryGetFieldByStaticName(fieldInternalName);
            if (spField == null)
                throw new Exception(string.Format("AddFieldAttributes: Field with name {0} NOT exist", fieldInternalName));
            foreach (var attr in additionalFieldAttr)
            {
                AddFieldAttribute(spList, fieldInternalName, attr.AttributeName, attr.Value);
            }
        }

        private static void AddFieldAttribute(SPList spList, string fieldInternalName, string key, string value)
        {
            SPField spField = spList.Fields.TryGetFieldByStaticName(fieldInternalName);
            if (spField == null)
                throw new Exception(string.Format("AddFieldAttribute: Field with name {0} NOT exist", fieldInternalName));
            AddFieldAttribute(spField, key, value);
        }

        private static void AddFieldAttribute(SPField spField, string key, string value)
        {
            var fieldSchema = XDocument.Parse(spField.SchemaXml);
            var xElement = fieldSchema.Element(@"Field");
            if (xElement != null)
            {
                var tabAttribute = xElement.Attribute(key);
                if (tabAttribute == null)
                    xElement.Add(new XAttribute(key, value));
                else
                    tabAttribute.Value = value;
            }
            if (spField.Type == SPFieldType.Lookup)
                spField.SchemaXml = ParseLookupXml(fieldSchema.ToString(), spField.ParentList.ParentWeb);
            else
                spField.SchemaXml = fieldSchema.ToString();
            spField.PushChangesToLists = true;
            spField.Update();
        }

        private static void DeleteFields(SPList spList, string internalFieldName)
        {
            SPField f = spList.Fields.TryGetFieldByStaticName(internalFieldName);
            if (f.Hidden && f.ReadOnlyField && !string.IsNullOrEmpty(f.GetProperty("BdcField")))
                return;
            f.Sealed = false;
            f.Hidden = false;
            f.ReadOnlyField = false;
            f.Update();
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
            if (spList == null) return null;
            return (spList.Fields.Cast<SPField>()
                .Where(field => !SPBuiltInFieldId.Contains(field.Id) && !field.SourceId.StartsWith("http://"))).ToList();
        }

        private static void AddContentTypeToList(SPList spList)
        {
            if (string.IsNullOrEmpty(ContentTypeId))
                throw new Exception(string.Format("AddContentTypeToList: Content type id for list {0} not set", ListDisplayName));

            SPContentType ct = spList.ParentWeb.AvailableContentTypes[new SPContentTypeId(ContentTypeId)];
            if (ct == null)
                throw new Exception(string.Format("AddContentTypeToList: Content type with id: {0} not exist", ContentTypeId));

            spList.ContentTypesEnabled = true;
            if (!spList.IsContentTypeAllowed(ct))
                throw new Exception(string.Format("AddContentTypeToList: Content type with id: {0} not allow for list '{1}'", ContentTypeId, ListDisplayName));
            if (spList.ContentTypes[ct.Name] != null)
                return;

            spList.ContentTypes.Add(ct);
            try
            {
                //var itemCTName = spList.ParentWeb.Site.RootWeb.ContentTypes[SPBuiltInContentTypeId.Item].Name;
                SPContentTypeId listItemContentTypeId = spList.ContentTypes["Item"].Id;
                spList.ContentTypes.Delete(listItemContentTypeId);
            }
            catch (Exception ex) { }

            spList.Update();

            SPView view = spList.DefaultView;
            foreach (SPField field in ct.Fields)
            {
                view.ViewFields.Add(field);
            }
            view.ViewFields.Delete("LinkTitle");//delete Title from view
            view.Update();
        }

        private static void RemoveTitle(SPWeb spWeb, SPContentTypeId contentTypeId)
        {
            SPContentType contentType = spWeb.ContentTypes[contentTypeId];
            if (contentType.FieldLinks.Cast<SPFieldLink>().Any(x => x.Id == SPBuiltInFieldId.Title))
            {
                contentType.FieldLinks.Delete(SPBuiltInFieldId.Title);
                contentType.Update();
            }
        }

        private static List<BaseFieldInfo> FieldsInfo
        {
            get
            {
                List<BaseFieldInfo> fi = new List<BaseFieldInfo>();
                foreach (PropertyInfo prop in PropertyFields)
                    fi.Add(new BaseFieldInfo(prop));
                return fi;
            }
        }
        #endregion

        class BaseFieldInfo
        {
            public string InternalName { get; private set; }
            public string DisplayName { get; private set; }
            public string FieldType { get; private set; }
            public string XmlAttribute { get; private set; }
            public string AdditionalAttributeString { get; private set; }
            public IList<AdditionalFieldAttrAttribute> AdditionalAttributes { get; private set; }
            public bool DefaultView { get; private set; }
            public bool Requred { get; private set; }


            public BaseFieldInfo(PropertyInfo fieldsProperty)
            {
                InternalName = fieldsProperty.Name;
                var displayName = fieldsProperty.GetCustomAttribute<DisplayNameAttribute>();
                DisplayName = displayName != null ? displayName.Name : string.Empty;

                var fieldType = fieldsProperty.GetCustomAttribute<FieldTypeAttribute>();
                FieldType = fieldType != null ? fieldType.Type : string.Empty;

                var xmlAttr = fieldsProperty.GetCustomAttribute<InternalFieldXmlAttribute>();
                XmlAttribute = xmlAttr != null ? xmlAttr.InternalXml : string.Empty;

                DefaultView = fieldsProperty.GetCustomAttribute<DefaultViewAttribute>() != null;
                Requred = fieldsProperty.GetCustomAttribute<RequiredAttribute>() != null;

                var additionalAttr = fieldsProperty.GetCustomAttributes<AdditionalFieldAttrAttribute>();
                AdditionalAttributes = additionalAttr as IList<AdditionalFieldAttrAttribute> ?? additionalAttr.ToList();
                AdditionalAttributeString = GetAdditionalAttributesString(AdditionalAttributes);
            }
        }

    }

}
