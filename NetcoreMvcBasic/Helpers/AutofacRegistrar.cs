using Autofac;
using NetcoreMvcBasic.Attributes;
using NetcoreMvcBasic.Business.Repositories.Session;
using Smooth.IoC.Repository.UnitOfWork;
using Smooth.IoC.UnitOfWork.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NetcoreMvcBasic.Helpers
{
    public class AutofacRegistrar
    {
        public static void Register(ContainerBuilder builder)
        {
            builder.Register(c => new AutofacDbFactory(c.Resolve<IComponentContext>())).As<IDbFactory>().SingleInstance();
            builder.RegisterType<Smooth.IoC.UnitOfWork.UnitOfWork>().As<IUnitOfWork>();
            builder.RegisterType<AppSession>().As<IAppSession>();

            var dataAccess = Assembly.GetExecutingAssembly();

            builder.RegisterAssemblyTypes(dataAccess)
                   .Where(t => t.Name.EndsWith("Repository") || t.Name.StartsWith("Repository"))
                   .AsImplementedInterfaces();

            var assemblies = AssemblyHelper.GetReferencingAssemblies("NetcoreMvcBasic.Business");
            builder.RegisterAssemblyTypes(assemblies.ToArray()).AsClosedTypesOf(typeof(IRepository<,>));
        }
        [NoIoCFluentRegistration]
        sealed class AutofacDbFactory : IDbFactory
        {
            private readonly IComponentContext _container;
            public AutofacDbFactory(IComponentContext container)
            {
                _container = container;
            }
            public T Create<T>() where T : class, ISession
            {
                return _container.Resolve<T>();
            }
            public TUnitOfWork Create<TUnitOfWork, TSession>(IsolationLevel isolationLevel = IsolationLevel.Serializable) where TUnitOfWork : class, IUnitOfWork where TSession : class, ISession
            {
                return _container.Resolve<TUnitOfWork>(new NamedParameter("factory", _container.Resolve<IDbFactory>()),
                    new NamedParameter("session", Create<TSession>()), new NamedParameter(nameof(isolationLevel), isolationLevel)
                    , new NamedParameter("sessionOnlyForThisUnitOfWork", true));
            }
            public T Create<T>(IDbFactory factory, ISession session, IsolationLevel isolationLevel = IsolationLevel.Serializable) where T : class, IUnitOfWork
            {
                return _container.Resolve<T>(new NamedParameter(nameof(factory), factory),
                    new NamedParameter(nameof(session), session), new NamedParameter(nameof(isolationLevel), isolationLevel));
            }
            public void Release(IDisposable instance)
            {
                instance.Dispose();
            }
        }
    }
}

