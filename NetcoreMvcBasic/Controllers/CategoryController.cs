using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetcoreMvcBasic.Business.Entities;
using NetcoreMvcBasic.Business.Repositories;
using NetcoreMvcBasic.Business.Repositories.Session;
using Smooth.IoC.UnitOfWork.Interfaces;

namespace NetcoreMvcBasic.Controllers
{
    public class CategoryController : BaseController
    {
        private readonly ICategoryRepository _categoryRepository;
        
        public CategoryController(IDbFactory dbFactory, CategoryRepository categoryrepository) : base(dbFactory)
        {
            _categoryRepository = categoryrepository;
        }

        public ActionResult Index()
        {
            return View();
        }

    }
}
