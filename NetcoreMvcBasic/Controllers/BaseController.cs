using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetcoreMvcBasic.Models;
using Smooth.IoC.UnitOfWork.Interfaces;

namespace NetcoreMvcBasic.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IDbFactory _dbFactory;

        public BaseController(IDbFactory dbFactory)
        {
            this._dbFactory = dbFactory;
        }
    }
}
