using Dapper.FastCrud;
using NetcoreMvcBasic.Business.Models;
using NetcoreMvcBasic.Business.Entities;
using NetcoreMvcBasic.Business.Repositories.Session;
using Smooth.IoC.Repository.UnitOfWork;
using Smooth.IoC.UnitOfWork.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetcoreMvcBasic.Business.Repositories
{
    public interface IProductRepository : IRepository<Product, int>
    {
        bool isExist(Product product);
        int GetTotalRecord(out int total);
        IEnumerable<Product> Filter(FilterParam param, out int totalProduct);

        bool IsExistProduct(Product prod);

        IEnumerable<Product> ExportData(FilterParam param);

    }

    public class ProductRepository : Repository<Product,int>, IProductRepository
    {
        public ProductRepository(IDbFactory factory) : base(factory)
        {

        }

        public int GetTotalRecord(out int total)
        {
            using (var session = Factory.Create<IAppSession>())
            {
                total = session.Count<Product>(p =>
                                                p.Include<Category>(join => join.LeftOuterJoin())
                                                .Include<Supplier>(join => join.LeftOuterJoin())
                                                );
                return total;
            }
        }

        private FilterParam filterAndExport(FilterParam param)
        {
            param.condition = "(1=1)";

            if (!string.IsNullOrEmpty(param.searchString))
                param.condition += $@" AND 
                                            (
                                            LOWER({Sql.Table<Product>()}.{nameof(Product.product_name)}) LIKE LOWER ('%{param.searchString}%') 
                                            OR LOWER({Sql.Table<Supplier>()}.{nameof(Supplier.company_name)}) LIKE LOWER ('%{param.searchString}%') 
                                            OR LOWER({Sql.Table<Category>()}.{nameof(Category.category_name)}) LIKE LOWER ('%{param.searchString}%')
                                            )
                                            ";

            if (param.minStock > 0 && param.maxStock > 0)
                param.condition += $" AND {Sql.Table<Product>()}.{nameof(Product.units_in_stock)} BETWEEN '{param.minStock}' AND '{param.maxStock}'";

            if (param.minStock > 0)
                param.condition += $" AND {Sql.Table<Product>()}.{nameof(Product.units_in_stock)} > '{param.minStock}'";

            if (param.maxStock > 0)
                param.condition += $" AND {Sql.Table<Product>()}.{nameof(Product.units_in_stock)} < '{param.maxStock}'";

            if (param.cateId > 0)
                param.condition += $" AND {Sql.Table<Product>()}.{nameof(Product.category_id)}= '{param.cateId }'";

            if (param.supId > 0)
                param.condition += $" AND {Sql.Table<Product>()}.{nameof(Product.supplier_id)}= '{param.supId }'";

            if (param.discount > 0)
                param.condition += $" AND {Sql.Table<Product>()}.{nameof(Product.discontinued)} = 1";

            if (param.discount < 0)
                param.condition += $" AND {Sql.Table<Product>()}.{nameof(Product.discontinued)} = 0";
            return param;
        }

        public IEnumerable<Product> Filter(FilterParam param, out int totalProduct)
        {
            using (var session = Factory.Create<IAppSession>())
            {
                try
                {
                    filterAndExport(param);
                    totalProduct = session.Count<Product>(p =>
                                                p.Include<Category>(join => join.LeftOuterJoin())
                                                .Include<Supplier>(join => join.LeftOuterJoin())
                                                .Where($"{param.condition}")
                                                );

                    var item = session.Find<Product>(p =>
                                                  p.Include<Category>(join => join.LeftOuterJoin())
                                                  .Include<Supplier>(join => join.LeftOuterJoin())
                                                  .Where($"{param.condition}")
                                                  .OrderBy($"{Sql.Table<Product>()}.{nameof(Product.product_id)}")
                                                  .Skip((param.page - 1) * param.pageSize)
                                                  .Top(param.pageSize)
                                                  );
                    return item;
                }
                catch (Exception)
                {
                    totalProduct = 0;
                    return new List<Product>();
                }
            }
        }

        public IEnumerable<Product> ExportData(FilterParam param)
        {
            using (var session = Factory.Create<IAppSession>())
            {
                try
                {
                    filterAndExport(param);

                    var item = session.Find<Product>(p =>
                                                 p.Include<Category>(join => join.LeftOuterJoin())
                                                 .Include<Supplier>(join => join.LeftOuterJoin())
                                                 .Where($"{param.condition}")
                                                 .OrderBy($"{Sql.Table<Product>()}.{nameof(Product.product_id)}")
                                                 );
                    return item;

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }



        public bool isExist(Product product)
        {
            using (var session = Factory.Create<IAppSession>())
            {
                var condition = $"{nameof(Product.product_name)}='{product.product_name}'";
                if (product.product_id > 0)
                {
                    condition += $"AND {nameof(Product.product_id)}<>'{product.product_id}'";
                }
                var existItem = session.Find<Product>(p => p.Where($"{condition}"));
                if (existItem.Count() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsExistProduct(Product prod)
        {
            using (var session = Factory.Create<IAppSession>())
            {
                var condition = $"{nameof(Product.product_name)}='{prod.product_name.Trim()}' and {nameof(Product.category_id)}={prod.category_id} and {nameof(Product.supplier_id)}={prod.supplier_id}";
                if (prod.product_id > 0)
                {
                    condition += $"and {nameof(Product.product_id)}<>{prod.product_id}";
                }
                var existItems = session.Find<Product>(p => p.Where($"{condition}"));
                if (existItems.Count() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
