﻿using Kooboo.Api;
using Kooboo.Data.Interface;
using Kooboo.Sites.Extensions;
using Kooboo.Sites.Models;
using System;
using System.Collections.Generic;

namespace Kooboo.Web.Api.Implementation
{
    public class CoreSettingApi : IApi
    {
        public string ModelName => "CoreSetting";

        public bool RequireSite => true;

        public bool RequireUser => true;

        public Dictionary<string, string> Get(ApiCall call, string name)
        {
            var sitedb = call.Context.WebSite.SiteDb();

          
            var item = sitedb.CoreSetting.Get(name); 
            if (item !=null)
            {
                return item.Values; 
            }
            else
            {
                var type = Sites.Service.CoreSettingService.GetSettingType(name);

                if (type != null)
                {
                    Dictionary<string, string> result = new Dictionary<string, string>(); 
                    var allfieldsl = Lib.Reflection.TypeHelper.GetPublicPropertyOrFields(type);
                    foreach (var field in allfieldsl)
                    {
                        result[field.Name] = ""; 
                    }

                    return result; 
                }
            }
            return null; 
        }

        public List<CoreSettingViewModel> List(ApiCall call)
        {  
            var sitedb = call.WebSite.SiteDb();

            List<CoreSettingViewModel> result = new List<CoreSettingViewModel>(); 
             
            foreach (var item in Kooboo.Sites.Service.CoreSettingService.types)
            {
                var value = sitedb.CoreSetting.Get(item.Key); 
               
                if (value == null)
                {
                    var instance = Activator.CreateInstance(item.Value) as ISiteSetting; 
                    if (instance !=null)
                    {
                        result.Add(new CoreSettingViewModel() { Name = instance.Name }); 
                    } 
                }
                else
                {
                    var json = Lib.Helper.JsonHelper.Serialize(value.Values); 

                    result.Add(new CoreSettingViewModel() { Name =value.Name, Value = json, lastModify = value.LastModified });  
                }

            }

            return result; 
        }

        public void Update(string name, Dictionary<string, string> model, ApiCall call)
        {
            var Core = new CoreSetting();
            Core.Name = name;
            Core.Values = model;
            call.WebSite.SiteDb().CoreSetting.AddOrUpdate(Core); 

        }
    }

    public class CoreSettingViewModel
    {
       
        public string Name { get; set; }

        public string Value { get; set; }

        public DateTime lastModify { get; set; }
    }
}
