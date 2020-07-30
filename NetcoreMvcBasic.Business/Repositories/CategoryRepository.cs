using Dapper.FastCrud;
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
    public interface ICategoryRepository : IRepository<Category, int> 
    {
        bool isExist(Category cate);
        IEnumerable<Category> Filter(string search, int? page, int pageSize, out int totalCategory);
    }

    public class CategoryRepository : Repository<Category, int>, ICategoryRepository
    {
        public CategoryRepository(IDbFactory factory) : base(factory)
        {

        }

        public IEnumerable<Category> Filter(string search, int? page, int pageSize, out int totalCategory)
        {
            using (var session = Factory.Create<IAppSession>())
            {
                try
                {
                    var condition = "(1=1)";
                    if (!String.IsNullOrEmpty(search))
                    {
                        condition += $"AND LOWER({nameof(Category.category_name)}) LIKE LOWER('%{search.Trim()}%')";
                    }
                    totalCategory = session.Count<Category>(c => c.Where($"{condition}"));

                    var totalPage = (int)Math.Ceiling((double)totalCategory / pageSize);

                    var items = session.Find<Category>(c => c
                    .Where($"{condition}")
                    .OrderBy($"{Sql.Table<Category>()}.{nameof(Category.category_id)}")
                    .Skip((page - 1) * pageSize)
                    .Top(pageSize));

                    return items;
                }
                catch (Exception ex)
                {
                    totalCategory = 0;
                    return new List<Category>();
                }
            }
        }

        public bool isExist(Category cate)
        {
            using (var session = Factory.Create<IAppSession>())
            {
                var condition = $"{nameof(Category.category_name)}='{cate.category_name.Trim()}'";
                if (cate.category_id > 0)
                {
                    condition += $"AND {nameof(Category.category_id)}<>{cate.category_id}";
                }
                var items = session.Find<Category>(c => c.Where($"{condition}"));
                if (items.Count() > 0)
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
