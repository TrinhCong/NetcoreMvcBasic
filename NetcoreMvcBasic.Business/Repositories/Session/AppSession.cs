using Microsoft.Extensions.Options;
using Npgsql;
using NetcoreMvcBasic.Business.Models.Config;
using Smooth.IoC.UnitOfWork.Abstractions;
using Smooth.IoC.UnitOfWork.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetcoreMvcBasic.Business.Repositories.Session
{

    public interface IAppSession : Smooth.IoC.UnitOfWork.Interfaces.ISession
    {
    }

    public class AppSession : Session<NpgsqlConnection>, IAppSession
    {
        public AppSession(IDbFactory session, IOptions<ConnectionConfig> connectionConfig)
                : base(session, connectionConfig.Value.DefaultConnection)
        {
            Dapper.FastCrud.OrmConfiguration.DefaultDialect = Dapper.FastCrud.SqlDialect.PostgreSql;
        }

    }
}
