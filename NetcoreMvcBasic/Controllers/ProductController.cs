using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper.FastCrud;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using NetcoreMvcBasic.Business.Models;
using NetcoreMvcBasic.Business.Repositories;
using NetcoreMvcBasic.Models;
using NetcoreMvcBasic.Business.Entities;
using NetcoreMvcBasic.Business.Repositories;
using NetcoreMvcBasic.Business.Repositories.Session;
using Smooth.IoC.UnitOfWork.Interfaces;

namespace NetcoreMvcBasic.Controllers
{
    public class ProductController : BaseController
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISupplierRepository _supplierRepository;

        public ProductController(
            IDbFactory dbFactory,
            ProductRepository productRepository,
            CategoryRepository categoryRepository,
            SupplierRepository supplierRepository
            ) : base(dbFactory)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _supplierRepository = supplierRepository;

        }

        public ActionResult Index(FilterParam param)
        {
            int total = 0;
            var pageSize = 10;

            using (var session = _dbFactory.Create<IAppSession>())
            {
                _productRepository.GetTotalRecord(out total);
                var lastpage = (int)Math.Ceiling((double)total / pageSize);
                ViewBag.LastPage = lastpage;

                ViewBag.GetCategory = _categoryRepository.GetAll(session);
                ViewBag.GetSupplier = _supplierRepository.GetAll(session);
                ViewBag.GetProducts = _productRepository.GetAll(session);
                return View();
            }
        }

        [HttpPost]
        public ActionResult GetTable(FilterParam param)
        {
            using (var session = _dbFactory.Create<IAppSession>())
            {
                ViewBag.GetCategory = _categoryRepository.GetAll(session);
                ViewBag.GetSupplier = _supplierRepository.GetAll(session);
                param.pageSize = 10;
                ViewData["CurrentFilter"] = param.searchString;
                ViewData["CategoryId"] = param.cateId;
                ViewData["SupplierId"] = param.supId;
                int total = 0;
                var products = _productRepository.Filter(param, out total);
                int pageCount = (int)Math.Ceiling((double)total / param.pageSize);
                ViewBag.TotalPage = pageCount;
                ViewBag.CurrentPage = (param.page ?? 1);
                return PartialView("_ResultTable", products);
            }
        }

        [HttpPost]
        public ActionResult Create([FromBody] Product product)
        {
            using (var session = _dbFactory.Create<IAppSession>())
            {
                using (var uow = session.UnitOfWork())
                {
                    ViewBag.GetCategory = _categoryRepository.GetAll(session);
                    ViewBag.GetSupplier = _supplierRepository.GetAll(session);
                    try
                    {
                        if (_productRepository.isExist(product))
                        {
                            return Json(new { success = false });
                        }
                        _productRepository.SaveOrUpdate(product, uow);
                        return Json(new { success = true });
                    }
                    catch (Exception)
                    {
                        return Json(new { success = false, message = "Cập nhật thất bại !" });
                    }
                }
            }
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            using (var session = _dbFactory.Create<IAppSession>())
            {
                var product = _productRepository.GetKey(id, session);
                return Json(product);
            }
        }

        [HttpPost]
        public ActionResult Edit([FromBody] Product product)
        {
            using (var session = _dbFactory.Create<IAppSession>())
            {
                using (var uow = session.UnitOfWork())
                {
                    try
                    {
                        if (_productRepository.isExist(product))
                        {
                            return Json(new { success = false });
                        }
                        _productRepository.SaveOrUpdate(product, uow);
                        return Json(new { success = true });
                    }
                    catch (Exception)
                    {
                        return Json(new { success = false, message = "Đã có lỗi trong quá trình sửa đổi, vui lòng kiểm tra lại !" });
                    }
                }
            }
        }

        [HttpGet]
        public ActionResult Detail(int id)
        {
            using (var session = _dbFactory.Create<IAppSession>())
            {
                var product = _productRepository.GetKey(id, session);
                return Json(product);
            }
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            using (var session = _dbFactory.Create<IAppSession>())
            {
                try
                {
                    if (id > 0)
                    {
                        _productRepository.DeleteKey(id, session);
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

        [HttpGet]
        public ActionResult loadSupplier(int cateId)
        {
            using (var session = _dbFactory.Create<IAppSession>())
            {
                var listSupplier = _supplierRepository.GetListSupplier(cateId);
                return Json(listSupplier);
            }
        }

        [HttpGet]
        public ActionResult Export(FilterParam param)
        {
            using (var session = _dbFactory.Create<IAppSession>())
            {
                var data = _productRepository.ExportData(param);

                var stream = new MemoryStream();
                using (var package = new ExcelPackage(stream))
                {
                    var sheet = package.Workbook.Worksheets.Add("Sản phẩm");
                    sheet.Cells[1, 1].Value = "ID";
                    sheet.Cells[1, 2].Value = "Tên hàng";
                    sheet.Cells[1, 3].Value = "Danh mục";
                    sheet.Cells[1, 4].Value = "Nhà cung cấp";
                    sheet.Cells[1, 5].Value = "Quy cách";
                    sheet.Cells[1, 6].Value = "Đơn giá";
                    sheet.Cells[1, 7].Value = "Số lượng tồn";
                    sheet.Cells[1, 8].Value = "Đ/v xuất bán";
                    sheet.Cells[1, 9].Value = "Mức độ đặt hàng lại";
                    sheet.Cells[1, 10].Value = "Giảm giá";

                    int rowIndex = 2;
                    foreach (var row in data)
                    {
                        sheet.Cells[rowIndex, 1].Value = row.product_id;
                        sheet.Cells[rowIndex, 2].Value = row.product_name;
                        sheet.Cells[rowIndex, 3].Value = row.Category.category_name;
                        sheet.Cells[rowIndex, 4].Value = row.Supplier.company_name;
                        sheet.Cells[rowIndex, 5].Value = row.quantity_per_unit;
                        sheet.Cells[rowIndex, 6].Value = row.unit_price;
                        sheet.Cells[rowIndex, 7].Value = row.units_in_stock;
                        sheet.Cells[rowIndex, 8].Value = row.units_on_order;
                        sheet.Cells[rowIndex, 9].Value = row.reorder_level;
                        if (row.discontinued > 0)
                        {
                            sheet.Cells[rowIndex, 10].Value = "Có";
                        }
                        else
                        {
                            sheet.Cells[rowIndex, 10].Value = "Không";
                        }

                        //Format Excel

                        using (var range = sheet.Cells["A1:J1"])
                        {
                            sheet.Row(1).Height = 35;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
                            range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        }

                        using (var range = sheet.Cells[rowIndex, 1, rowIndex, 10])
                        {
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        }
                        using (var range = sheet.Cells[rowIndex, 1])
                        {
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        }
                        using (var range = sheet.Cells[rowIndex, 2, rowIndex, 10])
                        {
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        }
                        rowIndex++;
                    }
                    sheet.Cells["A:AZ"].AutoFitColumns();

                    package.Save();
                }
                stream.Position = 0;
                var fileName = $"Product_{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        [HttpPost]
        public ActionResult Upload(IFormFile importFile)
        {
            using (var session = _dbFactory.Create<IAppSession>())
            {
                using (var uow = session.UnitOfWork())
                {
                    var listError = new List<string>();
                    try
                    {
                        if (importFile.FileName.EndsWith("xls") || importFile.FileName.EndsWith("xlsx"))
                        {
                            var rootFolder = @"D:\TestFile";
                            var fileName = importFile.FileName;
                            var filePath = Path.Combine(rootFolder, fileName);
                            FileInfo fileInfo = new FileInfo(filePath);
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                importFile.CopyToAsync(fileStream);
                            }

                            using (ExcelPackage package = new ExcelPackage(fileInfo))
                            {
                                ExcelWorksheet sheet = package.Workbook.Worksheets[0];
                                int totalRows = sheet.Dimension.End.Row;
                                int totalCols = sheet.Dimension.End.Column;

                                for (int i = 2; i <= totalRows; i++)
                                {
                                    string error = "";
                                    var product = new Product();
                                    var cate = new Category();

                                    var product_name = sheet.Cells[i, 1].Value.ToString();

                                    product.product_name = sheet.Cells[i, 1].Value.ToString();

                                    var cateName = sheet.Cells[i, 2].Value.ToString();

                                    var existCate = session.Find<Category>(p => p.Where($"LOWER ({nameof(Category.category_name)}) LIKE LOWER('%{cateName.Trim()}%')")).FirstOrDefault();

                                    if (existCate == null)
                                    {
                                        error += " cột [Danh mục] nhập không đúng, ";
                                    }

                                    else
                                    {
                                        var listSup = session.Find<Supplier>(s => s
                                                                       .Include<Product>(j => j.LeftOuterJoin())
                                                                       .Include<Category>(j => j.LeftOuterJoin())
                                                                       .Where($"{Sql.Table<Category>()}.{nameof(Category.category_id)} = {existCate.category_id}")
                                       ).Distinct();

                                        var sup_name = sheet.Cells[i, 3].Value.ToString();
                                        var existSup = session.Find<Supplier>(s => s.Where($"LOWER ({nameof(Supplier.company_name)}) LIKE LOWER('%{sup_name.Trim()}%')")).FirstOrDefault();
                                        string[] listSupName = listSup.Select(s => s.company_name).ToArray();
                                        if (existSup == null)
                                            error += " cột [Nhà cung cấp] nhập không đúng, ";
                                        else
                                        {
                                            if (listSupName.Contains(existSup.company_name))
                                            {
                                                product.category_id = existCate.category_id;
                                                product.supplier_id = existSup.supplier_id;
                                            }
                                            else
                                            {
                                                error += " cột [Nhà cung cấp] hoặc [Danh mục] không đúng";
                                            }
                                        }
                                    }
                                    var quantity_per_unit = sheet.Cells[i, 4].Value.ToString();
                                    if (int.TryParse(quantity_per_unit, out int q))
                                    {
                                        error += " cột [Quy cách] nhập sai kiểu dữ liệu, ";

                                    }

                                    else
                                        product.quantity_per_unit = sheet.Cells[i, 4].Value.ToString();

                                    float price;
                                    var unit_price = sheet.Cells[i, 5].Value.ToString();
                                    if (float.TryParse(unit_price, out price))
                                        product.unit_price = float.Parse(sheet.Cells[i, 5].Value.ToString());
                                    else
                                    {
                                        error += " cột [Đơn giá] nhập sai kiểu dữ liệu, ";
                                    }

                                    int stock;
                                    var unit_stock = sheet.Cells[i, 6].Value.ToString();
                                    if (Int32.TryParse(unit_stock, out stock))
                                        product.units_in_stock = Convert.ToInt32(sheet.Cells[i, 6].Value);
                                    else
                                    {
                                        error += " cột [Số lượng tồn] nhập sai kiểu dữ liệu, ";
                                    }

                                    int order;
                                    var unit_order = sheet.Cells[i, 7].Value.ToString();
                                    if (Int32.TryParse(unit_order, out order))
                                        product.units_on_order = Convert.ToInt32(sheet.Cells[i, 7].Value);
                                    else
                                    {
                                        error += " cột [Đ/v xuất bán] nhập sai kiểu dữ liệu, ";
                                    }

                                    int reorder;
                                    var reorder_level = sheet.Cells[i, 8].Value.ToString();
                                    if (Int32.TryParse(reorder_level, out reorder))
                                        product.reorder_level = Convert.ToInt32(sheet.Cells[i, 8].Value);
                                    else
                                    {
                                        error += " cột [Mức độ đặt hàng lại] nhập sai kiểu dữ liệu, ";
                                    }

                                    var discontinued = sheet.Cells[i, 9].Text as string;
                                    if (string.IsNullOrEmpty(discontinued))
                                    {
                                        product.discontinued = 0;
                                    }
                                    else if (!string.IsNullOrEmpty(discontinued))
                                    {
                                        if (discontinued.ToLower() == "x")
                                        {
                                            product.discontinued = 1;
                                        }
                                        else if (discontinued.ToLower() == "có" || discontinued.ToLower() == "không")
                                        {
                                            product.discontinued = sheet.Cells[i, 9].Value.ToString().ToLower() == "có" ? 1 : 0;
                                        }
                                        else
                                        {
                                            error += " cột [Giảm giá] nhập không đúng, ";
                                        }
                                    }

                                    if (_productRepository.IsExistProduct(product))
                                    {
                                        error = " Sản phẩm [" + product.product_name + "] đã tồn tại";
                                    }
                                    if (!string.IsNullOrEmpty(error))
                                    {
                                        error = $"Dòng [{i}]: " + error;
                                        listError.Add(error);
                                        continue;
                                    }
                                    _productRepository.SaveOrUpdate(product, uow);
                                }
                            }
                            if (listError.Count() > 0)
                            {
                                return Json(new { status = "error", errors = listError });
                            }

                            else
                            {
                                return Json(new { status = "success" });
                            }
                        }
                        else
                        {
                            return Json(new { message = "Định dạng file không đúng !" });
                        }
                    }
                    catch (Exception e)
                    {
                        return Json(new { message = "Lỗi nhập liệu, vui lòng kiểu tra lại !" });
                    }
                }
            }
        }

    }
}
