using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SPExtention
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="W">Wrapper Type (a class that inherits this class)</typeparam>
    /// <typeparam name="L">List Props Type</typeparam>
    /// <typeparam name="I">ListItem Type</typeparam>
    public class SPListWrapper<W,L,I>
        where W: SPListWrapper<W,L,I>, new()
        where L: ListProps, new() 
        where I: BaseListItem, new()
    {
        private SPWeb web;

        private PropertyInfo[] allFields;
        private PropertyInfo[] ownFields;

        public SPList SPList { get; private set; }
        public string Title => Props?.DisplayName;
        public string[] Columns { 
            get
            {
                return allFields.Select(prop => prop.Name).ToArray();
            }
        }

        public L Props { get; private set; }

        protected SPListWrapper()
        {
            allFields = Utils.GetPropertyFields<I>();
            ownFields = Utils.GetPropertyFields<I>(true);
            Props = new L();
        }

        /// <summary>
        /// Create list in specific web
        /// </summary>
        public static W Create(SPWeb web)
        {
            var list = new W();
            list.SPList = SPListHelper<L,I>.Create(web);
            list.web = web;
            return list;
        }

        /// <summary>
        /// Create list if not exist or update if exist
        /// </summary>
        public static W CreateOrUpdate(SPWeb web)
        {
            var list = GetExisting(web);

            if (list == null)
            {
                list = Create(web);
            } 
            else
            {
                SPListHelper<L, I>.UpdateListInfo(web);
                SPListHelper<L, I>.UpdateFields(web, true);
            }

            return list;
        }

        /// <summary>
        /// Get list from specific web
        /// </summary>
        /// <param name="web"></param>
        /// <returns></returns>
        public static W GetExisting(SPWeb web)
        {
            var spList = SPListHelper<L,I>.GetSPListByInternalOrDisplayName(web);

            if (spList != null)
            {
                var list = new W();
                list.web = web;
                list.SPList = spList;
                return list;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Delete list
        /// </summary>
        public void Delete()
        {
            SPListHelper<L, I>.Delete(web);
        }

        /// <summary>
        /// Update fields in list from class-wrapper
        /// </summary>
        /// <param name="removeOldFields">true if delete fields that not specified in class wrapper</param>
        /// <returns></returns>
        public void UpdateFields(bool removeOldFields = false)
        {
            SPListHelper<L, I>.UpdateFields(web, removeOldFields);
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
            SPListHelper<L, I>.SaveAsTemplate(web, templateName, description, saveData);
        }

        public IEnumerable<I> GetItems()
        {
            return SPListHelper<L, I>.GetAllListItems(web);
        }

        public void AddItem(I item)
        {
            var spListItem = SPList.Items.Add();

            foreach (PropertyInfo prop in Utils.GetPropertyFields<I>())
            {
                var value = prop.GetValue(item);
                if (value != null)
                {
                    spListItem[prop.Name] = value;
                }
            }

            spListItem.Update();
        }

        public void DeleteItem(I item)
        {
            var spListItem = SPList.Items.GetItemById(int.Parse(item.ID));
            spListItem.Delete();
        }

        /// <summary>
        /// Delete data
        /// </summary>
        public void Clear()
        {

        }

    }
}
