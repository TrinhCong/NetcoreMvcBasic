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

        [HttpGet]
        public ActionResult GetTable(string searchString, int? page)
        {
            using (var session = _dbFactory.Create<IAppSession>())
            {
                var pageSize = 5;
                ViewData["CurrentFilter"] = searchString;
                int total = 0;
                var categories = _categoryRepository.Filter(searchString, page, pageSize, out total);
                int pageCount = (int)Math.Ceiling((double)total / pageSize);
                ViewBag.TotalPage = pageCount;
                ViewBag.CurrentPage = (page ?? 1);
                return View("_ResultTable", categories);
            }
                
        }

        [HttpPost]
        public ActionResult Create( [FromBody]Category category)
        {
            using (var session = _dbFactory.Create<IAppSession>())
            {
                using (var uow = session.UnitOfWork())
                {
                    try
                    {
                        if (_categoryRepository.isExist(category))
                        {
                            return Json(new { success = false });
                        }
                        _categoryRepository.SaveOrUpdate(category, uow);
                        return Json(new { success = true });
                    }
                    catch (Exception)
                    {
                        return Json(new { success = false, message = "Vui lòng kiểm tra lại !" });
                    }
                }
            }
        }

        [HttpGet]
        public ActionResult Update(int id)
        {
            using (var session = _dbFactory.Create<IAppSession>())
            {
                var category = _categoryRepository.GetKey(id, session);
                return Json(category);
            }
        }
        [HttpPost]
        public ActionResult Update([FromBody] Category cate)
        {
            using (var session = _dbFactory.Create<IAppSession>())
            {
                using (var uow = session.UnitOfWork())
                {
                    try
                    {
                        if (_categoryRepository.isExist(cate))
                        {
                            return Json(new { success = false });
                        }
                        _categoryRepository.SaveOrUpdate(cate, uow);
                        return Json(new { success = true });
                    }
                    catch (Exception)
                    {
                        return Json(new { success = false, message = "Vui lòng kiểm tra lại !" });
                    }
                }
            }
        }
        [HttpGet]
        public ActionResult Detail(int id)
        {
            using (var session = _dbFactory.Create<IAppSession>())
            {
                var detailCategory = _categoryRepository.GetKey(id, session);
                return Json(detailCategory);
            }
        }

        public ActionResult Delete(int id)
        {
            using (var session = _dbFactory.Create<IAppSession>())
            {
                try
                {
                    if (id > 0)
                    {
                        _categoryRepository.DeleteKey(id, session);
                        return Json(new { success = true, message = "Xóa thành công !" });
                    }
                }
                catch (Exception)
                {
                    return Json(new { success = false, message = "Xóa thất bại !" });
                }
                return RedirectToAction();
            }
        }
    }
}
