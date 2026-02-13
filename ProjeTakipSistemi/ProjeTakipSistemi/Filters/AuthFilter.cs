using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjeTakipSistemi.Filters
{
    public class AuthFilter : FilterAttribute, IAuthorizationFilter
    {
        protected int yetkiTur;
        public AuthFilter(int yetkiTur) 
        { 
             this.yetkiTur = yetkiTur;       
        }
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            //int yetkiTurId = Convert.ToInt32()
        }
    }
}