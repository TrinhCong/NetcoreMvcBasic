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
    public interface ISupplierRepository : IRepository<Supplier,int>
    {
        IEnumerable<Supplier> GetListSupplier(int cateId);
    }
    
    public class SupplierRepository : Repository<Supplier,int>, ISupplierRepository
    {
        public SupplierRepository(IDbFactory factory) : base(factory)
        {

        }

        public IEnumerable<Supplier> GetListSupplier(int cateId)
        {
            using (var session = Factory.Create<IAppSession>())
            {
                try
                {
                    var items = session.Find<Supplier>(p => p
                                               .Include<Product>(j => j.LeftOuterJoin())
                                               .Include<Category>(j => j.LeftOuterJoin())
                                               .Where($"{Sql.Table<Product>()}.{nameof(Product.category_id)}='{cateId}'")).Distinct();
                    return items;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
