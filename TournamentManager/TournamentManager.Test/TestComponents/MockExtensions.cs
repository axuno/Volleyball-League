using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Moq.Language.Flow;
using TournamentManager.MultiTenancy;

namespace TournamentManager.Tests.TestComponents
{
    public static class MockExtensions
    {
        public static IReturnsResult<DbContext> SetupAppDb(this Mock<DbContext> dbCtxMock, Mock<MultiTenancy.AppDb> appDbMock)
        {
            return dbCtxMock.Setup(o => o.AppDb).Returns(appDbMock.Object);
        }
        
        public static IReturnsResult<ITenantContext> SetupDbContext(this Mock<ITenantContext> tenantCtxMock, Mock<MultiTenancy.DbContext> dbCtxMock)
        {
            return tenantCtxMock.Setup(o => o.DbContext).Returns(dbCtxMock.Object);
        }
    }
}
